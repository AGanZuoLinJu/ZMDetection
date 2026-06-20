namespace PCBDetection.Services.Interfaces;

using PCBDetection.Models;

public interface ICameraService
{
    event EventHandler<CameraFrame>? ImageCaptured;

    string CameraName { get; }

    string ConnectionStatus { get; }

    Task<DeviceStatus> InitializeAsync(CancellationToken cancellationToken);

    Task<DeviceStatus> ConnectAsync(CancellationToken cancellationToken);

    Task<DeviceStatus> DisconnectAsync(CancellationToken cancellationToken);

    Task<DeviceStatus> StartGrabbingAsync(CancellationToken cancellationToken);

    Task<DeviceStatus> StopGrabbingAsync(CancellationToken cancellationToken);

    Task<CameraFrame> SoftwareTriggerAsync(CancellationToken cancellationToken);

    string CapturePreview();
}
