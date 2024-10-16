using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using MaxBackup.Views;
using ReactiveUI;
namespace MaxBackup.ViewModels;

public partial class MainWindowViewModel : ReactiveObject
{
    private string status = "Status: Waiting";
    private int backupId = 1; // Start the backup ID counter at 1

    public string Status
    {
        get => status;
        set => this.RaiseAndSetIfChanged(ref status, value);
        
    }

    // Collection to hold the list of backups
    public ObservableCollection<BackupItem> Backups { get; } = new ObservableCollection<BackupItem>();

    public ICommand CreateBackup { get; }

    public MainWindowViewModel()
    {
        CreateBackup = ReactiveCommand.CreateFromTask(async () =>
        {
            Status = "Status: Backup in progress...";

            // File paths
            
            string sourcePath = @"C:\Users\Drew\Documents\MaxBackup";

            // Generate a unique file name based on the ID, date, and time
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string backupFileName = $"Backup_{backupId}_{timestamp}.box";
            string backupPath = $@"C:\Users\Drew\Documents\{backupFileName}";

            // Perform the backup in a background thread
            await Task.Run(() =>
            {
                BoxCompressor.Compress(sourcePath, backupPath);
            });

            // Get file info (name, size, etc.)
            var fileInfo = new FileInfo(backupPath);
            string fileName = fileInfo.Name;
            string backupDate = DateTime.Now.ToShortDateString();
            string backupTime = DateTime.Now.ToShortTimeString();
            long fileSize = fileInfo.Length;

            // Add a new backup entry to the ObservableCollection
            Backups.Add(new BackupItem
            {
                Id = backupId++, // Increment the backup ID for the next backup
                FileName = fileName,
                BackupDate = backupDate,
                BackupTime = backupTime,
                FileSizeInBytes = fileSize
            });

            Status = "Status: Waiting";
        });
    }
}
