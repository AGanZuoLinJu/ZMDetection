using PCBDetection.Models;
using PCBDetection.Services.Interfaces;

namespace PCBDetection.Services;

public sealed class MockAiDetectionService : IDetectionService
{
    public bool Status { get; private set; } = false;

    public Task InitializeAsync(RecipeProfile recipe, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.Run(() =>
        {
            Status = true;
        });
    }

    public Task<InspectionResult> DetectAsync(InspectionRequest request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(new InspectionResult(
            string.IsNullOrWhiteSpace(request.PanelId) ? $"PCB-{DateTime.Now:yyyyMMdd-HHmmss}" : request.PanelId,
            true,
            0,
            0,
            request.RecipeName,
            request.PanelId,
            request.Frame?.ImagePath ?? string.Empty,
            "Mock AI detection passed"));
    }

    public Task ReleaseAsync()
    {
        return Task.Run(() =>
        {
            Status = false;
        });
    }
}
