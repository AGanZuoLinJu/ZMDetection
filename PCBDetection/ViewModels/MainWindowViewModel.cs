using System.Windows;
using PCBDetection.Services;
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
    private bool isSidebarExpanded = true;
    //绑定属性 控制主界面页面导航按钮
    public bool IsDetectionSelected => SelectedPage == "DetectionView";
    public bool IsParametersSelected => SelectedPage == "ParameterSettingsView";
    public bool IsStatisticsSelected => SelectedPage == "ProductionStatisticsView";
    public bool IsSystemSelected => SelectedPage == "SystemSettingsView";
    public bool IsSinmulationTestSelected => SelectedPage == "SinmulationTestView";
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
                RaisePropertyChanged(nameof(IsSinmulationTestSelected));
            }
        }
    }
    public string PageTitle
    {
        get => pageTitle;
        private set => SetProperty(ref pageTitle, value);
    }

    #region <<<侧边栏样式
    public bool IsSidebarExpanded
    {
        get => isSidebarExpanded;
        set
        {
            if (SetProperty(ref isSidebarExpanded, value))
            {
                RaisePropertyChanged(nameof(SidebarWidth));
                RaisePropertyChanged(nameof(SidebarToggleIcon));
                RaisePropertyChanged(nameof(SidebarToggleToolTip));
            }
        }
    }
    public GridLength SidebarWidth => new(IsSidebarExpanded ? 196 : 64);
    public string SidebarToggleIcon => IsSidebarExpanded ? "\uE76B" : "\uE76C";
    public string SidebarToggleToolTip => IsSidebarExpanded ? "收起侧边栏" : "展开侧边栏";
    #endregion

    /// <summary>
    /// 导航切换命令
    /// </summary>
    public AsyncDelegateCommand<string> NavigateCommand { get; }
    /// <summary>
    /// 登录按钮命令
    /// </summary>
    public DelegateCommand LoginCommand { get; }
    #endregion

    #region <<<Services
    private readonly IInspectionWorkflowService workflowService;                    //检测流程服务
    private readonly IApplicationStatus applicationStatus;
    private readonly IUserSession userSession;
    private readonly ILoginDialogService loginDialogService;
    #endregion

    public MainWindowViewModel(
        IRegionManager regionManager,
        IApplicationStatus applicationStatus,
        IInspectionWorkflowService workflowService,
        IUserSession userSession,
        ILoginDialogService loginDialogService)
    {
        this.regionManager = regionManager;
        this.applicationStatus = applicationStatus;
        this.workflowService = workflowService;
        this.userSession = userSession;
        this.loginDialogService = loginDialogService;
        Status = applicationStatus;
        UserSession = userSession;
        NavigateCommand = new AsyncDelegateCommand<string>(NavigateAsync);
        LoginCommand = new DelegateCommand(() => loginDialogService.ShowDialog(Application.Current?.MainWindow));
        userSession.SessionExpired += OnSessionExpired;
    }
    public string ApplicationTitle => "ZM-PCB检测平台";
    /// <summary>
    /// 软件状态
    /// </summary>
    public IApplicationStatus Status { get; }
    public IUserSession UserSession { get; }

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
    /// <summary>
    /// 切换页面
    /// </summary>
    /// <param name="target"></param>
    /// <returns></returns>
    private async Task NavigateAsync(string? target)
    {
        if (string.IsNullOrWhiteSpace(target))
        {
            return;
        }

        string? navigationTarget = target!;

        //if (navigationTarget == "ParameterSettingsView" && !userSession.CanAccessParameterSettings)
        //{
        //    MessageBox.Show(Application.Current?.MainWindow, "参数设置需要工程师或管理员权限", "权限不足", MessageBoxButton.OK, MessageBoxImage.Warning);
        //    RefreshNavigationSelection();
        //    return;
        //}

        //设置页面和仿真测试页面需要登录权限
        if ((navigationTarget == "ParameterSettingsView" ||
             navigationTarget == "SinmulationTestView") &&
             applicationStatus.IsInspectionRunning)
        {
            MessageBoxResult confirmation = MessageBox.Show(
                Application.Current?.MainWindow,
                "软件正在运行，是否停止运行？",
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
        PageTitle = navigationTarget switch
        {
            "ParameterSettingsView" => "参数设置",
            "ProductionStatisticsView" => "生产统计",
            "SystemSettingsView" => "系统设置",
            "SinmulationTestView" => "仿真测试",
            _ => "检测界面"
        };
    }
    private void OnSessionExpired(object? sender, EventArgs e)
    {
        MessageBox.Show(
            Application.Current?.MainWindow,
            "登录已超过 5 分钟，请重新登录。",
            "登录超时",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }
    /// <summary>
    /// 刷新按钮的状态
    /// </summary>
    private void RefreshNavigationSelection()
    {
        RaisePropertyChanged(nameof(IsDetectionSelected));
        RaisePropertyChanged(nameof(IsParametersSelected));
        RaisePropertyChanged(nameof(IsStatisticsSelected));
        RaisePropertyChanged(nameof(IsSinmulationTestSelected));
        //RaisePropertyChanged(nameof(IsSystemSelected));
    }
    #endregion
}
