using System.Windows;
using PCBDetection.Services;
using PCBDetection.Services.Detection;
using PCBDetection.Services.Interfaces;
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
            Container.Resolve<ILogService>().Info("软件正在启动.");
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
            Container.Resolve<ILogService>().Error($"软件启动失败: {ex.Message}");
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
            Container.Resolve<ILogService>().Error($"软件关闭错误: {ex.Message}");
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
        containerRegistry.RegisterSingleton<ILogService, LogService>();
        containerRegistry.RegisterSingleton<ICameraService, MockCameraService>();
        containerRegistry.RegisterSingleton<IInspectionService, MockInspectionService>();
        containerRegistry.RegisterSingleton<IDetectionService, MockAiDetectionService>();
        containerRegistry.RegisterSingleton<IRecipeService, RecipeService>();
        containerRegistry.RegisterSingleton<ILightService, MockLightService>();
        containerRegistry.RegisterSingleton<IPlcService, MockPlcService>();
        containerRegistry.RegisterSingleton<IMesService, MockMesService>();
        containerRegistry.RegisterSingleton<IProductionStatisticsService, ProductionStatisticsService>();
        containerRegistry.RegisterSingleton<IInspectionWorkflowService, InspectionWorkflowService>();
        containerRegistry.RegisterSingleton<IApplicationStatus, ApplicationStatus>();
        containerRegistry.RegisterSingleton<IStartupService, StartupService>();

        //页面注册
        containerRegistry.RegisterForNavigation<DetectionView, DetectionViewModel>("DetectionView");
        containerRegistry.RegisterForNavigation<ParameterSettingsView, ParameterSettingsViewModel>("ParameterSettingsView");
        containerRegistry.RegisterForNavigation<ProductionStatisticsView, ProductionStatisticsViewModel>("ProductionStatisticsView");
        containerRegistry.RegisterForNavigation<SystemSettingsView, SystemSettingsViewModel>("SystemSettingsView");
    }
}

