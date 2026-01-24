using SQLite;
using JournalMaui.Models;

namespace JournalMaui.Services;

/// <summary>
/// Database service for all journal-related CRUD operations.
/// Uses SQLite for local, offline-first data storage.
/// </summary>
public class JournalDatabase
{
    private SQLiteAsyncConnection _connection;
    private readonly string _dbPath;
    private readonly JournalMate.Services.AppCurrentState _appState;

    public JournalDatabase(string dbPath, JournalMate.Services.AppCurrentState appState)
    {
        _dbPath = dbPath;
        _connection = new SQLiteAsyncConnection(dbPath);
        _appState = appState;
    }

    /// <summary>
    /// Initialize database tables
    /// </summary>
    public async Task InitAsync()
    {
        await _connection.CreateTableAsync<JournalEntry>();
        await _connection.CreateTableAsync<User>();
    }


    private static string FormatDateKey(DateTime date) => date.Date.ToString("yyyy-MM-dd");


    /// <summary>
    /// Get entry by date (one per day)
    /// </summary>
    public async Task<JournalEntry?> GetByDateAsync(DateTime date)
    {
        var formattedKey = FormatDateKey(date);
        return await _connection.Table<JournalEntry>()
                  .Where(x => x.DateKey == formattedKey)
                  .FirstOrDefaultAsync();
    }

    /// <summary>
    /// Get entry by ID
    /// </summary>
    public async Task<JournalEntry?> GetByIdAsync(int id)
    {
        return await _connection.Table<JournalEntry>()
                  .Where(x => x.Id == id)
                  .FirstOrDefaultAsync();
    }

    /// <summary>
    /// Save or update entry for a date (upsert)
    /// </summary>
    public async Task SaveAsync(JournalEntry entry)
    {
        var formattedKey = FormatDateKey(DateTime.Parse(entry.DateKey));
        var timestamp = DateTime.Now;

        // Ensure word count is updated
        entry.UpdateWordCount();

        // Set mood category based on primary mood
        if (!string.IsNullOrWhiteSpace(entry.PrimaryMood))
        {
            entry.MoodCategory = MoodData.GetMoodCategory(entry.PrimaryMood);
        }

        var existingEntry = await _connection.Table<JournalEntry>()
                                .Where(x => x.DateKey == formattedKey)
                                .FirstOrDefaultAsync();

        if (existingEntry is null)
        {
            entry.DateKey = formattedKey;
            entry.CreatedAt = timestamp;
            entry.UpdatedAt = timestamp;
            await _connection.InsertAsync(entry);
        }
        else
        {
            existingEntry.Title = entry.Title;
            existingEntry.Content = entry.Content;
            existingEntry.PrimaryMood = entry.PrimaryMood;
            existingEntry.MoodCategory = entry.MoodCategory;
            existingEntry.SecondaryMood1 = entry.SecondaryMood1;
            existingEntry.SecondaryMood2 = entry.SecondaryMood2;
            existingEntry.Category = entry.Category;
            existingEntry.Tags = entry.Tags;
            existingEntry.WordCount = entry.WordCount;
            existingEntry.UpdatedAt = timestamp;
            await _connection.UpdateAsync(existingEntry);
        }

        _appState.NotifyDataChanged();
    }

    /// <summary>
    /// Legacy save method for backwards compatibility
    /// </summary>
    public async Task SaveAsync(DateTime date, string title, string content)
    {
        var entry = new JournalEntry
        {
            DateKey = FormatDateKey(date),
            Title = title,
            Content = content
        };
        await SaveAsync(entry);
    }

    /// <summary>
    /// Delete entry by date
    /// </summary>
    public async Task<int> DeleteAsync(DateTime date)
    {
        var formattedKey = FormatDateKey(date);
        var entryToDelete = await _connection.Table<JournalEntry>()
                                .Where(x => x.DateKey == formattedKey)
                                .FirstOrDefaultAsync();

        if (entryToDelete != null)
        {
            var result = await _connection.DeleteAsync(entryToDelete);
            _appState.NotifyDataChanged();
            return result;
        }

        return 0;
    }

    /// <summary>
    /// Delete entry by ID (more robust for multi-entry scenarios or UI lists)
    /// </summary>
    public async Task<int> DeleteByIdAsync(int id)
    {
        Console.WriteLine($"[JournalDatabase] DeleteByIdAsync called for ID: {id}");
        var entryToDelete = await _connection.Table<JournalEntry>()
                                .Where(x => x.Id == id)
                                .FirstOrDefaultAsync();

        if (entryToDelete != null)
        {
            Console.WriteLine($"[JournalDatabase] Found entry to delete: {entryToDelete.Title} (Date: {entryToDelete.DateKey})");
            var result = await _connection.DeleteAsync(entryToDelete);
            Console.WriteLine($"[JournalDatabase] Delete result: {result}");
            if (result > 0)
            {
                _appState.NotifyDataChanged();
                return result;
            }
        }
        else
        {
            Console.WriteLine($"[JournalDatabase] No entry found with ID: {id}");
        }

        return 0;
    }


