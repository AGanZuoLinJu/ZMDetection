using PCBDetection.Models;

namespace PCBDetection.Services;

public interface IAIDetectionService
{
    bool Status { get; }
    Task InitializeAsync(CancellationToken cancellationToken);
    Task<InspectionResult> DetectAsync(object img,CancellationToken cancellationToken);
    Task ReleaseAsync();
}
