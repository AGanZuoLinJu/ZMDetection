using System.Collections.ObjectModel;
using System.Windows;
using PCBDetection.Models;
using PCBDetection.Services.Interfaces;
using Prism.Commands;
using Prism.Mvvm;

namespace PCBDetection.ViewModels;

public sealed class MainWindowViewModel : BindableBase
{
    private readonly IInspectionWorkflowService workflowService;
    private readonly IInspectionService inspectionService;
    private readonly ICameraService cameraService;
    private readonly IRecipeService recipeService;
    private readonly ILightService lightService;
    private readonly IPlcService plcService;
    private readonly IMesService mesService;
    private readonly IAiDetectionService aiDetectionService;
    private readonly IProductionStatisticsService statisticsService;
    private readonly ILogService logService;
    private CancellationTokenSource? runCancellation;
    private string inspectionStatus = "Ready";
    private string currentBoardId = "--";
    private string cameraStatus;
    private string plcStatus = "Offline";
    private string mesStatus = "Offline";
    private string lightStatus = "Offline";
    private string aiStatus = "Not initialized";
    private string currentRecipe = "PCB_TOP_AOI_V1";
    private string previewMessage;
    private int okCount;
    private int ngCount;
    private int defectCount;
    private double cycleTime;
    private bool isRunning;

    public MainWindowViewModel(
        IInspectionWorkflowService workflowService,
        IInspectionService inspectionService,
        ICameraService cameraService,
        IRecipeService recipeService,
        ILightService lightService,
        IPlcService plcService,
        IMesService mesService,
        IAiDetectionService aiDetectionService,
        IProductionStatisticsService statisticsService,
        ILogService logService)
    {
        this.workflowService = workflowService;
        this.inspectionService = inspectionService;
        this.cameraService = cameraService;
        this.recipeService = recipeService;
        this.lightService = lightService;
        this.plcService = plcService;
        this.mesService = mesService;
        this.aiDetectionService = aiDetectionService;
        this.statisticsService = statisticsService;
        this.logService = logService;

        cameraStatus = $"{cameraService.CameraName} / {cameraService.ConnectionStatus}";
        previewMessage = "Camera preview is waiting for inspection.";

        StartInspectionCommand = new DelegateCommand(async () => await StartInspectionAsync(), () => !IsRunning)
            .ObservesProperty(() => IsRunning);
        StopInspectionCommand = new DelegateCommand(async () => await StopInspectionAsync(), () => IsRunning)
            .ObservesProperty(() => IsRunning);
        ResetStatisticsCommand = new DelegateCommand(ResetStatistics);
        LoadImageCommand = new DelegateCommand(async () => await LoadImageAsync());

        logService.LogAdded += OnLogAdded;
        inspectionService.InspectionCompleted += OnInspectionCompleted;
        _ = InitializeAsync();
    }

    public string ApplicationTitle => "PCB检测平台";

    public string InspectionStatus
    {
        get => inspectionStatus;
        private set => SetProperty(ref inspectionStatus, value);
    }

    public string CurrentBoardId
    {
        get => currentBoardId;
        private set => SetProperty(ref currentBoardId, value);
    }

    public string CameraStatus
    {
        get => cameraStatus;
        private set => SetProperty(ref cameraStatus, value);
    }

    public string PlcStatus
    {
        get => plcStatus;
        private set => SetProperty(ref plcStatus, value);
    }

    public string MesStatus
    {
        get => mesStatus;
        private set => SetProperty(ref mesStatus, value);
    }

    public string LightStatus
    {
        get => lightStatus;
        private set => SetProperty(ref lightStatus, value);
    }

    public string AiStatus
    {
        get => aiStatus;
        private set => SetProperty(ref aiStatus, value);
    }

    public string CurrentRecipe
    {
        get => currentRecipe;
        private set => SetProperty(ref currentRecipe, value);
    }

    public string PreviewMessage
    {
        get => previewMessage;
        private set => SetProperty(ref previewMessage, value);
    }

    public int OkCount
    {
        get => okCount;
        private set
        {
            if (SetProperty(ref okCount, value))
            {
                RaisePropertyChanged(nameof(YieldRate));
            }
        }
    }

    public int NgCount
    {
        get => ngCount;
        private set
        {
            if (SetProperty(ref ngCount, value))
            {
                RaisePropertyChanged(nameof(YieldRate));
            }
        }
    }

    public int DefectCount
    {
        get => defectCount;
        private set => SetProperty(ref defectCount, value);
    }

    public double CycleTime
    {
        get => cycleTime;
        private set
        {
            if (SetProperty(ref cycleTime, value))
            {
                RaisePropertyChanged(nameof(CycleTimeProgress));
            }
        }
    }

    public double CycleTimeProgress
    {
        get
        {
            const double targetCycleTime = 1600;
            return Math.Max(0, Math.Min(CycleTime / targetCycleTime * 100, 100));
        }
    }

