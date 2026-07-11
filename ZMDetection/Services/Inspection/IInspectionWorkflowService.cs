using ZMDetection.Models;

namespace ZMDetection.Services;

public interface IInspectionWorkflowService
{
    bool IsRunning { get; }
    Task<InspectionResult> StartRunAsync(CancellationToken cancellationToken);
    Task StopAsync();
    bool InitializeCamera();
}
