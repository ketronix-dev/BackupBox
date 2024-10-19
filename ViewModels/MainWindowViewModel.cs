using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Reactive;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using BackupBox.Services;
using LiteDB;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using ReactiveUI;

namespace BackupBox.ViewModels
{
    public partial class MainWindowViewModel : ReactiveObject
    {
        private string status = "Status: Waiting";
        private int backupId;
        private BackupItem? selectedBackup;  // Додаємо змінну для вибраного елемента
        private string dbPath = $@"C:\Users\{Environment.UserName}\Documents\Backups\backups.db";
        private LiteDatabase database;
        private ILiteCollection<BackupItem> backupsCollection;
        private ILiteCollection<BsonDocument> settingsCollection; // Колекція для налаштувань (наприклад, для backupId)
        private bool backupsLoaded = false; // Прапорець, щоб гарантувати, що завантаження виконується тільки один раз
        private double backupProgress;

        public string Status
        {
            get => status;
            set => this.RaiseAndSetIfChanged(ref status, value);
        }

        // Колекція для зберігання списку бекапів
        public ObservableCollection<BackupItem> Backups { get; } = new ObservableCollection<BackupItem>();

        // Властивість для вибраного бекапу
        public BackupItem? SelectedBackup
        {
            get => selectedBackup;
            set => this.RaiseAndSetIfChanged(ref selectedBackup, value);
        }

        public double BackupProgress
        {
            get => backupProgress;
            set => this.RaiseAndSetIfChanged(ref backupProgress, value);
        }

        public ICommand CreateBackup { get; }
        public ICommand RestoreFromFileCommand { get; }
        public ICommand ShowAboutCommand { get; }
        public ICommand OpenSupportCommand { get; }
        public ICommand RestoreCommand { get; }  // Додаємо команду для відновлення
        public ReactiveCommand<Unit, Unit>? QuitCommand { get; set; }

        public MainWindowViewModel()
        {
            // Підключення до бази даних LiteDB
            database = new LiteDatabase(dbPath);
            backupsCollection = database.GetCollection<BackupItem>("backups");
            settingsCollection = database.GetCollection<BsonDocument>("settings");

            // Завантажити бекапи з бази при старті
            if (!backupsLoaded)
            {
                LoadBackupIdFromDatabase(); // Завантажити останній backupId із бази
                LoadBackupsFromDatabase();
                backupsLoaded = true; // Прапорець встановлюється після першого виклику
            }

            ShowAboutCommand = ReactiveCommand.CreateFromTask(ShowAboutMessage);
            OpenSupportCommand = ReactiveCommand.Create(OpenSupportLink);

            // Створення папок, якщо вони не існують
            if (!Directory.Exists($@"C:\Users\{Environment.UserName}\Documents\Backups"))
            {
                Directory.CreateDirectory($@"C:\Users\{Environment.UserName}\Documents\Backups");
            }

            CreateBackup = ReactiveCommand.CreateFromTask(async () =>
            {
                Status = "Status: Backup in progress...";
                BackupProgress = 0;

                // Етап 1: Копіювання даних
                string tempBackupPath = @"C:\ProgramData\HiMarket-bkp";
                FileService.CopyDirectory(@"C:\ProgramData\HiMarket", tempBackupPath);
                BackupProgress = 33; // Встановлюємо прогрес на 33%

                // Етап 2: Стискання
                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string backupFileName = $"Backup_{backupId++}_{timestamp}.box";
                string backupPath = $@"C:\Users\{Environment.UserName}\Documents\Backups\{backupFileName}";

                await Task.Run(() =>
                {
                    BoxCompressor.Compress(tempBackupPath, backupPath);
                });
                BackupProgress = 66; // Встановлюємо прогрес на 66%

                // Етап 3: Видалення тимчасових даних
                Directory.Delete(tempBackupPath, true);
                BackupProgress = 100; // Завершення

                var fileInfo = new FileInfo(backupPath);
                var newBackup = new BackupItem
                {
                    Id = backupId,
                    FileName = fileInfo.Name,
                    BackupDate = DateTime.Now.ToShortDateString(),
                    BackupTime = DateTime.Now.ToShortTimeString(),
                    FileSizeInBytes = fileInfo.Length,
                    FilePath = backupPath
                };

                Backups.Add(newBackup);
                backupsCollection.Insert(newBackup);
                SaveBackupIdToDatabase();

                Status = "Status: Waiting";
            });


            RestoreCommand = ReactiveCommand.Create(() =>
            {
                if (SelectedBackup != null)
                {
                    Console.WriteLine($"Restoring backup: {SelectedBackup.FilePath}");
                }
            });

            RestoreFromFileCommand = ReactiveCommand.CreateFromTask(OpenFileDialog);
        }

        // Метод для завантаження backupId з бази даних
        private void LoadBackupIdFromDatabase()
        {
            // Отримуємо останній запис за backupId
            var setting = settingsCollection.FindOne(Query.All());

            // Якщо запис існує і має поле backupId, завантажуємо його
            if (setting != null && setting.ContainsKey("backupId"))
            {
                backupId = setting["backupId"];
            }
            else
            {
                // Якщо немає налаштувань backupId, ініціалізуємо його
                // Перевіряємо всі бекапи, щоб знайти найбільший backupId
                var maxBackupItem = backupsCollection.Query().OrderByDescending(x => x.Id).FirstOrDefault();
                backupId = (maxBackupItem != null) ? maxBackupItem.Id + 1 : 1;
            }
        }

        // Метод для збереження backupId в базу даних
        private void SaveBackupIdToDatabase()
        {
            // Видаляємо старі записи, щоб уникнути дублювання
            settingsCollection.DeleteMany(BsonExpression.Create("1=1"));

            // Створюємо новий запис з оновленим значенням backupId
            var setting = new BsonDocument
            {
                ["backupId"] = backupId
            };

            settingsCollection.Upsert(setting); // Оновлюємо або додаємо новий запис
        }
        // Метод для завантаження бекапів з бази даних
        private void LoadBackupsFromDatabase()
        {
            // Отримуємо всі записи з колекції бекапів
            var backupItems = backupsCollection.FindAll();

            // Додаємо кожен бекап у колекцію для відображення в ListBox
            foreach (var backup in backupItems)
            {
                Backups.Add(backup);
            }
        }

        private void OpenSupportLink()
        {
            var url = "https://maxbox.com.ua/support/";
            Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });
        }

        private async Task ShowAboutMessage()
        {
            var aboutMessage = "BackupBox v1.0\nCreated by Drew\nThis application allows you to create and restore backups.";
            var box = MessageBoxManager
            .GetMessageBoxStandard("About", aboutMessage,
                ButtonEnum.Ok);

            await box.ShowAsync();
        }

        private async Task OpenFileDialog()
        {
            var mainWindow = Avalonia.Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop
                ? desktop.MainWindow
                : null;

            if (mainWindow == null)
                return;

            var fileDialog = new OpenFileDialog
            {
                Title = "Select a backup file",
                Filters = new System.Collections.Generic.List<FileDialogFilter>
                {
                    new FileDialogFilter { Name = "Backup files", Extensions = { "box" } }
                },
                AllowMultiple = false
            };

            string[]? result = await fileDialog.ShowAsync(mainWindow);

            if (result != null && result.Length > 0)
            {
                string selectedFilePath = result[0];
                Console.WriteLine($"Selected file: {selectedFilePath}");
            }
        }
    }
}
