using PCBDetection.Services.Interfaces;
using PCBDetection.Models;

namespace PCBDetection.Services;

public sealed class MockCameraService : ICameraService
{
    private string connectionStatus = "Offline";

    public event EventHandler<CameraFrame>? ImageCaptured;

    public string CameraName => "LineScan-CAM-01";

    public string ConnectionStatus => connectionStatus;

    public Task<DeviceStatus> InitializeAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        connectionStatus = "Initialized";
        return Task.FromResult(new DeviceStatus(CameraName, connectionStatus, "Mock camera initialized"));
    }

    public Task<DeviceStatus> ConnectAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        connectionStatus = "Online";
        return Task.FromResult(new DeviceStatus(CameraName, connectionStatus, "Mock camera connected"));
    }

    public Task<DeviceStatus> DisconnectAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        connectionStatus = "Offline";
        return Task.FromResult(new DeviceStatus(CameraName, connectionStatus, "Mock camera disconnected"));
    }

    public Task<DeviceStatus> StartGrabbingAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        connectionStatus = "Grabbing";
        return Task.FromResult(new DeviceStatus(CameraName, connectionStatus, "Mock grabbing started"));
    }

    public Task<DeviceStatus> StopGrabbingAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        connectionStatus = "Online";
        return Task.FromResult(new DeviceStatus(CameraName, connectionStatus, "Mock grabbing stopped"));
    }

    public Task<CameraFrame> SoftwareTriggerAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var frame = new CameraFrame(CameraName, DateTime.Now, $"MockFrame-{DateTime.Now:yyyyMMdd-HHmmssfff}.bmp");
        ImageCaptured?.Invoke(this, frame);
        return Task.FromResult(frame);
    }

    public string CapturePreview()
    {
        return $"Preview frame loaded from {CameraName} ({ConnectionStatus}).";
    }
}
