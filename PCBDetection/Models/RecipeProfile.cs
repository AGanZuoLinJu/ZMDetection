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
    /// <summary>
    /// 机种名
    /// </summary>
    public string RecipeName { get; }
    /// <summary>
    /// 视觉参数路径
    /// </summary>
    public string VisionConfigPath { get; }
    /// <summary>
    /// 硬件参数路径
    /// </summary>
    public string HardwareConfigPath { get; }
    /// <summary>
    /// 通讯参数路径
    /// </summary>
    public string CommunicationConfigPath { get; }
}
