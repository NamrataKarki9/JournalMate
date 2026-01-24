using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;

#if WINDOWS
using Windows.Storage;
using Windows.Storage.Pickers;
using WinRT.Interop;
#endif

namespace JournalMate.Services;

public class FileSaverService : IFileSaverService
{
    public async Task<string?> SaveFileAsync(string fileName, string content)
    {
#if WINDOWS
        var savePicker = new FileSavePicker();
        savePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
        savePicker.FileTypeChoices.Add("Text File", new List<string>() { ".txt" });
        savePicker.SuggestedFileName = fileName;

        // Need to set Window Handle for the picker to show on Windows
        var window = Microsoft.Maui.Controls.Application.Current?.Windows[0].Handler?.PlatformView as Microsoft.UI.Xaml.Window;
        if (window != null)
        {
            var hwnd = WindowNative.GetWindowHandle(window);
            InitializeWithWindow.Initialize(savePicker, hwnd);
        }

        StorageFile file = await savePicker.PickSaveFileAsync();
        if (file != null)
        {
            await File.WriteAllTextAsync(file.Path, content);
            return file.Path;
        }
        return null;
#else
        // Fallback for other platforms
        var filePath = Path.Combine(FileSystem.AppDataDirectory, fileName);
        await File.WriteAllTextAsync(filePath, content);
        return filePath;
#endif
    }
}
