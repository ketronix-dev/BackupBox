// SettingsWindowViewModel.cs
using Avalonia;
using LiteDB;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Linq;

namespace MaxBackup.ViewModels
{
    public class SettingsWindowViewModel : ReactiveObject
    {
        public ObservableCollection<string> AvailableThemes { get; }
        private string selectedTheme;

        public string SelectedTheme
        {
            get => selectedTheme;
            set => this.RaiseAndSetIfChanged(ref selectedTheme, value);
        }

        public ReactiveCommand<Unit, Unit> ApplyThemeCommand { get; }

        public SettingsWindowViewModel()
        {
            // Load settings from LiteDB
            using (var db = new LiteDatabase(@"AppData.db"))
            {
                var settingsCollection = db.GetCollection<AppSettings>("settings");
                var settings = settingsCollection.FindAll().FirstOrDefault();
                if (settings != null)
                {
                    SelectedTheme = settings.Theme;
                }
            }

            AvailableThemes = new ObservableCollection<string> { "Light", "Dark" };

            ApplyThemeCommand = ReactiveCommand.Create(ApplyTheme);
        }

        private void ApplyTheme()
        {
            // Apply the theme (light or dark) based on SelectedTheme
            var uri = SelectedTheme == "Light"
                ? new Uri("avares://Avalonia.Themes.Default/Accents/BaseLight.xaml")
                : new Uri("avares://Avalonia.Themes.Default/Accents/BaseDark.xaml");

            var style = new Avalonia.Styling.StyleInclude(uri)
            {
                Source = uri
            };

            // Remove existing styles and add the new style
            Application.Current.Styles.Clear();
            Application.Current.Styles.Add(style);

            // Save the selected theme to LiteDB
            using (var db = new LiteDatabase(@"AppData.db"))
            {
                var settingsCollection = db.GetCollection<AppSettings>("settings");
                settingsCollection.Upsert(new AppSettings { Theme = SelectedTheme });
            }
        }

    }
}
