using PCBDetection.Models;

namespace PCBDetection.Services;

public sealed class MockInspectionService : IInspectionService
{
    private readonly IAIDetectionService aiDetectionService;
    private RecipeProfile? currentRecipe;

    public MockInspectionService(IAIDetectionService aiDetectionService)
    {
        this.aiDetectionService = aiDetectionService;
    }
    public bool Status { get; private set; }
    public async Task InitializeAsync(
        RecipeProfile recipe,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (Status &&
            currentRecipe?.RecipeName == recipe.RecipeName)
        {
            return;
        }

        Status = false;

        if (aiDetectionService.Status)
        {
            await aiDetectionService.ReleaseAsync();
        }

        await aiDetectionService.InitializeAsync(recipe, cancellationToken);

        currentRecipe = recipe;
        Status = true;
    }

    public async Task<InspectionResult> RunInspectionAsync(
        InspectionRequest request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!Status || currentRecipe == null)
        {
            throw new InvalidOperationException("检测服务尚未初始化。");
        }

        return await aiDetectionService.DetectAsync(request, cancellationToken);
    }

    public async Task ReleaseAsync()
    {
        if (aiDetectionService.Status)
        {
            await aiDetectionService.ReleaseAsync();
        }

        currentRecipe = null;
        Status = false;
    }
}
