using PCBDetection.Models;
using PCBDetection.Services.Interfaces;

namespace PCBDetection.Services.Detection;

public sealed class InspectionWorkflowService : IInspectionWorkflowService
{
    private readonly ICameraService cameraService;
    private readonly IInspectionService inspectionService;
    private readonly IProductionStatisticsService statisticsService;
    private readonly IRecipeService recipeService;
    private readonly ILogService logService;
    private CancellationTokenSource? currentRunCancellation;

    public InspectionWorkflowService(
        ICameraService cameraService,
        IInspectionService inspectionService,
        IProductionStatisticsService statisticsService,
        IRecipeService recipeService,
        ILogService logService)
    {
        this.cameraService = cameraService;
        this.inspectionService = inspectionService;
        this.statisticsService = statisticsService;
        this.recipeService = recipeService;
        this.logService = logService;
    }

    public bool IsRunning { get; private set; }
    /// <summary>
    /// 执行检测任务
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<InspectionResult> RunSingleAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        using var linkedCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        currentRunCancellation = linkedCancellation;
        var runToken = linkedCancellation.Token;
        IsRunning = true;

        try
        {
            var recipe = await recipeService.LoadCurrentRecipeAsync(runToken);
            await cameraService.ConnectAsync(runToken);
            var frame = await cameraService.SoftwareTriggerAsync(runToken);
            var request = new InspectionRequest(recipe.RecipeName, frame.ImagePath, frame);
            var result = await inspectionService.RunInspectionAsync(request, runToken);

            statisticsService.ApplyResult(result);
            logService.Info($"检测完成: {result.BoardId}, 结果={(result.IsOk ? "OK" : "NG")}");
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

    public async Task StopAsync()
    {
        currentRunCancellation?.Cancel();
        await inspectionService.StopAutoInspectionAsync();
        IsRunning = false;
    }
}
