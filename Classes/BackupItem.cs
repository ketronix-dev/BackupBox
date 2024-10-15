using System;

public class BackupItem
{
    public int Id { get; set; }
    public required string FileName { get; set; }
    public required string BackupDate { get; set; }
    public required string BackupTime { get; set; }
    public long FileSizeInBytes { get; set; }

    // This property will return the file size in MB
    public double FileSizeInMB => Math.Round((double)FileSizeInBytes / (1024 * 1024), 2);

    // This returns the display format for each item in the ListBox
    public override string ToString()
    {
        return $"Backup ID: {Id} - {BackupDate} - {BackupTime} - {FileSizeInMB} MB";
    }
}
