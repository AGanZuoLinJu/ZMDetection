using PCBDetection.Models;
using PCBDetection.Services.Interfaces;

namespace PCBDetection.Services.Camera;
/// <summary>
/// 海康面阵相机
/// </summary>
public sealed class HKAreaCameraService : ICameraService
{
    public event EventHandler<CameraFrame>? ImageCaptured;

    public string CameraName => "HK Area Camera";

    public bool ConnectionStatus { get; private set; } = false;

    public Task InitializeAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ConnectionStatus = true;
        return Task.Run(async () =>
        {
            await Task.Delay(1000);
        }, cancellationToken);
    }

    public Task ConnectAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ConnectionStatus = true;
        return Task.Run(async () =>
        {
            await Task.Delay(1000);
        }, cancellationToken);
    }

    public Task DisconnectAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ConnectionStatus = false;
        return Task.Run(async () =>
        {
            await Task.Delay(1000);
        }, cancellationToken);
    }

    public Task StartGrabbingAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.Run(async () =>
        {
            if (ConnectionStatus)
            {
                await Task.Delay(1000);
            }
        }, cancellationToken);
    }

    public Task StopGrabbingAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.Run(async () =>
        {
            if (ConnectionStatus)
            {
                await Task.Delay(1000);
            }
        }, cancellationToken);
    }

    public Task<CameraFrame> SoftwareTriggerAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.Run(() =>
        {
            return new CameraFrame("A", DateTime.Now);
        }, cancellationToken);
    }

    public string CapturePreview()
    {
        return "Vendor camera service is registered but still uses the migration boundary.";
    }
}
