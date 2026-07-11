using System.Windows;

namespace ZMDetection.Services;

public interface ILoginDialogService
{
    bool ShowDialog(Window? owner);
}
