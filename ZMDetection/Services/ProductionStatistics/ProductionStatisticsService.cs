using ZMDetection.Models;
using System.IO;

namespace ZMDetection.Services;

public sealed class ProductionStatisticsService : IProductionStatisticsService
{
    private int okCount;
    private int ngCount;
    private int defectCount;
    private double lastCycleTimeMilliseconds;

    public ProductionStatisticsSnapshot Current => new(okCount, ngCount, defectCount, lastCycleTimeMilliseconds);

    public Task LoadAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.CompletedTask;
    }

    public void ApplyResult(InspectionResult result)
    {
        if (result.IsOk)
        {
            okCount++;
        }
        else
        {
            ngCount++;
        }

        defectCount += result.DefectCount;
        lastCycleTimeMilliseconds = result.CycleTimeMilliseconds;
    }

    public void Reset()
    {
        okCount = 0;
        ngCount = 0;
        defectCount = 0;
        lastCycleTimeMilliseconds = 0;
    }

    public Task SaveCsvAsync(string filePath, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var snapshot = Current;
        var csv = "OK,NG,Defects,Yield,CycleTimeMs" + Environment.NewLine +
                  $"{snapshot.OkCount},{snapshot.NgCount},{snapshot.DefectCount},{snapshot.YieldRate},{snapshot.LastCycleTimeMilliseconds}";
        File.WriteAllText(filePath, csv);
        return Task.CompletedTask;
    }
}
