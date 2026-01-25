using JournalMate.Services;
using JournalMaui.Services;
using Microsoft.Extensions.Logging;
using MudBlazor.Services;

namespace JournalMate
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            builder.Services.AddMauiBlazorWebView();
            builder.Services.AddMudServices();

#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
            builder.Logging.AddDebug();
#endif

            builder.Services.AddSingleton<AppCurrentState>();

            builder.Services.AddSingleton(sp =>
            {
                var appState = sp.GetRequiredService<AppCurrentState>();
                var dbPath = Path.Combine(FileSystem.AppDataDirectory, "journal.db3");
                var db = new JournalDatabase(dbPath, appState);
                Task.Run(async () => await db.InitAsync()).Wait();
                return db;
            });

            builder.Services.AddSingleton<ToggleTheme>();

            builder.Services.AddSingleton<AuthService>(sp =>
            {
                var db = sp.GetRequiredService<JournalDatabase>();
                return new AuthService(db);
            });

            builder.Services.AddSingleton<IFileSaverService, FileSaverService>();
            builder.Services.AddSingleton<IPdfExportService, PdfExportService>();

            return builder.Build();
        }
    }
}
