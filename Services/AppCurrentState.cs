using Microsoft.Maui.Storage;

namespace JournalMate.Services
{
    /// <summary>
    /// Manages global application state including authentication and theme.
    /// Uses MAUI Preferences for persistence across app restarts.
    /// </summary>
    public class AppCurrentState
    {
        private const string ThemePreferenceKey = "IsDarkMode";
        private const string IsLoggedInKey = "IsLoggedIn";

        /// <summary>
        /// Whether user is currently authenticated
        /// </summary>
        public bool IsLoggedIn { get; private set; } = false;

        /// <summary>
        /// Whether dark mode is enabled
        /// </summary>
        public bool IsDarkMode { get; private set; }

        /// <summary>
        /// Global mood selected by the user (shared across dashboard and diary)
        /// </summary>
        public string GlobalMood { get; private set; } = "";

        /// <summary>
        /// Path to the user's profile picture
        /// </summary>
        public string ProfilePicturePath { get; private set; }

        /// <summary>
        /// Current user's display name (optional)
        /// </summary>
        public string UserDisplayName { get; set; } = "Journal User";

        /// <summary>
        /// Event fired when any state changes (for UI updates like theme, auth)
        /// </summary>
        public event Action? OnChange;

        /// <summary>
        /// Event fired specifically when database data changes
        /// </summary>
        public event Action? OnDataChanged;

        public AppCurrentState()
        {
            IsDarkMode = Preferences.Default.Get(ThemePreferenceKey, false);
            ProfilePicturePath = Preferences.Default.Get("ProfilePicturePath", "");
            UserDisplayName = Preferences.Default.Get("UserDisplayName", "Journal User");
        }

        /// <summary>
        /// Mark user as authenticated
        /// </summary>
        public void AuthenticateUser(string displayName)
        {
            IsLoggedIn = true;
            if (!string.IsNullOrWhiteSpace(displayName))
            {
                UserDisplayName = displayName;
                Preferences.Default.Set("UserDisplayName", UserDisplayName);
            }
            BroadcastStateUpdate();
        }

        /// <summary>
        /// Log out user
        /// </summary>
        public void RevokeAuthentication()
        {
            IsLoggedIn = false;
            BroadcastStateUpdate();
        }

        /// <summary>
        /// Check if user needs to log in
        /// </summary>
        public bool RequiresAuthentication()
        {
            return !IsLoggedIn;
        }

        /// <summary>
        /// Toggle between light and dark mode
        /// </summary>
        public void SwitchThemeMode()
        {
            IsDarkMode = !IsDarkMode;
            Preferences.Default.Set(ThemePreferenceKey, IsDarkMode);
            BroadcastStateUpdate();
        }

        /// <summary>
        /// Set theme explicitly
        /// </summary>
        public void SetTheme(bool isDarkMode)
        {
            IsDarkMode = isDarkMode;
            Preferences.Default.Set(ThemePreferenceKey, IsDarkMode);
            BroadcastStateUpdate();
        }

        public void UpdateUserDisplayName(string newName)
        {
            if (!string.IsNullOrWhiteSpace(newName))
            {
                UserDisplayName = newName;
                Preferences.Default.Set("UserDisplayName", UserDisplayName);
                BroadcastStateUpdate();
            }
        }

        public void SetGlobalMood(string mood)
        {
            GlobalMood = mood;
            BroadcastStateUpdate();
        }

        /// <summary>
        /// Update the profile picture path and persist it
        /// </summary>
        public void UpdateProfilePicture(string path)
        {
            ProfilePicturePath = path;
            Preferences.Default.Set("ProfilePicturePath", path);
            BroadcastStateUpdate();
        }

        /// <summary>
        /// Helper to get a displayable path for the WebView (handles converting to Base64 to bypass file access issues)
        /// </summary>
        public string ProfilePictureDisplayPath
        {
            get
            {
                if (!string.IsNullOrEmpty(ProfilePicturePath) && File.Exists(ProfilePicturePath))
                {
                    try
                    {
                        // Use Base64 to avoid WebView local file access restrictions
                        var bytes = File.ReadAllBytes(ProfilePicturePath);
                        var base64 = Convert.ToBase64String(bytes);
                        var mimeType = "image/jpeg"; // Default fallback
                        if (ProfilePicturePath.EndsWith(".png", StringComparison.OrdinalIgnoreCase)) mimeType = "image/png";
                        if (ProfilePicturePath.EndsWith(".gif", StringComparison.OrdinalIgnoreCase)) mimeType = "image/gif";

                        return $"data:{mimeType};base64,{base64}";
                    }
                    catch
                    {
                        // Fallback will be hit below
                    }
                }

                // Default profile picture
                return "images/Nammu.jpg";
            }
        }

        private void BroadcastStateUpdate() => OnChange?.Invoke();

        /// <summary>
        /// Notify subcribers that database data has changed
        /// </summary>
        public void NotifyDataChanged() => OnDataChanged?.Invoke();

        /// <summary>
        /// Wipe all user preferences and local profile pictures
        /// </summary>
        public void ResetAllData()
        {
            try
            {
                // Delete local profile pictures if they exist in AppData
                if (!string.IsNullOrEmpty(ProfilePicturePath) && File.Exists(ProfilePicturePath))
                {
                    File.Delete(ProfilePicturePath);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting profile picture during reset: {ex.Message}");
            }

            // Clear all MAUI Preferences
            Preferences.Default.Clear();

            // Reset local state
            IsLoggedIn = false;
            IsDarkMode = false;
            UserDisplayName = "Journal User";
            ProfilePicturePath = "";
            GlobalMood = "";

            BroadcastStateUpdate();
        }
    }
}
