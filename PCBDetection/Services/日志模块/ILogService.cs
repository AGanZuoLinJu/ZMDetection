using PCBDetection.Models;

namespace PCBDetection.Services;

public interface ILogService
{
    event EventHandler<LogItem>? LogAdded;
    IReadOnlyList<LogItem> GetHistory(LogCategory category);
    void Info(LogCategory category, string message);
    void Warning(LogCategory category, string message);
    void Error(LogCategory category, string message);
}
