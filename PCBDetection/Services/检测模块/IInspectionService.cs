using PCBDetection.Models;

namespace PCBDetection.Services;

/// <summary>
/// 检测服务
/// </summary>
public interface IInspectionService
{
    bool Status { get; }
    Task InitializeAsync(RecipeProfile recipe, CancellationToken cancellationToken);
    Task<InspectionResult> RunInspectionAsync(
        InspectionRequest request,
        CancellationToken cancellationToken);
    Task ReleaseAsync();
}
