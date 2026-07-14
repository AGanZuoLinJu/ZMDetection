namespace ZMDetection.Models;

public sealed class InspectionResult
{
    public InspectionResult(
        bool isOk,
        int defectCount,
        string id = "",
        long cycleTimeMilliseconds = 0,
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

    public string ID { get; set; }
    public bool IsOk { get; }
    public int DefectCount { get; }
    public long CycleTimeMilliseconds { get; set; }
    public string Message { get; }
    public IReadOnlyList<DefectDetail> Defects { get; }
    public object? ResultImage { get; }
}
