using PCBDetection.Models;

namespace PCBDetection.Services;

public sealed class InspectionWorkflowService : IInspectionWorkflowService
{
    private readonly ICameraManager cameraManager;
    private readonly IInspectionService inspectionService;
    private readonly IProductionStatisticsService statisticsService;
    private readonly IRecipeService recipeService;
    private readonly ILogService logService;
    private CancellationTokenSource? currentRunCancellation;

    public InspectionWorkflowService(
        ICameraManager cameraManager,
        IInspectionService inspectionService,
        IProductionStatisticsService statisticsService,
        IRecipeService recipeService,
        ILogService logService)
    {
        this.cameraManager = cameraManager;
        this.inspectionService = inspectionService;
        this.statisticsService = statisticsService;
        this.recipeService = recipeService;
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
        var runToken = linkedCancellation.Token;
        IsRunning = true;

        try
        {
            var recipe = await recipeService.LoadCurrentRecipeAsync(runToken);
            ICameraService camera = cameraManager.Cameras[0];
            var frame = await camera.GetOneFrameImageAsync(runToken);
            var request = new InspectionRequest(recipe.RecipeName, frame.ImagePath, frame);
            var result = await inspectionService.RunInspectionAsync(request, runToken);

            statisticsService.ApplyResult(result);
            logService.Info(LogCategory.Running,$"检测完成: {result.BoardId}, 结果={(result.IsOk ? "OK" : "NG")}");
            return result;
        }
        finally
        {
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
}

