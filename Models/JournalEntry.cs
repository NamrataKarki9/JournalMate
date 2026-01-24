using SQLite;

namespace JournalMaui.Models;

/// <summary>
/// Represents a single journal entry for one day.
/// Each date can have only one entry (enforced by DateKey unique index).
/// </summary>
public class JournalEntry
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    /// <summary>
    /// Unique date key in format "yyyy-MM-dd" - ensures one entry per day
    /// </summary>
    [Indexed(Unique = true)]
    public string DateKey { get; set; } = "";

    /// <summary>
    /// Title/summary of the journal entry
    /// </summary>
    public string Title { get; set; } = "";

    /// <summary>
    /// Main content of the journal entry (supports rich text/markdown)
    /// </summary>
    public string Content { get; set; } = "";

    /// <summary>
    /// System-generated timestamp when entry was created
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// System-generated timestamp when entry was last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    // ============ MOOD TRACKING ============

    /// <summary>
    /// Primary mood (REQUIRED) - one of: Happy, Excited, Relaxed, Grateful, Confident,
    /// Calm, Thoughtful, Curious, Nostalgic, Bored, Sad, Angry, Stressed, Lonely, Anxious
    /// </summary>
    public string PrimaryMood { get; set; } = "";

    /// <summary>
    /// Mood category for analytics: "Positive", "Neutral", or "Negative"
    /// </summary>
    public string MoodCategory { get; set; } = "";

    /// <summary>
    /// Optional secondary mood 1
    /// </summary>
    public string SecondaryMood1 { get; set; } = "";

    /// <summary>
    /// Optional secondary mood 2
    /// </summary>
    public string SecondaryMood2 { get; set; } = "";

    // ============ CATEGORIZATION ============

    /// <summary>
    /// Category for organizing entries (e.g., "Daily Reflection", "Work Notes")
    /// </summary>
    public string Category { get; set; } = "";

    /// <summary>
    /// Comma-separated list of tags (e.g., "Work,Health,Personal Growth")
    /// </summary>
    public string Tags { get; set; } = "";

    // ============ ANALYTICS ============

    /// <summary>
    /// Word count of the content for analytics
    /// </summary>
    public int WordCount { get; set; } = 0;

    // ============ HELPER METHODS ============

    /// <summary>
    /// Get tags as a list
    /// </summary>
    public List<string> GetTagsList()
    {
        if (string.IsNullOrWhiteSpace(Tags))
            return new List<string>();

        return Tags.Split(',', StringSplitOptions.RemoveEmptyEntries)
                   .Select(t => t.Trim())
                   .Where(t => !string.IsNullOrWhiteSpace(t))
                   .ToList();
    }

    /// <summary>
    /// Set tags from a list
    /// </summary>
    public void SetTagsList(List<string> tagList)
    {
        Tags = string.Join(",", tagList.Where(t => !string.IsNullOrWhiteSpace(t)));
    }

    /// <summary>
    /// Calculate word count from content
    /// </summary>
    public void UpdateWordCount()
    {
        if (string.IsNullOrWhiteSpace(Content))
        {
            WordCount = 0;
            return;
        }

        WordCount = Content.Split(new[] { ' ', '\n', '\r', '\t' },
                                   StringSplitOptions.RemoveEmptyEntries).Length;
    }
}
