using System.Windows;
using PCBDetection.Services.Interfaces;
using PCBDetection.Views;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation.Regions;

namespace PCBDetection.ViewModels;

public sealed class MainWindowViewModel : BindableBase
{
    #region <<<界面导航相关
    private readonly IRegionManager regionManager;
    public const string MainRegionName = "MainContentRegion";                       //软件主区域
    private string selectedPage = "DetectionView";                                  
    private string pageTitle = "检测界面";
    private bool navigationInitialized;
    //绑定属性 控制主界面页面导航按钮
    public bool IsDetectionSelected => SelectedPage == "DetectionView";
    public bool IsParametersSelected => SelectedPage == "ParameterSettingsView";
    public bool IsStatisticsSelected => SelectedPage == "ProductionStatisticsView";
    public bool IsSystemSelected => SelectedPage == "SystemSettingsView";
    /// <summary>
    /// 选中的页面
    /// </summary>
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
                RaisePropertyChanged(nameof(IsSystemSelected));
            }
        }
    }
    public string PageTitle
    {
        get => pageTitle;
        private set => SetProperty(ref pageTitle, value);
    }
    /// <summary>
    /// 导航切换命令
    /// </summary>
    public AsyncDelegateCommand<string> NavigateCommand { get; }
    #endregion

    #region <<<Services
    private readonly IInspectionWorkflowService workflowService;                    //检测流程服务
    private readonly IApplicationStatus applicationStatus;
    #endregion

    public MainWindowViewModel(
        IRegionManager regionManager,
        IApplicationStatus applicationStatus,
        IInspectionWorkflowService workflowService)
    {
        this.regionManager = regionManager;
        this.applicationStatus = applicationStatus;
        this.workflowService = workflowService;
        Status = applicationStatus;
        NavigateCommand = new AsyncDelegateCommand<string>(NavigateAsync);
    }
    public string ApplicationTitle => "ZM-PCB检测平台";
    /// <summary>
    /// 软件状态
    /// </summary>
    public IApplicationStatus Status { get; }

    #region <<<其他方法
    public void InitializeNavigation()
    {
        if (navigationInitialized)
        {
            return;
        }

        navigationInitialized = true;
        _ = NavigateAsync("DetectionView");
    }
    private async Task NavigateAsync(string? target)
    {
        if (string.IsNullOrWhiteSpace(target))
        {
            return;
        }

        string? navigationTarget = target!;

        if (navigationTarget == "ParameterSettingsView" && applicationStatus.IsInspectionRunning)
        {
            MessageBoxResult confirmation = MessageBox.Show(
                Application.Current?.MainWindow,
                "软件正在运行，进入参数设置前需要停止检测。是否停止运行？",
                "停止运行确认",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question,
                MessageBoxResult.No);

            if (confirmation != MessageBoxResult.Yes)
            {
                RefreshNavigationSelection();
                return;
            }

            applicationStatus.SetInspectionRunning(false);
            await workflowService.StopAsync();
        }
        regionManager.RequestNavigate(MainRegionName, navigationTarget);
        SelectedPage = navigationTarget;
        switch (navigationTarget)
        {
            case "ParameterSettingsView":
                PageTitle = "参数设置";
                break;
            case "ProductionStatisticsView":
                PageTitle = "生产统计";
                break;
            case "SystemSettingsView":
                PageTitle = "系统设置";
                break;
            default:
                PageTitle = "检测界面";
                break;
        }
    }
    /// <summary>
    /// 刷新按钮的状态
    /// </summary>
    private void RefreshNavigationSelection()
    {
        RaisePropertyChanged(nameof(IsDetectionSelected));
        RaisePropertyChanged(nameof(IsParametersSelected));
        RaisePropertyChanged(nameof(IsStatisticsSelected));
        RaisePropertyChanged(nameof(IsSystemSelected));
    }
    #endregion
}
