using System.Security.Cryptography;
using System.Text;
using JournalMaui.Models;

namespace JournalMate.Services;

/// <summary>
/// Authentication service for PIN-based security.
/// Handles PIN setup, verification, and lockout logic.
/// </summary>
public class AuthService
{
    private readonly JournalDatabase _database;
    private const int MaxFailedAttempts = 5;
    private static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(5);

    public AuthService(JournalDatabase database)
    {
        _database = database;
    }

    /// <summary>
    /// Check if PIN has been set up
    /// </summary>
    public async Task<bool> IsPinSetupAsync()
    {
        return await _database.HasUserSetupAsync();
    }

    /// <summary>
    /// Set up a new PIN (first time)
    /// </summary>
    public async Task<bool> SetupPinAsync(string pin, string displayName, string email)
    {
        if (string.IsNullOrWhiteSpace(pin) || pin.Length != 4)
            return false;

        // Check if PIN is numeric
        if (!pin.All(char.IsDigit))
            return false;

        var salt = GenerateSalt();
        var hash = HashPin(pin, salt);

        var user = new User
        {
            DisplayName = displayName,
            Email = email,
            PinHash = hash,
            Salt = salt,
            CreatedAt = DateTime.Now,
            LastLoginAt = DateTime.Now,
            FailedAttempts = 0
        };

        await _database.SaveUserAsync(user);
        return true;
    }

    /// <summary>
    /// Verify PIN and handle lockout
    /// </summary>
    public async Task<(bool success, string message, string displayName)> VerifyPinAsync(string pin)
    {
        var user = await _database.GetUserAsync();

        if (user == null)
            return (false, "No PIN has been set up.", "");

        // Check if locked out
        if (user.LockedUntil.HasValue && user.LockedUntil > DateTime.Now)
        {
            var remainingTime = user.LockedUntil.Value - DateTime.Now;
            return (false, $"Account locked. Try again in {remainingTime.Minutes}m {remainingTime.Seconds}s", "");
        }

        // Clear lockout if expired
        if (user.LockedUntil.HasValue && user.LockedUntil <= DateTime.Now)
        {
            user.LockedUntil = null;
            user.FailedAttempts = 0;
        }

        // Verify PIN
        var hash = HashPin(pin, user.Salt);

        if (hash == user.PinHash)
        {
            // Success - reset failed attempts
            user.FailedAttempts = 0;
            user.LockedUntil = null;
            user.LastLoginAt = DateTime.Now;
            await _database.SaveUserAsync(user);
            return (true, "Login successful!", user.DisplayName);
        }
        else
        {
            // Failed attempt
            user.FailedAttempts++;

            if (user.FailedAttempts >= MaxFailedAttempts)
            {
                user.LockedUntil = DateTime.Now.Add(LockoutDuration);
                await _database.SaveUserAsync(user);
                return (false, $"Too many failed attempts. Account locked for {LockoutDuration.Minutes} minutes.", "");
            }

            await _database.SaveUserAsync(user);
            var remaining = MaxFailedAttempts - user.FailedAttempts;
            return (false, $"Incorrect PIN. {remaining} attempts remaining.", "");
        }
    }

    /// <summary>
    /// Change PIN (requires current PIN)
    /// </summary>
    public async Task<(bool success, string message)> ChangePinAsync(string currentPin, string newPin)
    {
        var verifyResult = await VerifyPinAsync(currentPin);

        if (!verifyResult.success)
            return (false, verifyResult.message);

        if (string.IsNullOrWhiteSpace(newPin) || newPin.Length != 4)
            return (false, "New PIN must be exactly 4 digits.");

        if (!newPin.All(char.IsDigit))
            return (false, "PIN must contain only numbers.");

        var user = await _database.GetUserAsync();
        if (user == null)
            return (false, "User not found.");

        user.Salt = GenerateSalt();
        user.PinHash = HashPin(newPin, user.Salt);
        await _database.SaveUserAsync(user);

        return (true, "PIN changed successfully!");
    }

    /// <summary>
    /// Verify if the provided email matches the user's email
    /// </summary>
    public async Task<bool> VerifyEmailAsync(string email)
    {
        var user = await _database.GetUserAsync();
        if (user == null || string.IsNullOrWhiteSpace(user.Email)) return false;
        return string.Equals(user.Email, email, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Force set a new PIN (used after recovery)
    /// </summary>
    public async Task<bool> OverwritePinAsync(string newPin)
    {
        if (string.IsNullOrWhiteSpace(newPin) || newPin.Length != 4 || !newPin.All(char.IsDigit))
            return false;

        var user = await _database.GetUserAsync();
        if (user == null) return false;

        user.Salt = GenerateSalt();
        user.PinHash = HashPin(newPin, user.Salt);
        user.FailedAttempts = 0;
        user.LockedUntil = null;
        await _database.SaveUserAsync(user);
        return true;
    }

    /// <summary>
    /// Update user's display name
    /// </summary>
    public async Task UpdateDisplayNameAsync(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
            return;

        var user = await _database.GetUserAsync();
        if (user != null)
        {
            user.DisplayName = newName;
            await _database.SaveUserAsync(user);
        }
    }

    /// <summary>
    /// Reset PIN (for development/testing only - would need recovery in production)
    /// </summary>
    public async Task ResetPinAsync()
    {
        var user = await _database.GetUserAsync();
        if (user != null)
        {
            user.PinHash = "";
            user.Salt = "";
            user.FailedAttempts = 0;
            user.LockedUntil = null;
            await _database.SaveUserAsync(user);
        }
    }

    // ============ PRIVATE HELPERS ============

    private static string GenerateSalt()
    {
        var saltBytes = new byte[16];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(saltBytes);
        }
        return Convert.ToBase64String(saltBytes);
    }

    private static string HashPin(string pin, string salt)
    {
        using (var sha256 = SHA256.Create())
        {
            var combined = pin + salt;
            var bytes = Encoding.UTF8.GetBytes(combined);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }
    }
}
