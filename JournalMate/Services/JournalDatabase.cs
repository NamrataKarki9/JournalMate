using SQLite;
using JournalMaui.Models;

namespace JournalMaui.Services;

public class JournalDatabase
{
    private readonly SQLiteAsyncConnection _connection;

    public JournalDatabase(string dbPath)
    {
        _connection = new SQLiteAsyncConnection(dbPath);    
    }

    public async Task InitAsync()
    {
        await _connection.CreateTableAsync<JournalEntry>();
    }

    private static string FormatDateKey(DateTime date) => date.Date.ToString("yyyy-MM-dd");

    // One-per-day: load by DateKey only
    public Task<JournalEntry?> GetByDateAsync(DateTime date)
    {
        var formattedKey = FormatDateKey(date);
        return _connection.Table<JournalEntry>()
                  .Where(x => x.DateKey == formattedKey)
                  .FirstOrDefaultAsync();
    }

    // One-per-day: save/upsert by DateKey only (renaming title updates same row)
    public async Task SaveAsync(DateTime date, string title, string content)
    {
        var formattedKey = FormatDateKey(date);
        var timestamp = DateTime.Now;

        title = (title ?? "").Trim();
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title is required.", nameof(title));

        var currentEntry = await _connection.Table<JournalEntry>()
                                .Where(x => x.DateKey == formattedKey)
                                .FirstOrDefaultAsync();

        if (currentEntry is null)
        {
            var newEntry = new JournalEntry
            {
                DateKey = formattedKey,
                Title = title,
                Content = content ?? "",
                CreatedAt = timestamp,
                UpdatedAt = timestamp
            };
            await _connection.InsertAsync(newEntry);
        }
        else
        {
            // Update SAME row (no new row)
            currentEntry.Title = title;
            currentEntry.Content = content ?? "";
            currentEntry.UpdatedAt = timestamp;
            await _connection.UpdateAsync(currentEntry);
        }
    }

    // One-per-day: delete by DateKey only
    public async Task<int> DeleteAsync(DateTime date)
    {
        var formattedKey = FormatDateKey(date);
        
        Console.WriteLine($"[Database.DeleteAsync] Looking for DateKey: '{formattedKey}'");
        
        // First, find the entry
        var entryToDelete = await _connection.Table<JournalEntry>()
                                .Where(x => x.DateKey == formattedKey)
                                .FirstOrDefaultAsync();
        
        if (entryToDelete != null)
        {
            Console.WriteLine($"[Database.DeleteAsync] Found entry - Id: {entryToDelete.Id}, Title: '{entryToDelete.Title}'");
            var result = await _connection.DeleteAsync(entryToDelete);
            Console.WriteLine($"[Database.DeleteAsync] Delete result: {result}");
            return result;
        }
        
        Console.WriteLine($"[Database.DeleteAsync] No entry found with DateKey: '{formattedKey}'");
        
        // Debug: List all DateKeys in database
        var allEntries = await _connection.Table<JournalEntry>().ToListAsync();
        Console.WriteLine($"[Database.DeleteAsync] All DateKeys in database:");
        foreach (var entry in allEntries)
        {
            Console.WriteLine($"  - '{entry.DateKey}' (Id: {entry.Id})");
        }
        
        return 0;
    }

    public async Task<List<JournalEntry>> GetRecentAsync(int take = 20)
    {
        return await _connection.Table<JournalEntry>()
                        .OrderByDescending(x => x.UpdatedAt)
                        .Take(take)
                        .ToListAsync();
    }
}
