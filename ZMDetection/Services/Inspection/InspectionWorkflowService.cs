using System.Diagnostics;
using HalconDotNet;
using ZMDetection.Models;

namespace ZMDetection.Services;

public sealed class InspectionWorkflowService : IInspectionWorkflowService
{
    private readonly ICameraManager cameraManager;
    private readonly IInspectionService inspectionService;
    private readonly IProductionStatisticsService statisticsService;
    private readonly ILogService logService;
    private CancellationTokenSource? currentRunCancellation;

    private ICameraService? camera1;

    public InspectionWorkflowService(
        ICameraManager cameraManager,
        IInspectionService inspectionService,
        IProductionStatisticsService statisticsService,
        IParamService recipeService,
        ILogService logService)
    {
        this.cameraManager = cameraManager;
        this.inspectionService = inspectionService;
        this.statisticsService = statisticsService;
        this.logService = logService;
    }

    public bool IsRunning { get; private set; }
    /// <summary>
    /// 开始检测流程
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<InspectionResult> StartRunAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        using var linkedCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        currentRunCancellation = linkedCancellation;
        //var runToken = linkedCancellation.Token;
        IsRunning = true;
        InspectionResult result;

        try
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            object? frame = await camera1!.GetOneFrameImageAsync(linkedCancellation.Token);
            HObject? ho_inputImg = frame as HObject;
            if(ho_inputImg == null)
            {
                sw.Stop();
                result = new InspectionResult("Error", false, 0, sw.ElapsedMilliseconds);
                sw.Reset();
                logService.Error(LogCategory.Running, "检测错误,相机采集图像为空!");
                return result;
            }
            result = await inspectionService.RunInspectionAsync(ho_inputImg, linkedCancellation.Token);

            statisticsService.ApplyResult(result);
            logService.Info(LogCategory.Running, $"检测完成: {result.ID}, 结果={(result.IsOk ? "OK" : "NG")}");
            return result;
        }
        finally
        {
            //检测完成后token置空
            if (ReferenceEquals(currentRunCancellation, linkedCancellation))
            {
                currentRunCancellation = null;
            }

            IsRunning = false;
        }
    }
    /// <summary>
    /// 停止检测流程
    /// </summary>
    /// <returns></returns>
    public async Task StopAsync()
    {
        currentRunCancellation?.Cancel();
        await Task.Delay(100);
        IsRunning = false;
    }
    public bool InitializeCamera()
    {
        if(cameraManager.Cameras.Count == 0)
        {
            logService.Error(LogCategory.Running, "相机数量为0,无法调用相机!");
            return false;
        }
        camera1 = cameraManager.Cameras[0];
        return true;
    }
}

