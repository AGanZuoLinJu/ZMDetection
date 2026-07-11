namespace ZMDetection.Models;

public sealed class StartupProgress
{
    public StartupProgress(int percentage, string step, string message, bool isError = false)
    {
        Percentage = percentage;
        Step = step;
        Message = message;
        IsError = isError;
    }
    /// <summary>
    /// 进度条百分比
    /// </summary>
    public int Percentage { get; }
    /// <summary>
    /// 当前步骤
    /// </summary>
    public string Step { get; }
    /// <summary>
    /// 当前信息
    /// </summary>
    public string Message { get; }

    public bool IsError { get; }
}
