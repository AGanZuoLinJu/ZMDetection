namespace PCBDetection.Models;

public sealed class RecipeProfile
{
    public RecipeProfile(
        string recipeName,
        string visionConfigPath = "",
        string hardwareConfigPath = "",
        string communicationConfigPath = "")
    {
        RecipeName = recipeName;
        VisionConfigPath = visionConfigPath;
        HardwareConfigPath = hardwareConfigPath;
        CommunicationConfigPath = communicationConfigPath;
    }

    public string RecipeName { get; }

    public string VisionConfigPath { get; }

    public string HardwareConfigPath { get; }

    public string CommunicationConfigPath { get; }
}
