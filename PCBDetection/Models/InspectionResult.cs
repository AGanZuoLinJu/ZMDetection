namespace PCBDetection.Models;

public sealed class InspectionResult
{
    public InspectionResult(
        string id,
        bool isOk,
        int defectCount,
        long cycleTimeMilliseconds,
        object? resultImg = null,
        string message = "",
        IReadOnlyList<DefectDetail>? defects = null)
    {
        ID = id;
        IsOk = isOk;
        DefectCount = defectCount;
        CycleTimeMilliseconds = cycleTimeMilliseconds;
        Message = message;
        Defects = defects ?? Array.Empty<DefectDetail>();
        ResultImage = resultImg;
    }

    public string ID { get; }
    public bool IsOk { get; }
    public int DefectCount { get; }
    public long CycleTimeMilliseconds { get; }
    public string Message { get; }
    public IReadOnlyList<DefectDetail> Defects { get; }
    public object? ResultImage { get; }
}
