using PCBDetection.Models;

namespace PCBDetection.Services;

public interface IAIDetectionService
{
    bool Status { get; }

    Task InitializeAsync(RecipeProfile recipe, CancellationToken cancellationToken);

    Task<InspectionResult> DetectAsync(InspectionRequest request, CancellationToken cancellationToken);

    Task ReleaseAsync();
}
