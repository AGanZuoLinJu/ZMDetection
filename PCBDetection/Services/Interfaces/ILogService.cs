using PCBDetection.Models;

namespace PCBDetection.Services.Interfaces;

public interface ILogService
{
    event EventHandler<LogItem>? LogAdded;

    void Info(string message);

    void Warning(string message);

    void Error(string message);

    void Camera(string message);

    void Communication(string message);

    void Defect(string message);
}
