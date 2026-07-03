using System.IO;
using PCBDetection.Models;
using PCBDetection.Tools;
using static PCBDetection.Models.RecipeParam;

namespace PCBDetection.Services;

public sealed class ParamService : IParamService
{
    private readonly ILogService logService;
    public string RecipeParamFilePath => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Param", "RecipeParam.xml");
    public string VisionParamFilePath => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Param", RecipeParam.RecipeParamConfig!.CurrentRecipeName,"VisionParam.xml");
    public string CommunicationParamFilePath => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Param", "CommunicationParam.xml");
    public string HardParamFilePath => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Param", "HardwareParam.xml");
    public ParamService(ILogService logService)
    {
        this.logService = logService;
    }
    public bool LoadCommunicationParam()
    {
        try
        {
            if (!File.Exists(CommunicationParamFilePath))
            {
                XMLHelper.Save<CommunicationParam>(CommunicationParam.CommParamConfig, CommunicationParamFilePath);
            }
            else
            {
                CommunicationParam.CommParamConfig = XMLHelper.Load<CommunicationParam>(CommunicationParamFilePath);
            }
            return true;
        }
        catch(Exception ex)
        {
            logService.Error(LogCategory.Running, "通讯参数加载失败!", ex);
            return false;
        }
    }
    public bool LoadHardwareParam()
    {
        try
        {
            if (!File.Exists(HardParamFilePath))
            {
                XMLHelper.Save<HardwareParam>(HardwareParam.HardwareParamConfig, HardParamFilePath);
            }
            else
            {
                HardwareParam.HardwareParamConfig = XMLHelper.Load<HardwareParam>(HardParamFilePath);
            }
            return true;
        }
        catch (Exception ex)
        {
            logService.Error(LogCategory.Running,"硬件参数加载错误!" + ex.Message);
            return false;
        }
        
    }
    public bool LoadRecipeParam()
    {
        try
        {
            RecipeParam.RecipeParamConfig!.AllRecipeName = new List<RecipeEntry>();
            //没有配置文件时新建
            if (!File.Exists(RecipeParamFilePath))
            {
                RecipeParam.RecipeParamConfig.CurrentRecipeName = "AAAAAA";
                RecipeParam.RecipeParamConfig.AllRecipeName.Clear();
                RecipeParam.RecipeParamConfig.AllRecipeName.Add(new RecipeEntry());

                CurrentRecipeInfo = RecipeParam.RecipeParamConfig.AllRecipeName[0];
                XMLHelper.Save<RecipeParam>(RecipeParam.RecipeParamConfig, RecipeParamFilePath);
            }
            else
            {
                RecipeParamConfig = XMLHelper.Load<RecipeParam>(RecipeParamFilePath);
                foreach (var recipe in RecipeParamConfig.AllRecipeName!)
                {
                    if (recipe.RecipeName == RecipeParamConfig.CurrentRecipeName)
                    {
                        CurrentRecipeInfo = recipe;
                    }
                }
            }
            return true;
        }
        catch(Exception ex)
        {
            logService.Error(LogCategory.Running, "机种参数加载错误!",ex);
            return false;
        }
    }
    public bool LoadVisionParam()
    {
        try
        {
            string recipeName = RecipeParam.RecipeParamConfig!.CurrentRecipeName!;
            string recipePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Param", recipeName);
            if (!Directory.Exists(recipePath))
            {
                Directory.CreateDirectory(recipePath);
            }
            if (!File.Exists(VisionParamFilePath))
            {
                XMLHelper.Save<VisionParam>(VisionParam.VisionParamConfig, VisionParamFilePath);
            }
            VisionParam.VisionParamConfig = XMLHelper.Load<VisionParam>(VisionParamFilePath);

            return true;
        }
        catch(Exception ex)
        {
            logService.Error(LogCategory.Running,"视觉参数加载错误!",ex);
            return false;
        }
    }
    public Task<bool> InitAllParam(CancellationToken cancellationToken)
    {
        return Task.Run<bool>(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();

            bool result = true;
            //创建Param文件夹
            string paramPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Param");
            if (!Directory.Exists(paramPath))
            {
                Directory.CreateDirectory(paramPath);
            }

            result &= LoadRecipeParam();                //先加载机种参数 
            result &= LoadVisionParam();
            result &= LoadCommunicationParam();
            result &= LoadHardwareParam();
            return result;
        });
    }
}
