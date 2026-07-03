namespace PCBDetection.Services;

using PCBDetection.Models;

public interface ICameraService
{
    event EventHandler<object>? ImageCaptured;
    string CameraName { get; }
    string SerialNumber { get; }
    bool ConnectionStatus { get; }
    object? GetImage { get;}
    Task InitializeAsync(CancellationToken cancellationToken);
    Task DestroyCamera(CancellationToken cancellationToken);
    Task StartGrabbingAsync(CancellationToken cancellationToken);
    Task StopGrabbingAsync(CancellationToken cancellationToken);
    Task<object> GetOneFrameImageAsync(CancellationToken cancellationToken);
}
