using System.Windows;
using PCBDetection.Models;
using PCBDetection.Services;
using PCBDetection.ViewModels;
using PCBDetection.Views;
using Prism.DryIoc;
using Prism.Ioc;

namespace PCBDetection;

public partial class App : PrismApplication
{
    private CancellationTokenSource? startupCancellation;
    private Window? pendingShell;
    private LoadingWindow? loadingWindow;
    private LoadingWindowViewModel? loadingViewModel;
    private bool startupCompleted;
    private bool shutdownInProgress;

    protected override Window CreateShell()
    {
        return Container.Resolve<MainWindow>();
    }
    protected override void InitializeShell(Window shell)
    {
        ShutdownMode = ShutdownMode.OnExplicitShutdown;
        startupCancellation = new CancellationTokenSource();
        pendingShell = shell;

        loadingWindow = Container.Resolve<LoadingWindow>();
        loadingViewModel = (LoadingWindowViewModel)loadingWindow.DataContext;
        MainWindow = loadingWindow;

        loadingWindow.Closing += (_, _) =>
        {
            if (startupCompleted)
            {
                return;
            }

            startupCancellation.Cancel();
            Shutdown();
        };
    }
    protected override async void OnInitialized()
    {
        if (loadingWindow == null ||
            loadingViewModel == null ||
            pendingShell == null ||
            startupCancellation == null)
        {
            Shutdown();
            return;
        }

        try
        {
            Container.Resolve<ILogService>().Info(LogCategory.Running,"软件正在启动.");
            loadingWindow.Show();
            await loadingViewModel.InitializeAsync(startupCancellation.Token);
            startupCancellation.Token.ThrowIfCancellationRequested();

            MainWindow = pendingShell;
            pendingShell.Show();
            startupCompleted = true;
            loadingWindow.Close();
            ShutdownMode = ShutdownMode.OnMainWindowClose;
        }
        catch (OperationCanceledException)
        {
            Shutdown();
        }
        catch (Exception ex)
        {
            Container.Resolve<ILogService>().Error(LogCategory.Running,$"软件启动失败: {ex.Message}");
            Shutdown();
        }
    }
    public async Task ShutdownApplicationAsync(Window mainWindow)
    {
        if (shutdownInProgress)
        {
            return;
        }

        shutdownInProgress = true;
        ShutdownMode = ShutdownMode.OnExplicitShutdown;

        var shutdownWindow = Container.Resolve<LoadingWindow>();
        var shutdownViewModel = (LoadingWindowViewModel)shutdownWindow.DataContext;
        var canCloseShutdownWindow = false;

        shutdownWindow.Closing += (_, args) =>
        {
            if (!canCloseShutdownWindow)
            {
                args.Cancel = true;
            }
        };

        shutdownViewModel.PrepareForShutdown();
        MainWindow = shutdownWindow;
        mainWindow.Hide();
        shutdownWindow.Show();

        try
        {
            await shutdownViewModel.ShutdownAsync(CancellationToken.None);
        }
        catch (Exception ex)
        {
            Container.Resolve<ILogService>().Error(
                LogCategory.Running,
                $"软件关闭错误: {ex.Message}");
        }
        finally
        {
            canCloseShutdownWindow = true;
            shutdownWindow.Close();
            Shutdown();
        }
    }
    protected override void RegisterTypes(IContainerRegistry containerRegistry)
    {
        //日志模块
        containerRegistry.RegisterSingleton<ILogService, LogService>();

        //相机模块 这里手动创建实例
        ICameraService cam1 = new MockCameraService();
        ICameraManager cameraManager = new CameraManager(new[]
        {   
            cam1
        },Container.Resolve<ILogService>());
        containerRegistry.RegisterInstance<ICameraManager>(cameraManager);

        //通讯
        containerRegistry.RegisterSingleton<ITCPServerService, TCPServerService>();
        containerRegistry.RegisterSingleton<ITCPClientService, TCPClientService>();

        containerRegistry.RegisterSingleton<IInspectionService, InspectionService>();
        containerRegistry.RegisterSingleton<IAIDetectionService, MockAiDetectionService>();
        containerRegistry.RegisterSingleton<IParamService, ParamService>();
        containerRegistry.RegisterSingleton<ILightService, MockLightService>();

        containerRegistry.RegisterSingleton<IProductionStatisticsService, ProductionStatisticsService>();
        containerRegistry.RegisterSingleton<IInspectionWorkflowService, InspectionWorkflowService>();
        containerRegistry.RegisterSingleton<IApplicationStatus, ApplicationStatus>();
        containerRegistry.RegisterSingleton<IStartupService, StartupService>();

        //用户登录相关
        containerRegistry.RegisterSingleton<IAuthenticationService, AuthenticationService>();
        containerRegistry.RegisterSingleton<IUserSession, UserSession>();
        containerRegistry.RegisterSingleton<ILoginDialogService, LoginDialogService>();

        //页面注册
        containerRegistry.RegisterForNavigation<DetectionView, DetectionViewModel>("DetectionView");
        containerRegistry.RegisterForNavigation<ParameterSettingsView, ParameterSettingsViewModel>("ParameterSettingsView");
        containerRegistry.RegisterForNavigation<ProductionStatisticsView, ProductionStatisticsViewModel>("ProductionStatisticsView");
        containerRegistry.RegisterForNavigation<SystemSettingsView, SystemSettingsViewModel>("SystemSettingsView");
        containerRegistry.RegisterForNavigation<SinmulationTestView, SinmulationTestViewModel>("SinmulationTestView");
    }
}

