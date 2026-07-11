using System.Diagnostics;
using ZMDetection.Models;
using ZMDetection.Services;
using Prism.Mvvm;

namespace ZMDetection.ViewModels;

public sealed class LoadingWindowViewModel : BindableBase
{
    private readonly IStartupService startupService;
    private string windowTitle = "ZM-AI检测平台 - Initializing";
    private string operationDescription = "正在初始化视觉检测平台...";
    private double progressValue;
    private string currentStep = "启动中";
    private string statusMessage = "正在准备PCB检测平台...";
    private bool hasError;

    public LoadingWindowViewModel(IStartupService startupService)
    {
        this.startupService = startupService;
    }

    public string WindowTitle
    {
        get => windowTitle;
        private set => SetProperty(ref windowTitle, value);
    }

    public string OperationDescription
    {
        get => operationDescription;
        private set => SetProperty(ref operationDescription, value);
    }

    public double ProgressValue
    {
        get => progressValue;
        private set => SetProperty(ref progressValue, value);
    }

    public string CurrentStep
    {
        get => currentStep;
        private set => SetProperty(ref currentStep, value);
    }

    public string StatusMessage
    {
        get => statusMessage;
        private set => SetProperty(ref statusMessage, value);
    }

    public bool HasError
    {
        get => hasError;
        private set => SetProperty(ref hasError, value);
    }

    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        await RunOperationAsync(startupService.InitializeAsync, 1500, cancellationToken);
    }
    /// <summary>
    /// 显示关闭信息
    /// </summary>
    public void PrepareForShutdown()
    {
        WindowTitle = "ZM-AI检测平台 - Closing";
        OperationDescription = "关闭视觉检测平台...";
        ProgressValue = 0;
        CurrentStep = "Closing";
        StatusMessage = "Preparing to close the application...";
        HasError = false;
    }

    public async Task ShutdownAsync(CancellationToken cancellationToken)
    {
        await RunOperationAsync(startupService.ShutdownAsync, 1500, cancellationToken);
    }

    private async Task RunOperationAsync(
        Func<IProgress<StartupProgress>, CancellationToken, Task> operation,
        int minimumDurationMilliseconds,
        CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        Progress<StartupProgress>? progress = new Progress<StartupProgress>(ApplyProgress);

        try
        {
            await operation(progress, cancellationToken);
        }
        finally
        {
            var remaining = TimeSpan.FromMilliseconds(minimumDurationMilliseconds) - stopwatch.Elapsed;
            if (remaining > TimeSpan.Zero)
            {
                await Task.Delay(remaining, cancellationToken);
            }
        }
    }
    private void ApplyProgress(StartupProgress progress)
    {
        ProgressValue = progress.Percentage;
        CurrentStep = progress.Step;
        StatusMessage = progress.Message;
        HasError = progress.IsError;
    }
}
