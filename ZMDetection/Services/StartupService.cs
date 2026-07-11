using ZMDetection.Models;

namespace ZMDetection.Services;

public sealed class StartupService : IStartupService
{
    private readonly IParamService paramService;
    private readonly ICameraManager cameraManager;
    private readonly ILightService lightService;
    private readonly IInspectionService inspectionService;
    private readonly IInspectionWorkflowService workflowService;
    private readonly ITCPClientService plcClientService;
    private readonly ITCPServerService mesServerService;
    private readonly IApplicationStatus applicationStatus;
    private readonly ILogService logService;
    private readonly IAuthenticationService authenticationService;
    private bool initialized;

    public StartupService(
        IParamService recipeService,
        ICameraManager cameraManager,
        ILightService lightService,
        IInspectionService inspectionService,
        IInspectionWorkflowService workflowService,
        ITCPClientService plcClientService,
        ITCPServerService mesServerService,
        IApplicationStatus applicationStatus,
        ILogService logService,
        IAuthenticationService authenticationService)
    {
        this.paramService = recipeService;
        this.cameraManager = cameraManager;
        this.lightService = lightService;
        this.inspectionService = inspectionService;
        this.workflowService = workflowService;
        this.plcClientService = plcClientService;
        this.mesServerService = mesServerService;
        this.applicationStatus = applicationStatus;
        this.logService = logService;
        this.authenticationService = authenticationService;

        plcClientService.ConnectionStatusChanged += OnPlcConnectionStatusChanged;
    }
    /// <summary>
    /// 初始化各个模块
    /// </summary>
    /// <param name="progress"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task InitializeAsync(IProgress<StartupProgress> progress, CancellationToken cancellationToken)
    {
        if (initialized)
        {
            progress.Report(new StartupProgress(100, "Ready", "软件启动加载完成."));
            return;
        }

        progress.Report(new StartupProgress(0, "Starting", "正在启动检测系统..."));
        //初始化本地用户配置
        await RunStepAsync(
            8,
            "用户配置",
            async () =>
            {
                await authenticationService.InitializeAsync(cancellationToken);
                return "用户配置加载完成";
            },
            progress,
            cancellationToken);
        //初始化机种参数
        await RunStepAsync(
            12,
            "参数模块",
            async () =>
            {
                var result = await paramService.InitAllParam(cancellationToken);
                applicationStatus.SetParamStatus(result);
                return $"机种[{RecipeParam.RecipeParamConfig!.CurrentRecipeName}]参数加载完成";
            },
            progress,
            cancellationToken);
        await Task.Delay(500);
        //初始化相机
        await RunStepAsync(
            30,
            "相机模块",
            async () =>
            {
                foreach (ICameraService camera in cameraManager.Cameras)
                {
                    await camera.InitializeAsync(cancellationToken);
                }

                applicationStatus.SetCameraStatus(cameraManager.Cameras.All(camera => camera.ConnectionStatus));
                return $"相机模块初始化完成，共 {cameraManager.Cameras.Count} 台";
            },
            progress,
            cancellationToken);
        await Task.Delay(500);
        //初始化光源
        await RunStepAsync(
            46,
            "光源模块",
            async () =>
            {
                await lightService.InitializeAsync(cancellationToken);
                applicationStatus.SetLightStatus(lightService.Status);
                return "正在初始化光源模块";
            },
            progress,
            cancellationToken);
        await Task.Delay(500);
        //初始化检测模块
        await RunStepAsync(
            66,
            "检测模块",
            async () =>
            {
                await inspectionService.InitializeAsync(cancellationToken);
                applicationStatus.SetDetectionStatus(inspectionService.Status);
                return "正在初始化检测模块";
            },
            progress,
            cancellationToken);
        await Task.Delay(500);
        //初始化PLC
        await RunStepAsync(
            83,
            "PLC",
            async () =>
            {
                string plcIpAddress = CommunicationParam.CommParamConfig.PLCIPAddress;
                int plcPort = CommunicationParam.CommParamConfig.PLCPort;
                var connected = await plcClientService.ConnectAsync(plcIpAddress, plcPort);
                applicationStatus.SetPlcStatus(plcClientService.IsConnected);

                if (!connected)
                {
                    throw new InvalidOperationException($"PLC TCP 连接失败: {plcIpAddress}:{plcPort}");
                }
                return $"PLC TCP 客户端已连接: {plcIpAddress}:{plcPort}";
            },
            progress,
            cancellationToken);
        await Task.Delay(500);
        //初始化MES
        //await RunStepAsync(
        //    100,
        //    "MES",
        //    async () =>
        //    {
        //        int mesListenPort = CommunicationParam.CommParamConfig.MESPort;
        //        await mesServerService.StartListeningAsync(mesListenPort);
        //        applicationStatus.SetMesStatus(mesServerService.IsListening);
        //        return $"MES TCP 服务端已监听端口: {mesListenPort}";
        //    },
        //    progress,
        //    cancellationToken);
        //await Task.Delay(500);
        applicationStatus.SetMesStatus(true);

        initialized = true;
        logService.Info(LogCategory.Running, "软件初始化完成");
        progress.Report(new StartupProgress(100, "Ready", "软件初始化完成"));
    }

