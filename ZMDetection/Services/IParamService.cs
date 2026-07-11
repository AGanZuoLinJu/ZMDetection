using ZMDetection.Models;

namespace ZMDetection.Services;

public interface IParamService
{
    string RecipeParamFilePath { get; }
    string VisionParamFilePath { get; }
    string CommunicationParamFilePath { get; }
    string HardParamFilePath { get; }
    bool LoadRecipeParam();
    bool LoadVisionParam();
    bool LoadCommunicationParam();
    bool LoadHardwareParam();
    Task<bool> InitAllParam(CancellationToken cancellationToken);
}
