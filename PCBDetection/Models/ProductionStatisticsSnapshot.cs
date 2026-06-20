namespace PCBDetection.Models;

public sealed class ProductionStatisticsSnapshot
{
    public ProductionStatisticsSnapshot(int okCount, int ngCount, int defectCount, double lastCycleTimeMilliseconds)
    {
        OkCount = okCount;
        NgCount = ngCount;
        DefectCount = defectCount;
        LastCycleTimeMilliseconds = lastCycleTimeMilliseconds;
    }

    public int OkCount { get; }

    public int NgCount { get; }

    public int DefectCount { get; }

    public double LastCycleTimeMilliseconds { get; }

    public int TotalCount => OkCount + NgCount;

    public double YieldRate => TotalCount == 0 ? 100 : Math.Round(OkCount * 100d / TotalCount, 1);
}
