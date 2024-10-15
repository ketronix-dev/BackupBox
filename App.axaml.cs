using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.ThemeManager;
using MaxBackup.ViewModels;
using MaxBackup.Views;

namespace MaxBackup
{
    public partial class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
            // Load the ThemeManager
            ThemeManager.Initialize();
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                // Load settings and apply theme
                using (var db = new LiteDatabase(@"AppData.db"))
                {
                    var settingsCollection = db.GetCollection<AppSettings>("settings");
                    var settings = settingsCollection.FindAll().FirstOrDefault();
                    string initialTheme = settings?.Theme ?? "Light"; // Default to "Light"

                    // Set the initial theme using ThemeManager
                    ThemeManager.CurrentTheme = initialTheme == "Light"
                        ? ThemeManager.LightTheme
                        : ThemeManager.DarkTheme;
                }

                desktop.MainWindow = new MainWindow
                {
                    DataContext = new MainWindowViewModel(),
                };
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}
