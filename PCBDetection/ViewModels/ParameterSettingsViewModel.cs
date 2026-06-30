using Prism.Mvvm;

namespace PCBDetection.ViewModels;

public sealed class ParameterSettingsViewModel : BindableBase
{
    public string Title => "参数设置";

    public string Description => "Vision, recipe and inspection parameters will be configured here.";
}