    /// <summary>
    /// 释放各个模块
    /// </summary>
    public async Task ShutdownAsync(IProgress<StartupProgress> progress, CancellationToken cancellationToken)
    {
        progress.Report(new StartupProgress(0, "Closing", "正在关闭检测系统..."));

        await RunShutdownStepAsync(
            12,
            "检测流程",
            async () =>
            {
                await workflowService.StopAsync();
                return "正在停止运行检测流程";
            },
            progress,
            cancellationToken);

        await RunShutdownStepAsync(
            32,
            "相机模块",
            async () =>
            {
                foreach (ICameraService camera in cameraManager.Cameras)
                {
                    await camera.StopGrabbingAsync(cancellationToken);
                    await camera.DestroyCamera(cancellationToken);
                }

                applicationStatus.SetCameraStatus(cameraManager.Cameras.All(camera => camera.ConnectionStatus));
                return "正在释放相机模块";
            },
            progress,
            cancellationToken);

        await RunShutdownStepAsync(
            50,
            "光源模块",
            async () =>
            {
                await lightService.TurnOffAsync(cancellationToken);
                await lightService.ReleaseAsync();
                applicationStatus.SetLightStatus(lightService.Status);
                return "正在释放光源模块";
            },
            progress,
            cancellationToken);

        await RunShutdownStepAsync(
            68,
            "检测模块",
            async () =>
            {
                await inspectionService.ReleaseAsync();
                applicationStatus.SetDetectionStatus(inspectionService.Status);
                return "正在释放检测模块";
            },
            progress,
            cancellationToken);

        await RunShutdownStepAsync(
            84,
            "PLC",
            () =>
            {
                plcClientService.Disconnect();
                applicationStatus.SetPlcStatus(plcClientService.IsConnected);
                return Task.FromResult("PLC TCP 客户端已断开");
            },
            progress,
            cancellationToken);

        //await RunShutdownStepAsync(
        //    100,
        //    "MES",
        //    () =>
        //    {
        //        mesServerService.StopListening();
        //        applicationStatus.SetMesStatus(mesServerService.IsListening);
        //        return Task.FromResult("MES TCP 服务端已停止");
        //    },
        //    progress,
        //    cancellationToken);

        initialized = false;
        logService.Info(LogCategory.Running, "软件退出");
        progress.Report(new StartupProgress(100, "Closed", "所有模块释放完成."));
    }

    private async Task RunShutdownStepAsync(
        int percentage,
        string step,
        Func<Task<string>> action,
        IProgress<StartupProgress> progress,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        progress.Report(new StartupProgress(Math.Max(0, percentage - 10), step, $"正在关闭 {step}..."));

        try
        {
            var message = await action();
            progress.Report(new StartupProgress(percentage, step, message));
            logService.Info(LogCategory.Running, $"{step} closed: {message}");
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            logService.Error(LogCategory.Running,$"{step} 关闭软件错误 : {ex.Message}");
            progress.Report(new StartupProgress(percentage, step, $"关闭 {step} 错误 : {ex.Message}", true));
        }

        await Task.Delay(300, cancellationToken);
    }

    private async Task RunStepAsync(
        int percentage,
        string step,
        Func<Task<string>> action,
        IProgress<StartupProgress> progress,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        progress.Report(new StartupProgress(Math.Max(0, percentage - 10), step, $"初始化 {step}..."));

        try
        {
            var message = await action();
            progress.Report(new StartupProgress(percentage, step, message));
            logService.Info(LogCategory.Running, $"{step}: {message}");
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            logService.Error(LogCategory.Running,$"{step} 初始化错误 : {ex.Message}");
            progress.Report(new StartupProgress(percentage, step, $"{step} 未知错误 : {ex.Message}", true));
        }
    }

    private void OnPlcConnectionStatusChanged(object? sender, bool isConnected)
    {
        applicationStatus.SetPlcStatus(isConnected);
    }
}

