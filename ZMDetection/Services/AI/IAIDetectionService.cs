using ZMDetection.Models;

namespace ZMDetection.Services;

public interface IAIDetectionService
{
    bool Status { get; }
    Task InitializeAsync(CancellationToken cancellationToken);
    Task<InspectionResult> DetectAsync(object img,string id,CancellationToken cancellationToken);
    Task ReleaseAsync();
}
