using System.Windows.Input;
using ReactiveUI;
namespace MaxBackup.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
#pragma warning disable CA1822 // Mark members as static
    public string Status {get;set;} = "Status: Waiting";

    public ICommand CreateBackup { get; }

    public MainWindowViewModel()
    {
        Status = "Status: Backuping...";
        CreateBackup = ReactiveCommand.Create(() =>
        {
            BoxCompressor.Compress("/home/artem/Документи/BackupBox", "/home/artem/Документи/BackupBox.box");
            Status = "Status: Waiting";
        });
    }

#pragma warning restore CA1822 // Mark members as static
}