    /// <summary>
    /// Get all entries ordered by date descending
    /// </summary>
    public async Task<List<JournalEntry>> GetAllEntriesAsync()
    {
        return await _connection.Table<JournalEntry>()
                        .OrderByDescending(x => x.DateKey)
                        .ToListAsync();
    }

    /// <summary>
    /// Get recent entries with limit
    /// </summary>
    public async Task<List<JournalEntry>> GetRecentAsync(int take = 20)
    {
        return await _connection.Table<JournalEntry>()
                        .OrderByDescending(x => x.UpdatedAt)
                        .Take(take)
                        .ToListAsync();
    }

    /// <summary>
    /// Get entries by date range
    /// </summary>
    public async Task<List<JournalEntry>> GetEntriesByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        var startKey = FormatDateKey(startDate);
        var endKey = FormatDateKey(endDate);

        var sql = "SELECT * FROM JournalEntry WHERE DateKey >= ? AND DateKey <= ? ORDER BY DateKey DESC";
        return await _connection.QueryAsync<JournalEntry>(sql, startKey, endKey);
    }

    /// <summary>
    /// Get entries by mood category
    /// </summary>
    public async Task<List<JournalEntry>> GetEntriesByMoodCategoryAsync(string moodCategory)
    {
        return await _connection.Table<JournalEntry>()
                        .Where(x => x.MoodCategory == moodCategory)
                        .OrderByDescending(x => x.DateKey)
                        .ToListAsync();
    }

    /// <summary>
    /// Get entries by specific mood
    /// </summary>
    public async Task<List<JournalEntry>> GetEntriesByMoodAsync(string mood)
    {
        return await _connection.Table<JournalEntry>()
                        .Where(x => x.PrimaryMood == mood ||
                                    x.SecondaryMood1 == mood ||
                                    x.SecondaryMood2 == mood)
                        .OrderByDescending(x => x.DateKey)
                        .ToListAsync();
    }

    /// <summary>
    /// Search entries by title or content
    /// </summary>
    public async Task<List<JournalEntry>> SearchEntriesAsync(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return await GetAllEntriesAsync();

        var lowerQuery = query.ToLower();
        var allEntries = await GetAllEntriesAsync();

        return allEntries.Where(x =>
            x.Title.ToLower().Contains(lowerQuery) ||
            x.Content.ToLower().Contains(lowerQuery))
            .ToList();
    }

    /// <summary>
    /// Get entries count
    /// </summary>
    public async Task<int> GetEntryCountAsync()
    {
        return await _connection.Table<JournalEntry>().CountAsync();
    }

    /// <summary>
    /// Get all dates that have entries (for calendar)
    /// </summary>
    public async Task<List<string>> GetAllEntryDatesAsync()
    {
        var entries = await _connection.Table<JournalEntry>()
                              .ToListAsync();
        return entries.Select(x => x.DateKey).ToList();
    }


    /// <summary>
    /// Get mood distribution (count per category)
    /// </summary>
    public async Task<Dictionary<string, int>> GetMoodDistributionAsync()
    {
        var entries = await GetAllEntriesAsync();

        return new Dictionary<string, int>
        {
            { "Positive", entries.Count(e => e.MoodCategory == "Positive") },
            { "Neutral", entries.Count(e => e.MoodCategory == "Neutral") },
            { "Negative", entries.Count(e => e.MoodCategory == "Negative") }
        };
    }

    /// <summary>
    /// Get most frequent primary mood
    /// </summary>
    public async Task<string> GetMostFrequentMoodAsync()
    {
        var entries = await GetAllEntriesAsync();

        if (entries.Count == 0)
            return "";

        return entries
            .Where(e => !string.IsNullOrWhiteSpace(e.PrimaryMood))
            .GroupBy(e => e.PrimaryMood)
            .OrderByDescending(g => g.Count())
            .FirstOrDefault()?.Key ?? "";
    }

    /// <summary>
    /// Get tag frequency (count per tag)
    /// </summary>
    public async Task<Dictionary<string, int>> GetTagFrequencyAsync()
    {
        var entries = await GetAllEntriesAsync();
        var tagCounts = new Dictionary<string, int>();

        foreach (var entry in entries)
        {
            foreach (var tag in entry.GetTagsList())
            {
                if (tagCounts.ContainsKey(tag))
                    tagCounts[tag]++;
                else
                    tagCounts[tag] = 1;
            }
        }

        return tagCounts.OrderByDescending(x => x.Value)
                        .ToDictionary(x => x.Key, x => x.Value);
    }

    /// <summary>
    /// Get average word count
    /// </summary>
    public async Task<double> GetAverageWordCountAsync()
    {
        var entries = await GetAllEntriesAsync();

        if (entries.Count == 0)
            return 0;

        return entries.Average(e => e.WordCount);
    }

    /// <summary>
    /// Get total word count
    /// </summary>
    public async Task<int> GetTotalWordCountAsync()
    {
        var entries = await GetAllEntriesAsync();
        return entries.Sum(e => e.WordCount);
    }


    /// <summary>
    /// Calculate current journaling streak
    /// </summary>
    public async Task<int> GetCurrentStreakAsync()
    {
        var entryDates = await GetAllEntryDatesAsync();

        if (entryDates.Count == 0)
            return 0;

        var sortedDates = entryDates
            .Select(d => DateTime.Parse(d).Date)
            .OrderByDescending(d => d)
            .ToList();

        var today = DateTime.Today;
        var streak = 0;
        var checkDate = today;

        // Check if today has an entry, or if yesterday has (allow for current day)
        if (!sortedDates.Contains(today) && !sortedDates.Contains(today.AddDays(-1)))
            return 0;

        // Start from most recent entry date
        if (!sortedDates.Contains(today))
            checkDate = today.AddDays(-1);

        while (sortedDates.Contains(checkDate))
        {
            streak++;
            checkDate = checkDate.AddDays(-1);
        }

        return streak;
    }

    /// <summary>
    /// Calculate longest streak ever
    /// </summary>
    public async Task<int> GetLongestStreakAsync()
    {
        var entryDates = await GetAllEntryDatesAsync();

        if (entryDates.Count == 0)
            return 0;

        var sortedDates = entryDates
            .Select(d => DateTime.Parse(d).Date)
            .OrderBy(d => d)
            .ToList();

        var longestStreak = 1;
        var currentStreak = 1;

        for (int i = 1; i < sortedDates.Count; i++)
        {
            if (sortedDates[i] == sortedDates[i - 1].AddDays(1))
            {
                currentStreak++;
                longestStreak = Math.Max(longestStreak, currentStreak);
            }
            else
            {
                currentStreak = 1;
            }
        }

        return longestStreak;
    }

    /// <summary>
    /// Get missed days in the last N days
    /// </summary>
    public async Task<List<DateTime>> GetMissedDaysAsync(int lastNDays = 30)
    {
        var entryDates = await GetAllEntryDatesAsync();
        var entryDateSet = new HashSet<DateTime>(
            entryDates.Select(d => DateTime.Parse(d).Date));

        var missedDays = new List<DateTime>();
        var today = DateTime.Today;

        for (int i = 0; i < lastNDays; i++)
        {
            var checkDate = today.AddDays(-i);
            if (!entryDateSet.Contains(checkDate))
            {
                missedDays.Add(checkDate);
            }
        }

        return missedDays;
    }

    /// <summary>
    /// Get entries for current week
    /// </summary>
    public async Task<int> GetEntriesThisWeekAsync()
    {
        var today = DateTime.Today;
        var startOfWeek = today.AddDays(-(int)today.DayOfWeek);
        var entries = await GetEntriesByDateRangeAsync(startOfWeek, today);
        return entries.Count;
    }

    /// <summary>
    /// Get entries for current month
    /// </summary>
    public async Task<int> GetEntriesThisMonthAsync()
    {
        var today = DateTime.Today;
        var startOfMonth = new DateTime(today.Year, today.Month, 1);
        var entries = await GetEntriesByDateRangeAsync(startOfMonth, today);
        return entries.Count;
    }


    /// <summary>
    /// Get the user (single user app)
    /// </summary>
    public async Task<User?> GetUserAsync()
    {
        return await _connection.Table<User>().FirstOrDefaultAsync();
    }

    /// <summary>
    /// Create or update user
    /// </summary>
    public async Task SaveUserAsync(User user)
    {
        var existingUser = await GetUserAsync();

        if (existingUser == null)
        {
            user.CreatedAt = DateTime.Now;
            await _connection.InsertAsync(user);
        }
        else
        {
            user.Id = existingUser.Id;
            user.CreatedAt = existingUser.CreatedAt;
            await _connection.UpdateAsync(user);
        }
    }

    /// <summary>
    /// Check if user has set up PIN
    /// </summary>
    public async Task<bool> HasUserSetupAsync()
    {
        var user = await GetUserAsync();
        return user != null && !string.IsNullOrWhiteSpace(user.PinHash);
    }

    /// <summary>
    /// Deletes the database file and re-initializes the connection.
    /// This is used for a full factory reset.
    /// </summary>
    public async Task WipeDatabaseAsync()
    {
        try
        {
            await _connection.CloseAsync();

            if (File.Exists(_dbPath))
            {
                File.Delete(_dbPath);
            }

            // Re-create the connection object for a fresh start
            _connection = new SQLiteAsyncConnection(_dbPath);
            await InitAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[JournalDatabase] Error during WipeDatabaseAsync: {ex.Message}");
            // Attempt to re-establish connection even if delete failed
            _connection = new SQLiteAsyncConnection(_dbPath);
            throw;
        }
    }
}
