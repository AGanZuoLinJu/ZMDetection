using PCBDetection.Models;
using PCBDetection.Services.Interfaces;

namespace PCBDetection.Services;

public sealed class StartupService : IStartupService
{
    private readonly IRecipeService recipeService;
    private readonly ICameraService cameraService;
    private readonly ILightService lightService;
    private readonly IDetectionService aiDetectionService;
    private readonly IPlcService plcService;
    private readonly IMesService mesService;
    private readonly IInspectionWorkflowService workflowService;
    private readonly IApplicationStatus applicationStatus;
    private readonly ILogService logService;
    private bool initialized;

    public StartupService(
        IRecipeService recipeService,
        ICameraService cameraService,
        ILightService lightService,
        IDetectionService aiDetectionService,
        IPlcService plcService,
        IMesService mesService,
        IInspectionWorkflowService workflowService,
        IApplicationStatus applicationStatus,
        ILogService logService)
    {
        this.recipeService = recipeService;
        this.cameraService = cameraService;
        this.lightService = lightService;
        this.aiDetectionService = aiDetectionService;
        this.plcService = plcService;
        this.mesService = mesService;
        this.workflowService = workflowService;
        this.applicationStatus = applicationStatus;
        this.logService = logService;
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

        progress.Report(new StartupProgress(0, "Starting", "Preparing system services..."));
        //初始化机种
        await RunStepAsync(12,"Recipe",async () =>
            {
                var recipe = await recipeService.LoadCurrentRecipeAsync(cancellationToken);
                applicationStatus.SetCurrentRecipe(recipe.RecipeName);
                return $"Recipe loaded: {recipe.RecipeName}";
            },
            progress,
            cancellationToken);
        await Task.Delay(500);
        //初始化相机
        await RunStepAsync(
            30,
            "Camera",
            async () =>
            {
                await cameraService.InitializeAsync(cancellationToken);
                applicationStatus.SetCameraStatus(cameraService.ConnectionStatus);
                return "正在初始化相机模块";
            },
            progress,
            cancellationToken);
        await Task.Delay(500);
        //初始化光源
        await RunStepAsync(
            46,
            "Light",
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
            "AI",
            async () =>
            {
                await aiDetectionService.InitializeAsync(recipeService.CurrentRecipe, cancellationToken);
                applicationStatus.SetAiStatus(aiDetectionService.Status);
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
                await plcService.ConnectAsync(cancellationToken);
                applicationStatus.SetPlcStatus(plcService.ConnectionStatus);
                return "正在初始化PLC模块";
            },
            progress,
            cancellationToken);
        await Task.Delay(500);
        //初始化MES
        await RunStepAsync(
            100,
            "MES",
            async () =>
            {
                await mesService.ConnectAsync(cancellationToken);
                applicationStatus.SetMesStatus(mesService.ConnectionStatus);
                return "正在初始化MES模块";
            },
            progress,
            cancellationToken);
        await Task.Delay(500);

        initialized = true;
        logService.Info("软件初始化完成");
        progress.Report(new StartupProgress(100, "Ready", "软件初始化完成"));
    }

    /// <summary>
    /// 释放各个模块
    /// </summary>
    public async Task ShutdownAsync(IProgress<StartupProgress> progress, CancellationToken cancellationToken)
    {
        progress.Report(new StartupProgress(0, "Closing", "Preparing to release system services..."));

        await RunShutdownStepAsync(
            12,
            "停止运行工作流",
            async () =>
            {
                await workflowService.StopAsync();
                return "正在停止运行工作流";
            },
            progress,
            cancellationToken);

        await RunShutdownStepAsync(
            32,
            "相机",
            async () =>
            {
                await cameraService.StopGrabbingAsync(cancellationToken);
                await cameraService.DisconnectAsync(cancellationToken);
                applicationStatus.SetCameraStatus(cameraService.ConnectionStatus);
                return "正在释放相机模块";
            },
            progress,
            cancellationToken);

        await RunShutdownStepAsync(
            50,
            "Light",
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
            "AI",
            async () =>
            {
                await aiDetectionService.ReleaseAsync();
                applicationStatus.SetAiStatus(aiDetectionService.Status);
                return "正在释放检测模块";
            },
            progress,
            cancellationToken);

        await RunShutdownStepAsync(
            84,
            "PLC",
            async () =>
            {
                await plcService.DisconnectAsync(cancellationToken);
                applicationStatus.SetPlcStatus(plcService.ConnectionStatus);
                return "正在释放PLC模块";
            },
            progress,
            cancellationToken);

        await RunShutdownStepAsync(
            100,
            "MES",
            async () =>
            {
                await mesService.DisconnectAsync(cancellationToken);
                applicationStatus.SetMesStatus(mesService.ConnectionStatus);
                return "正在释放MES模块";
            },
            progress,
            cancellationToken);

        initialized = false;
        logService.Info("软件退出");
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
        progress.Report(new StartupProgress(Math.Max(0, percentage - 10), step, $"Closing {step}..."));

        try
        {
            var message = await action();
            progress.Report(new StartupProgress(percentage, step, message));
            logService.Info($"{step} closed: {message}");
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            logService.Error($"{step} shutdown failed: {ex.Message}");
            progress.Report(new StartupProgress(percentage, step, $"Failed to close {step}: {ex.Message}", true));
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
            logService.Info($"{step}: {message}");
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            logService.Error($"{step} initialization failed: {ex.Message}");
            progress.Report(new StartupProgress(percentage, step, $"{step} unavailable: {ex.Message}", true));
        }
    }
}

