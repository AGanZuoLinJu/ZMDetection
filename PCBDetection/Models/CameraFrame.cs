namespace PCBDetection.Models;

public sealed class CameraFrame
{
    public CameraFrame(string sourceName, DateTime timestamp, string imagePath = "", object? nativeImage = null)
    {
        SourceName = sourceName;
        Timestamp = timestamp;
        ImagePath = imagePath;
        NativeImage = nativeImage;
    }

    public string SourceName { get; }

    public DateTime Timestamp { get; }

    public string ImagePath { get; }

    public object? NativeImage { get; }
}
