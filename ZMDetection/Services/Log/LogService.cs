using System.IO;
using ZMDetection.Models;

namespace ZMDetection.Services;

public sealed class LogService : ILogService
{
    private const int HistoryLimit = 80;                    //界面允许最大保留80条
    private readonly object historySync = new();
    private readonly object diskSync = new();
    private readonly Dictionary<LogCategory, List<LogItem>> histories = new();

    public event EventHandler<LogItem>? LogAdded;
    public IReadOnlyList<LogItem> GetHistory(LogCategory category)
    {
        lock (historySync)
        {
            return histories.TryGetValue(category, out List<LogItem>? history)
                ? history.ToArray()
                : Array.Empty<LogItem>();
        }
    }
    public void Info(LogCategory category, string message) => Add(category, "INFO", message);
    public void Warning(LogCategory category, string message) => Add(category, "WARN", message);
    public void Error(LogCategory category, string message) => Add(category, "ERROR", message);
    public void Error(LogCategory category, string message, Exception e) => Add(category, "ERROR", $"{message} 错误:{e.Message}");
    private void Add(LogCategory category, string level, string message)
    {
        var logItem = new LogItem(DateTime.Now, category, level, message);

        lock (historySync)
        {
            if (!histories.TryGetValue(category, out List<LogItem>? history))
            {
                history = new List<LogItem>();
                histories.Add(category, history);
            }

            history.Add(logItem);
            if (history.Count > HistoryLimit)
            {
                history.RemoveAt(0);
            }
        }

        LogAdded?.Invoke(this, logItem);
        WriteToDisk(logItem);
    }
    /// <summary>
    /// 日志写入本地文件
    /// </summary>
    /// <param name="logItem"></param>
    private void WriteToDisk(LogItem logItem)
    {
        try
        {
            lock (diskSync)
            {
                string logRoot = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Record", GetFolderName(logItem.Category));
                Directory.CreateDirectory(logRoot);
                string filePath = Path.Combine(logRoot, $"{DateTime.Now:yyyyMMdd}.log");
                File.AppendAllText(filePath,
                    $"{logItem.Timestamp:yyyy-MM-dd HH:mm:ss.fff} " +
                    $"[{logItem.Level}] {logItem.Message}{Environment.NewLine}");
            }
        }
        catch
        {
        }
    }

    private static string GetFolderName(LogCategory category)
    {
        string logName = category switch
        {
            LogCategory.Camera => "CameraLog",
            LogCategory.Defect => "DefectLog",
            LogCategory.Communication => "CommunicationLog",
            _ => "RunningLog"
        };
        return logName;
    }
}
