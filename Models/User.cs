using SQLite;

namespace JournalMaui.Models;

/// <summary>
/// User model for PIN-based authentication.
/// Only one user per device (single-user app).
/// </summary>
public class User
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    /// <summary>
    /// User's display name
    /// </summary>
    public string DisplayName { get; set; } = "Journal User";

    /// <summary>
    /// User's email for recovery
    /// </summary>
    public string Email { get; set; } = "";

    /// <summary>
    /// Hashed PIN for security (using SHA256)
    /// </summary>
    public string PinHash { get; set; } = "";

    /// <summary>
    /// Salt used for PIN hashing
    /// </summary>
    public string Salt { get; set; } = "";

    /// <summary>
    /// When the user account was created
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Last successful login time
    /// </summary>
    public DateTime LastLoginAt { get; set; }

    /// <summary>
    /// Number of failed login attempts (for lockout)
    /// </summary>
    public int FailedAttempts { get; set; } = 0;

    /// <summary>
    /// When the account was locked (if applicable)
    /// </summary>
    public DateTime? LockedUntil { get; set; }
}
