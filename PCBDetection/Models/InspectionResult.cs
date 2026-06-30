namespace PCBDetection.Models;

public sealed class InspectionResult
{
    public InspectionResult(
        string boardId,
        bool isOk,
        int defectCount,
        double cycleTimeMilliseconds,
        string recipeName = "",
        string panelId = "",
        string imagePath = "",
        string message = "",
        IReadOnlyList<DefectDetail>? defects = null)
    {
        BoardId = boardId;
        IsOk = isOk;
        DefectCount = defectCount;
        CycleTimeMilliseconds = cycleTimeMilliseconds;
        RecipeName = recipeName;
        PanelId = panelId;
        ImagePath = imagePath;
        Message = message;
        Defects = defects ?? Array.Empty<DefectDetail>();
    }

    public string BoardId { get; }

    public bool IsOk { get; }

    public int DefectCount { get; }

    public double CycleTimeMilliseconds { get; }

    public string RecipeName { get; }

    public string PanelId { get; }

    public string ImagePath { get; }

    public string Message { get; }

    public IReadOnlyList<DefectDetail> Defects { get; }
}
