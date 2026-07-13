using ZMDetection.Models;

namespace ZMDetection.Services;

public interface IProductionStatisticsService
{
    event EventHandler<ProductionStatisticsChangedEventArgs>? StatisticsChanged;

    ProductionStatisticsSnapshot Current { get; }

    Task LoadAsync(CancellationToken cancellationToken);

    ProductionStatisticsSnapshot GetByDate(DateTime date);

    IReadOnlyList<ProductionStatisticsSnapshot> GetRange(DateTime endDate, int days);

    void ApplyResult(InspectionResult result);

    void Reset();

    Task SaveCsvAsync(string filePath, CancellationToken cancellationToken);
}
