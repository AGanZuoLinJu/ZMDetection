using PCBDetection.Models;
using PCBDetection.Services.Interfaces;

namespace PCBDetection.Services;

public sealed class MockAiDetectionService : IAiDetectionService
{
    public string Status { get; private set; } = "Not initialized";

    public Task<DeviceStatus> InitializeAsync(RecipeProfile recipe, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Status = "Ready";
        return Task.FromResult(new DeviceStatus("AI", Status, $"Mock model loaded for {recipe.RecipeName}"));
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

    public Task<DeviceStatus> ReleaseAsync()
    {
        Status = "Released";
        return Task.FromResult(new DeviceStatus("AI", Status));
    }
}
