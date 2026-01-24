namespace JournalMaui.Models;

/// <summary>
/// Static class containing all predefined moods organized by category.
/// Used throughout the app for mood selection and analytics.
/// </summary>
public static class MoodData
{
    // ============ POSITIVE MOODS ============
    public static readonly List<string> Positive = new()
    {
        "Happy",
        "Excited",
        "Relaxed",
        "Grateful",
        "Confident"
    };

    // ============ NEUTRAL MOODS ============
    public static readonly List<string> Neutral = new()
    {
        "Calm",
        "Thoughtful",
        "Curious",
        "Nostalgic",
        "Bored"
    };

    // ============ NEGATIVE MOODS ============
    public static readonly List<string> Negative = new()
    {
        "Sad",
        "Angry",
        "Stressed",
        "Lonely",
        "Anxious"
    };

    /// <summary>
    /// Get all moods as a flat list
    /// </summary>
    public static List<string> GetAllMoods()
    {
        var allMoods = new List<string>();
        allMoods.AddRange(Positive);
        allMoods.AddRange(Neutral);
        allMoods.AddRange(Negative);
        return allMoods;
    }

    /// <summary>
    /// Get the category (Positive/Neutral/Negative) for a given mood
    /// </summary>
    public static string GetMoodCategory(string mood)
    {
        if (string.IsNullOrWhiteSpace(mood))
            return "";

        if (Positive.Contains(mood))
            return "Positive";
        if (Neutral.Contains(mood))
            return "Neutral";
        if (Negative.Contains(mood))
            return "Negative";

        return "";
    }

    /// <summary>
    /// Get emoji for a mood
    /// </summary>
    public static string GetMoodEmoji(string mood)
    {
        return mood switch
        {
            // Positive
            "Happy" => "😊",
            "Excited" => "🤩",
            "Relaxed" => "😌",
            "Grateful" => "🙏",
            "Confident" => "💪",
            // Neutral
            "Calm" => "😐",
            "Thoughtful" => "🤔",
            "Curious" => "🧐",
            "Nostalgic" => "💭",
            "Bored" => "😑",
            // Negative
            "Sad" => "😢",
            "Angry" => "😠",
            "Stressed" => "😰",
            "Lonely" => "😔",
            "Anxious" => "😟",
            _ => "📝"
        };
    }

    /// <summary>
    /// Get color for mood category (for charts and UI)
    /// </summary>
    public static string GetCategoryColor(string category)
    {
        return category switch
        {
            "Positive" => "#22c55e", // Green
            "Neutral" => "#3b82f6",  // Blue
            "Negative" => "#ef4444", // Red
            _ => "#6b7280"           // Gray
        };
    }
}
