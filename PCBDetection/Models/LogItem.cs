namespace PCBDetection.Models;

public sealed class LogItem
{
    public LogItem(DateTime timestamp, string level, string message)
    {
        Timestamp = timestamp;
        Level = level;
        Message = message;
    }

    public DateTime Timestamp { get; }

    public string Level { get; }

    public string Message { get; }
}
