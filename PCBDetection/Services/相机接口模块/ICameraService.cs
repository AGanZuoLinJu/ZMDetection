namespace PCBDetection.Services;

using PCBDetection.Models;

public interface ICameraService
{
    event EventHandler<CameraFrame>? ImageCaptured;
    string CameraName { get; }
    string SerialNumber { get; }
    bool ConnectionStatus { get; }
    object? GetImage { get;}
    Task InitializeAsync(CancellationToken cancellationToken);
    Task DestroyCamera(CancellationToken cancellationToken);
    Task StartGrabbingAsync(CancellationToken cancellationToken);
    Task StopGrabbingAsync(CancellationToken cancellationToken);
    Task<CameraFrame> GetOneFrameImageAsync(CancellationToken cancellationToken);
}
