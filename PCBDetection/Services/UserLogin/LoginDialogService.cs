using System.Windows;
using PCBDetection.Views;
using Prism.Ioc;

namespace PCBDetection.Services;

public sealed class LoginDialogService : ILoginDialogService
{
    private readonly IContainerProvider containerProvider;

    public LoginDialogService(IContainerProvider containerProvider)
    {
        this.containerProvider = containerProvider;
    }

    public bool ShowDialog(Window? owner)
    {
        LoginWindow window = containerProvider.Resolve<LoginWindow>();
        window.Owner = owner;
        return window.ShowDialog() == true;
    }
}
