using PCBDetection.Models;
using PCBDetection.Services.Interfaces;

namespace PCBDetection.Hardware;

public sealed class VendorCameraService : ICameraService
{
    public event EventHandler<CameraFrame>? ImageCaptured;

    public string CameraName => "Vendor Camera";

    public string ConnectionStatus { get; private set; } = "Not wired";

    public Task<DeviceStatus> InitializeAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ConnectionStatus = "Pending";
        return Task.FromResult(new DeviceStatus(CameraName, ConnectionStatus, "HK/DALSA adapter boundary is ready; SDK calls are not enabled yet"));
    }

    public Task<DeviceStatus> ConnectAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(new DeviceStatus(CameraName, ConnectionStatus, "Real camera connection will be migrated from the old SDK module"));
    }

    public Task<DeviceStatus> DisconnectAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ConnectionStatus = "Offline";
        return Task.FromResult(new DeviceStatus(CameraName, ConnectionStatus));
    }

    public Task<DeviceStatus> StartGrabbingAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(new DeviceStatus(CameraName, ConnectionStatus, "Real grabbing is not enabled in this migration baseline"));
    }

    public Task<DeviceStatus> StopGrabbingAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(new DeviceStatus(CameraName, ConnectionStatus));
    }

    public Task<CameraFrame> SoftwareTriggerAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var frame = new CameraFrame(CameraName, DateTime.Now);
        ImageCaptured?.Invoke(this, frame);
        return Task.FromResult(frame);
    }

    public string CapturePreview()
    {
        return "Vendor camera service is registered but still uses the migration boundary.";
    }
}
