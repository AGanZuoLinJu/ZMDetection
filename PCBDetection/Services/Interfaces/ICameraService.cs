namespace PCBDetection.Services.Interfaces;

using PCBDetection.Models;

public interface ICameraService
{
    event EventHandler<CameraFrame>? ImageCaptured;

    string CameraName { get; }

    bool ConnectionStatus { get; }

    Task InitializeAsync(CancellationToken cancellationToken);

    Task ConnectAsync(CancellationToken cancellationToken);

    Task DisconnectAsync(CancellationToken cancellationToken);

    Task StartGrabbingAsync(CancellationToken cancellationToken);

    Task StopGrabbingAsync(CancellationToken cancellationToken);

    Task<CameraFrame> SoftwareTriggerAsync(CancellationToken cancellationToken);

    string CapturePreview();
}
