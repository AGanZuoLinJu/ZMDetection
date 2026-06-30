using PCBDetection.Services.Interfaces;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation.Regions;

namespace PCBDetection.ViewModels;

public sealed class MainWindowViewModel : BindableBase
{
    public const string MainRegionName = "MainContentRegion";

    private readonly IRegionManager regionManager;
    private string selectedPage = "DetectionView";
    private string pageTitle = "检测界面";
    private bool navigationInitialized;

    public MainWindowViewModel(IRegionManager regionManager, IApplicationStatus applicationStatus)
    {
        this.regionManager = regionManager;
        Status = applicationStatus;
        NavigateCommand = new DelegateCommand<string>(Navigate);
    }

    public string ApplicationTitle => "ZM-PCB检测平台";

    public IApplicationStatus Status { get; }
    /// <summary>
    /// 导航切换命令
    /// </summary>
    public DelegateCommand<string> NavigateCommand { get; }

    public string SelectedPage
    {
        get => selectedPage;
        private set
        {
            if (SetProperty(ref selectedPage, value))
            {
                RaisePropertyChanged(nameof(IsDetectionSelected));
                RaisePropertyChanged(nameof(IsParametersSelected));
                RaisePropertyChanged(nameof(IsStatisticsSelected));
                RaisePropertyChanged(nameof(IsDevicesSelected));
                RaisePropertyChanged(nameof(IsSystemSelected));
            }
        }
    }

    public string PageTitle
    {
        get => pageTitle;
        private set => SetProperty(ref pageTitle, value);
    }

    public bool IsDetectionSelected => SelectedPage == "DetectionView";

    public bool IsParametersSelected => SelectedPage == "ParameterSettingsView";

    public bool IsStatisticsSelected => SelectedPage == "ProductionStatisticsView";

    public bool IsDevicesSelected => SelectedPage == "DeviceManagementView";

    public bool IsSystemSelected => SelectedPage == "SystemSettingsView";

    public void InitializeNavigation()
    {
        if (navigationInitialized)
        {
            return;
        }

        navigationInitialized = true;
        Navigate("DetectionView");
    }

    private void Navigate(string? target)
    {
        if (string.IsNullOrWhiteSpace(target))
        {
            return;
        }

        string? navigationTarget = target!;
        regionManager.RequestNavigate(MainRegionName, navigationTarget);
        SelectedPage = navigationTarget;
        PageTitle = navigationTarget switch
        {
            "ParameterSettingsView" => "参数设置",
            "ProductionStatisticsView" => "生产统计",
            "DeviceManagementView" => "设备管理",
            "SystemSettingsView" => "系统设置",
            _ => "检测界面"
        };
    }
}
