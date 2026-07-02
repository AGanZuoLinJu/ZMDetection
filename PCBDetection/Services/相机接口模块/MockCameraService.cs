using PCBDetection.Models;

namespace PCBDetection.Services;
public sealed class MockCameraService : ICameraService
{
    private readonly string cameraName;
    private bool connectionStatus = false;
    public object? GetImage { get; set; }
    public event EventHandler<CameraFrame>? ImageCaptured;
    public MockCameraService() : this("Ä£ÄâÏà»ú1")
    {
    }
    public MockCameraService(string cameraName)
    {
        this.cameraName = cameraName;
    }

    public string CameraName => cameraName;
    public string SerialNumber => cameraName;
    public bool ConnectionStatus => connectionStatus;
    public Task InitializeAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.Run(() =>
        {
            connectionStatus = true;
        }, cancellationToken);
    }
    public Task DestroyCamera(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.Run(async () =>
        {
            if (connectionStatus)
            {
                await Task.Delay(1000);
                connectionStatus = false;
            }
        }, cancellationToken);
    }
    public Task StartGrabbingAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.Run(async () =>
        {
            if (connectionStatus)
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
            if (connectionStatus)
            {
                await Task.Delay(1000);
            }
        }, cancellationToken);
    }
    public Task<CameraFrame> GetOneFrameImageAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(new CameraFrame(CameraName, DateTime.Now));
    }
    public string CapturePreview()
    {
        return $"Preview frame loaded from {CameraName} ({ConnectionStatus}).";
    }
}
