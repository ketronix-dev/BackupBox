using System;

namespace BackupBox.ViewModels
{
    public class BackupItem
    {
        public int Id { get; set; }            // Backup ID
        public string FileName { get; set; }   // Name of the backup file
        public string BackupDate { get; set; } // Date of the backup
        public string BackupTime { get; set; } // Time of the backup
        public long FileSizeInBytes { get; set; } // Size in bytes
        public string FilePath { get; set; }   // Path to the backup file (not shown in the UI)

        // Property to convert file size to megabytes
        public double FileSizeInMB => Math.Round(FileSizeInBytes / (1024.0 * 1024.0), 2);

        // Override ToString() to format how backup items will be displayed in the ListBox
        public override string ToString()
        {
            return $"ID: {Id} | File: {FileName} | Date: {BackupDate} {BackupTime} | Size: {FileSizeInMB} MB";
        }
    }
}
