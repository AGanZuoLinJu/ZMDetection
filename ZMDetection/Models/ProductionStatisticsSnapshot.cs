namespace ZMDetection.Models;

public sealed class ProductionStatisticsSnapshot
{
    public ProductionStatisticsSnapshot(
        DateTime date,
        int okCount,
        int ngCount,
        int defectCount,
        double lastCycleTimeMilliseconds,
        double totalCycleTimeMilliseconds,
        IReadOnlyDictionary<string, int>? defectsByName = null)
    {
        Date = date.Date;
        OkCount = okCount;
        NgCount = ngCount;
        DefectCount = defectCount;
        LastCycleTimeMilliseconds = lastCycleTimeMilliseconds;
        TotalCycleTimeMilliseconds = totalCycleTimeMilliseconds;
        DefectsByName = defectsByName ?? new Dictionary<string, int>();
    }

    public DateTime Date { get; }

    public int OkCount { get; }

    public int NgCount { get; }

    public int DefectCount { get; }

    public double LastCycleTimeMilliseconds { get; }

    public double TotalCycleTimeMilliseconds { get; }

    public IReadOnlyDictionary<string, int> DefectsByName { get; }

    public int TotalCount => OkCount + NgCount;

    public double YieldRate => TotalCount == 0 ? 100 : Math.Round(OkCount * 100d / TotalCount, 2);

    public double AverageCycleTimeMilliseconds =>
        TotalCount == 0 ? 0 : Math.Round(TotalCycleTimeMilliseconds / TotalCount, 1);
}
