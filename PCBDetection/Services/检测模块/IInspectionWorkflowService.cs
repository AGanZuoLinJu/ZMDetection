using PCBDetection.Models;

namespace PCBDetection.Services;

public interface IInspectionWorkflowService
{
    bool IsRunning { get; }
    Task<InspectionResult> StartRunAsync(CancellationToken cancellationToken);
    Task StopAsync();
}
