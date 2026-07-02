namespace PCBDetection.Services;

public interface ICameraManager
{
    IReadOnlyList<ICameraService> Cameras { get; }

    ICameraService GetCamera(string cameraName);

    bool TryGetCamera(string cameraName, out ICameraService? camera);
}
