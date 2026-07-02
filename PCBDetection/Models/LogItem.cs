namespace PCBDetection.Models;

public sealed class LogItem
{
    public LogItem(
        DateTime timestamp,
        LogCategory category,
        string level,
        string message)
    {
        Timestamp = timestamp;
        Category = category;
        Level = level;
        Message = message;
    }

    public DateTime Timestamp { get; }
    public LogCategory Category { get; }
    public string Level { get; }
    public string Message { get; }
}
