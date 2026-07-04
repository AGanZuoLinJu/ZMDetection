using PCBDetection.Models;

namespace PCBDetection.Services;

/// <summary>
/// 检测服务
/// </summary>
public interface IInspectionService
{
    bool Status { get; }
    Task InitializeAsync(CancellationToken cancellationToken);
    Task<InspectionResult> RunInspectionAsync(object inputImg,CancellationToken cancellationToken);
    Task ReleaseAsync();
}
