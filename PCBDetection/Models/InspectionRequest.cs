namespace PCBDetection.Models;

public sealed class InspectionRequest
{
    public InspectionRequest(string recipeName, string panelId = "", CameraFrame? frame = null)
    {
        RecipeName = recipeName;
        PanelId = panelId;
        Frame = frame;
    }

    public string RecipeName { get; }

    public string PanelId { get; }

    public CameraFrame? Frame { get; }
}
