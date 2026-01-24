public class ToggleTheme
{
    public bool IsDarkMode { get; private set; }

    public event Action? OnChange;

    public void FlipThemeState()
    {
        IsDarkMode = !IsDarkMode;
        OnChange?.Invoke();
    }

    public void ApplyThemePreference(bool isDarkModeEnabled)
    {
        IsDarkMode = isDarkModeEnabled;
        OnChange?.Invoke();
    }
}
