namespace JournalMaui.Models;

/// <summary>
/// Static class containing all predefined tags and categories.
/// Users can also create custom tags.
/// </summary>
public static class TagData
{
    // ============ PRE-BUILT TAGS ============
    public static readonly List<string> PreBuiltTags = new()
    {
        // Work & Career
        "Work",
        "Career",
        "Studies",
        "Projects",
        "Planning",
        
        // Relationships
        "Family",
        "Friends",
        "Relationships",
        "Parenting",
        
        // Health & Wellness
        "Health",
        "Fitness",
        "Exercise",
        "Meditation",
        "Yoga",
        "Self-care",
        
        // Personal Development
        "Personal Growth",
        "Reflection",
        "Reading",
        "Writing",
        
        // Lifestyle
        "Hobbies",
        "Travel",
        "Nature",
        "Music",
        "Cooking",
        "Shopping",
        
        // Special Occasions
        "Birthday",
        "Holiday",
        "Vacation",
        "Celebration",
        
        // Other
        "Finance",
        "Spirituality"
    };

    // ============ JOURNAL CATEGORIES ============
    public static readonly List<string> Categories = new()
    {
        "Daily Reflection",
        "Work Notes",
        "Personal Goals",
        "Gratitude",
        "Dreams",
        "Ideas",
        "Travel Log",
        "Health Log",
        "Learning Notes",
        "Creative Writing"
    };

    /// <summary>
    /// Get tags that match a search query (for autocomplete)
    /// </summary>
    public static List<string> SearchTags(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return PreBuiltTags;

        return PreBuiltTags
            .Where(t => t.Contains(query, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    /// <summary>
    /// Check if a tag exists in pre-built tags
    /// </summary>
    public static bool IsPreBuiltTag(string tag)
    {
        return PreBuiltTags.Contains(tag, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Get color for a tag (consistent colors for visualization)
    /// </summary>
    public static string GetTagColor(string tag)
    {
        // Use hash code to generate consistent color
        var hash = Math.Abs(tag.GetHashCode());
        var colors = new[]
        {
            "#8b5cf6", // Purple
            "#06b6d4", // Cyan
            "#f59e0b", // Amber
            "#10b981", // Emerald
            "#f43f5e", // Rose
            "#6366f1", // Indigo
            "#14b8a6", // Teal
            "#f97316", // Orange
            "#84cc16", // Lime
            "#ec4899"  // Pink
        };

        return colors[hash % colors.Length];
    }
}
