using PCBDetection.Models;
using PCBDetection.Services.Interfaces;

namespace PCBDetection.Detection;

public sealed class InspectionWorkflowService : IInspectionWorkflowService
{
    private readonly ICameraService cameraService;
    private readonly IInspectionService inspectionService;
    private readonly IProductionStatisticsService statisticsService;
    private readonly IRecipeService recipeService;
    private readonly ILogService logService;

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

    public async Task<InspectionResult> RunSingleAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        IsRunning = true;

        try
        {
            var recipe = await recipeService.LoadCurrentRecipeAsync(cancellationToken);
            await cameraService.ConnectAsync(cancellationToken);
            var frame = await cameraService.SoftwareTriggerAsync(cancellationToken);
            var request = new InspectionRequest(recipe.RecipeName, frame.ImagePath, frame);
            var result = await inspectionService.RunInspectionAsync(request, cancellationToken);

            statisticsService.ApplyResult(result);
            logService.Info($"Inspection workflow completed: {result.BoardId}, result={(result.IsOk ? "OK" : "NG")}");
            return result;
        }
        finally
        {
            IsRunning = false;
        }
    }

    public async Task StopAsync()
    {
        await inspectionService.StopAutoInspectionAsync();
        IsRunning = false;
    }
}