    public double YieldRate => statisticsService.Current.YieldRate;

    public bool IsRunning
    {
        get => isRunning;
        private set => SetProperty(ref isRunning, value);
    }

    public ObservableCollection<LogItem> LogItems { get; } = new();

    public DelegateCommand  StartInspectionCommand { get; }

    public DelegateCommand StopInspectionCommand { get; }

    public DelegateCommand ResetStatisticsCommand { get; }

    public DelegateCommand LoadImageCommand { get; }

    private async Task InitializeAsync()
    {
        try
        {
            using var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(8));
            var recipe = await recipeService.LoadCurrentRecipeAsync(cancellation.Token);
            CurrentRecipe = recipe.RecipeName;

            CameraStatus = (await cameraService.InitializeAsync(cancellation.Token)).DisplayText;
            LightStatus = (await lightService.InitializeAsync(cancellation.Token)).DisplayText;
            AiStatus = (await aiDetectionService.InitializeAsync(recipe, cancellation.Token)).DisplayText;
            PlcStatus = (await plcService.ConnectAsync(cancellation.Token)).DisplayText;
            MesStatus = (await mesService.ConnectAsync(cancellation.Token)).DisplayText;

            logService.Info("System initialized with Prism container and migration service boundaries.");
        }
        catch (Exception ex)
        {
            InspectionStatus = "Initialize failed";
            logService.Error($"Initialization failed: {ex.Message}");
        }
    }

    private async Task StartInspectionAsync()
    {
        runCancellation?.Cancel();
        runCancellation = new CancellationTokenSource();
        IsRunning = true;
        InspectionStatus = "Running";

        try
        {
            var result = await workflowService.RunSingleAsync(runCancellation.Token);
            ApplyResult(result);
        }
        catch (OperationCanceledException)
        {
            InspectionStatus = "Stopped";
            logService.Warning("Inspection was canceled.");
        }
        catch (Exception ex)
        {
            InspectionStatus = "Error";
            logService.Error($"Inspection failed: {ex.Message}");
        }
        finally
        {
            IsRunning = false;
            runCancellation.Dispose();
            runCancellation = null;
        }
    }

    private async Task StopInspectionAsync()
    {
        runCancellation?.Cancel();
        await workflowService.StopAsync();
        IsRunning = false;
        InspectionStatus = "Stopped";
        logService.Warning("Inspection stopped by operator.");
    }

    private void ResetStatistics()
    {
        inspectionService.Reset();
        statisticsService.Reset();
        ApplyStatistics(statisticsService.Current);
        CurrentBoardId = "--";
        InspectionStatus = "Ready";
        IsRunning = false;
        PreviewMessage = "Camera preview is waiting for inspection.";
        LogItems.Clear();
        logService.Info("Statistics reset.");
    }

    private async Task LoadImageAsync()
    {
        try
        {
            using var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            var frame = await cameraService.SoftwareTriggerAsync(cancellation.Token);
            PreviewMessage = string.IsNullOrWhiteSpace(frame.ImagePath)
                ? cameraService.CapturePreview()
                : $"Preview frame captured: {frame.ImagePath}";
            CameraStatus = $"{cameraService.CameraName} / {cameraService.ConnectionStatus}";
            logService.Camera("Preview frame captured.");
        }
        catch (Exception ex)
        {
            logService.Error($"Preview capture failed: {ex.Message}");
        }
    }

    private void ApplyResult(InspectionResult result)
    {
        CurrentBoardId = result.BoardId;
        InspectionStatus = result.IsOk ? "OK" : "NG";
        ApplyStatistics(statisticsService.Current);

        if (!result.IsOk)
        {
            logService.Defect($"{result.BoardId} has {result.DefectCount} defects.");
        }
    }

    private void ApplyStatistics(ProductionStatisticsSnapshot snapshot)
    {
        OkCount = snapshot.OkCount;
        NgCount = snapshot.NgCount;
        DefectCount = snapshot.DefectCount;
        CycleTime = snapshot.LastCycleTimeMilliseconds;
        RaisePropertyChanged(nameof(YieldRate));
    }

    private void OnInspectionCompleted(object? sender, InspectionResult result)
    {
        PreviewMessage = string.IsNullOrWhiteSpace(result.ImagePath)
            ? result.Message
            : $"Inspection image: {result.ImagePath}";
    }

    private void OnLogAdded(object? sender, LogItem logItem)
    {
        void AddLog()
        {
            LogItems.Insert(0, logItem);

            while (LogItems.Count > 80)
            {
                LogItems.RemoveAt(LogItems.Count - 1);
            }
        }

        var dispatcher = Application.Current?.Dispatcher;
        if (dispatcher == null || dispatcher.CheckAccess())
        {
            AddLog();
            return;
        }

        dispatcher.BeginInvoke((Action)AddLog);
    }
}
