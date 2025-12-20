using System;
using Microsoft.Maui.Storage; // Preferences for MAUI

namespace JournalMate.Services
{
    public class AppCurrentState
    {
        private const string ThemePreferenceKey = "IsDarkMode";

        public bool IsLoggedIn { get; private set; } = false;

        // Global dark mode flag
        public bool IsDarkMode { get; private set; }

        public event Action? OnChange;

        public AppCurrentState()
        {
            // Load saved preference (default false)
            IsDarkMode = Preferences.Default.Get(ThemePreferenceKey, false);
        }

        public void AuthenticateUser()
        {
            IsLoggedIn = true;
            BroadcastStateUpdate();
        }

        public void RevokeAuthentication()
        {
            IsLoggedIn = false;
            BroadcastStateUpdate();
        }

        public void SwitchThemeMode()
        {
            IsDarkMode = !IsDarkMode;
            Preferences.Default.Set(ThemePreferenceKey, IsDarkMode);
            BroadcastStateUpdate();
        }

        private void BroadcastStateUpdate() => OnChange?.Invoke();
    }
}
