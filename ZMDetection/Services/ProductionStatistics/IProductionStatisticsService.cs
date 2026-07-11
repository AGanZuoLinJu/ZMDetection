using ZMDetection.Models;

namespace ZMDetection.Services;

public interface IProductionStatisticsService
{
    ProductionStatisticsSnapshot Current { get; }

    Task LoadAsync(CancellationToken cancellationToken);

    void ApplyResult(InspectionResult result);

    void Reset();

    Task SaveCsvAsync(string filePath, CancellationToken cancellationToken);
}
