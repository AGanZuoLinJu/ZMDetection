using PCBDetection.Models;

namespace PCBDetection.Services.Interfaces;

public interface IInspectionWorkflowService
{
    bool IsRunning { get; }

    Task<InspectionResult> RunSingleAsync(CancellationToken cancellationToken);

    Task StopAsync();
}
