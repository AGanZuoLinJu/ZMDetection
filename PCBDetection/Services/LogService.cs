using PCBDetection.Models;
using PCBDetection.Services.Interfaces;
using System.IO;

namespace PCBDetection.Services;

public sealed class LogService : ILogService
{
    private const int HistoryLimit = 80;
    private readonly object historySync = new();
    private readonly List<LogItem> history = new();

    public event EventHandler<LogItem>? LogAdded;

    public IReadOnlyList<LogItem> History
    {
        get
        {
            lock (historySync)
            {
                return history.ToArray();
            }
        }
    }
    public void Info(string message)
    {
        Add("INFO", message);
    }
    public void Warning(string message)
    {
        Add("WARN", message);
    }
    public void Error(string message)
    {
        Add("ERROR", message);
    }
    public void Camera(string message)
    {
        Add("CAMERA", message);
    }
    public void Communication(string message)
    {
        Add("COMM", message);
    }

    public void Defect(string message)
    {
        Add(level: "DEFECT", message);
    }
    private void Add(string level, string message)
    {
        var logItem = new LogItem(DateTime.Now, level, message);

        lock (historySync)
        {
            history.Add(logItem);
            if (history.Count > HistoryLimit)
            {
                history.RemoveAt(0);
            }
        }

        LogAdded?.Invoke(this, logItem);
        WriteToDisk(logItem);
    }

    private static void WriteToDisk(LogItem logItem)
    {
        try
        {
            var logRoot = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Record", "OperationLog");
            Directory.CreateDirectory(logRoot);
            var filePath = Path.Combine(logRoot, $"{DateTime.Now:yyyyMMdd}.log");
            File.AppendAllText(filePath, $"{logItem.Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{logItem.Level}] {logItem.Message}{Environment.NewLine}");
        }
        catch
        {
            // Logging must never interrupt inspection flow.
        }
    }
}
