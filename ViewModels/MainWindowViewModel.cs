using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using ReactiveUI;

namespace BackupBox.ViewModels
{
    public partial class MainWindowViewModel : ReactiveObject
    {
        private string status = "Status: Waiting";
        private int backupId = 1;

        public string Status
        {
            get => status;
            set => this.RaiseAndSetIfChanged(ref status, value);
        }

        // Колекція для зберігання списку бекапів
        public ObservableCollection<BackupItem> Backups { get; } = new ObservableCollection<BackupItem>();

        public ICommand CreateBackup { get; }
        public ICommand RestoreFromFileCommand { get; }
        public ICommand ShowAboutCommand { get; }
        public ICommand OpenSupportCommand { get; }

        public MainWindowViewModel()
        {
            ShowAboutCommand = ReactiveCommand.CreateFromTask(ShowAboutMessage);
            OpenSupportCommand = ReactiveCommand.Create(OpenSupportLink);

            CreateBackup = ReactiveCommand.CreateFromTask(async () =>
            {
                Status = "Status: Backup in progress...";

                string sourcePath = @"C:\Users\Drew\Documents\BackupBox";

                // Створення унікального імені файлу для бекапу
                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string backupFileName = $"Backup_{backupId}_{timestamp}.box";
                string backupPath = $@"C:\Users\Drew\Documents\{backupFileName}";

                // Виконуємо бекап у фоновому потоці
                await Task.Run(() =>
                {
                    BoxCompressor.Compress(sourcePath, backupPath);
                });

                // Отримуємо інформацію про файл
                var fileInfo = new FileInfo(backupPath);
                string fileName = fileInfo.Name;
                string backupDate = DateTime.Now.ToShortDateString();
                string backupTime = DateTime.Now.ToShortTimeString();
                long fileSize = fileInfo.Length;

                // Додаємо новий бекап у колекцію
                Backups.Add(new BackupItem
                {
                    Id = backupId++,
                    FileName = fileName,
                    BackupDate = backupDate,
                    BackupTime = backupTime,
                    FileSizeInBytes = fileSize,
                    FilePath = backupPath
                });

                Status = "Status: Waiting";
            });

            // Команда для відкриття діалогу вибору файлу
            RestoreFromFileCommand = ReactiveCommand.CreateFromTask(OpenFileDialog);
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

            var result = await box.ShowAsync();
        }

        // Метод для відкриття діалогу вибору файлу
        private async Task OpenFileDialog()
        {
            // Отримуємо головне вікно
            var mainWindow = Avalonia.Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop
                ? desktop.MainWindow
                : null;

            if (mainWindow == null)
                return;

            // Створюємо діалог вибору файлу
            var fileDialog = new OpenFileDialog
            {
                Title = "Select a backup file",
                Filters = new System.Collections.Generic.List<FileDialogFilter>
                {
                    new FileDialogFilter { Name = "Backup files", Extensions = { "box" } }
                },
                AllowMultiple = false
            };

            // Показуємо діалог і отримуємо вибраний файл
            string[]? result = await fileDialog.ShowAsync(mainWindow);

            if (result != null && result.Length > 0)
            {
                string selectedFilePath = result[0];
                Console.WriteLine($"Selected file: {selectedFilePath}");
            }
        }
    }
}
