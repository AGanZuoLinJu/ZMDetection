using System.Windows;

namespace PCBDetection.Services;

public interface ILoginDialogService
{
    bool ShowDialog(Window? owner);
}
