using System;

namespace JournalMate.Services
{
    public enum ToastType
    {
        Success,
        Error,
        Warning,
        Info
    }

    public class ToastService
    {
        public event Action<string, ToastType, int>? OnShow;

        public void ShowToast(string message, ToastType type = ToastType.Info, int duration = 3000)
        {
            OnShow?.Invoke(message, type, duration);
        }
    }
}
