using Prism.Mvvm;

namespace PCBDetection.ViewModels;

public sealed class SystemSettingsViewModel : BindableBase
{
    public string Title => "系统设置";

    public string Description => "Application and environment settings will be configured here.";
}
