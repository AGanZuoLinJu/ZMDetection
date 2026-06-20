using PCBDetection.Models;

namespace PCBDetection.Services.Interfaces;

public interface IInspectionService
{
    event EventHandler<InspectionResult>? InspectionCompleted;

    InspectionResult RunInspection();

    Task<InspectionResult> RunInspectionAsync(InspectionRequest request, CancellationToken cancellationToken);

    Task StartAutoInspectionAsync(CancellationToken cancellationToken);

    Task StopAutoInspectionAsync();

    void Reset();
}
