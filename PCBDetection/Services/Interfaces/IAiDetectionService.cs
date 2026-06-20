using PCBDetection.Models;

namespace PCBDetection.Services.Interfaces;

public interface IAiDetectionService
{
    string Status { get; }

    Task<DeviceStatus> InitializeAsync(RecipeProfile recipe, CancellationToken cancellationToken);

    Task<InspectionResult> DetectAsync(InspectionRequest request, CancellationToken cancellationToken);

    Task<DeviceStatus> ReleaseAsync();
}
