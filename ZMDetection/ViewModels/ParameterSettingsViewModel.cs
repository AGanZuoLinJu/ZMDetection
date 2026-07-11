using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows;
using Microsoft.Xaml.Behaviors;
using Prism.Commands;
using Prism.Mvvm;
using ZMDetection.EventAggregator;
using ZMDetection.Models;
using ZMDetection.Services;
using ZMDetection.Tools;

namespace ZMDetection.ViewModels;

public sealed class ParameterSettingsViewModel : BindableBase
{
    private readonly IParamService paramService;
    private readonly ILogService logService;
    private readonly IEventAggregator eventAggregator;
    private readonly IAddRecipeService addRecipeService;

    private string currentRecipeName = string.Empty;
    private double defectLong;
    private double defectWidth;
    private double defectHeight;
    private string plcIpAddress = string.Empty;
    private int plcPort;
    private int mesPort;
    private int lightSource;
    private int cameraExposureTime;
    private int cameraGain;
    private double cameraXCalibration;
    private double cameraYCalibration;
    private string statusMessage = "参数已加载";
    private int selectedIndex;

    public ParameterSettingsViewModel(
        IParamService paramService,
        ILogService logService,
        IEventAggregator eventAggregator,
        IAddRecipeService addRecipeService)
    {
        this.paramService = paramService;
        this.logService = logService;
        this.eventAggregator = eventAggregator;
        this.addRecipeService = addRecipeService;

        SaveCommand = new DelegateCommand(SaveParam);
        AddRecipeCommand = new DelegateCommand(AddNewRecipe);
        LoadParam();
    }
    public ObservableCollection<string> RecipeNames { get; } = new();
    public string CurrentRecipeName
    {
        get => currentRecipeName;
        set
        {
            SetProperty(ref currentRecipeName, value);
        }
    }
    public double DefectLong
    {
        get => defectLong;
        set => SetProperty(ref defectLong, value);
    }
    public double DefectWidth
    {
        get => defectWidth;
        set => SetProperty(ref defectWidth, value);
    }
    public double DefectHeight
    {
        get => defectHeight;
        set => SetProperty(ref defectHeight, value);
    }
    public string PlcIpAddress
    {
        get => plcIpAddress;
        set => SetProperty(ref plcIpAddress, value);
    }
    public int PlcPort
    {
        get => plcPort;
        set => SetProperty(ref plcPort, value);
    }
    public int MesPort
    {
        get => mesPort;
        set => SetProperty(ref mesPort, value);
    }
    public int LightSource
    {
        get => lightSource;
        set => SetProperty(ref lightSource, value);
    }
    public int CameraExposureTime
    {
        get => cameraExposureTime;
        set => SetProperty(ref cameraExposureTime, value);
    }
    public int CameraGain
    {
        get => cameraGain;
        set => SetProperty(ref cameraGain, value);
    }
    public double CameraXCalibration
    {
        get => cameraXCalibration;
        set => SetProperty(ref cameraXCalibration, value);
    }
    public double CameraYCalibration
    {
        get => cameraYCalibration;
        set => SetProperty(ref cameraYCalibration, value);
    }
    public string StatusMessage
    {
        get => statusMessage;
        private set => SetProperty(ref statusMessage, value);
    }
    public int SelectedIndex
    {
        get => selectedIndex;
        set
        {
            if(SetProperty(ref selectedIndex, value))
            {
                SelectionChangedAction();
            }
        }
    }
    public DelegateCommand SaveCommand { get; }
    public DelegateCommand AddRecipeCommand { get; }
    private void AddNewRecipe()
    {
        addRecipeService.ShowDialog(Application.Current.MainWindow);
        RecipeNames.Clear();
        foreach (var recipe in RecipeParam.RecipeParamConfig!.AllRecipeName ?? Enumerable.Empty<RecipeParam.RecipeEntry>())
        {
            if (!string.IsNullOrWhiteSpace(recipe.RecipeName))
            {
                RecipeNames.Add(recipe.RecipeName!);
            }
        }
        CurrentRecipeName = RecipeParam.RecipeParamConfig.CurrentRecipeName ?? string.Empty;
    }
    private void LoadParam()
    {
        RecipeNames.Clear();
        foreach (var recipe in RecipeParam.RecipeParamConfig!.AllRecipeName ?? Enumerable.Empty<RecipeParam.RecipeEntry>())
        {
            if (!string.IsNullOrWhiteSpace(recipe.RecipeName))
            {
                RecipeNames.Add(recipe.RecipeName!);
            }
        }

        CurrentRecipeName = RecipeParam.RecipeParamConfig.CurrentRecipeName ?? string.Empty;

        DefectLong = VisionParam.VisionParamConfig.DefectLong;
        DefectWidth = VisionParam.VisionParamConfig.DefectWidth;
        DefectHeight = VisionParam.VisionParamConfig.DefectHeight;

        PlcIpAddress = CommunicationParam.CommParamConfig.PLCIPAddress;
        PlcPort = CommunicationParam.CommParamConfig.PLCPort;
        MesPort = CommunicationParam.CommParamConfig.MESPort;

        LightSource = HardwareParam.HardwareParamConfig.LightSource;
        CameraExposureTime = HardwareParam.HardwareParamConfig.CamExposureTime;
        CameraGain = HardwareParam.HardwareParamConfig.CamGian;
        CameraXCalibration = HardwareParam.HardwareParamConfig.CamXCalibration;
        CameraYCalibration = HardwareParam.HardwareParamConfig.CamYCalibration;
    }
    /// <summary>
    /// 保存参数
    /// </summary>
    private void SaveParam()
    {
        try
        {
            if (SelectedIndex == 0)
            {
                SaveRecipeParam();
                MessageBox.Show(Application.Current.MainWindow, "机种参数保存成功!");
                StatusMessage = $"机种参数保存成功 · {DateTime.Now:HH:mm:ss}";
                eventAggregator.GetEvent<RecipeChangedEvent>().Publish();               //机种变化之后通知其他界面更改
            }
            else if (SelectedIndex == 1)
            {
                SaveVisionParam();
                MessageBox.Show(Application.Current.MainWindow, "视觉参数保存成功!");
                StatusMessage = $"视觉参数保存成功 · {DateTime.Now:HH:mm:ss}";
            }
            else if (SelectedIndex == 2)
            {
                if (string.IsNullOrWhiteSpace(PlcIpAddress))
                {
                    StatusMessage = "PLC IP 地址不能为空";
                    return;
                }
                if (PlcPort is < 1 or > 65535 || MesPort is < 1 or > 65535)
                {
                    StatusMessage = "通讯端口必须在 1 至 65535 之间";
                    return;
                }
                SaveCommunicationParam();
                MessageBox.Show(Application.Current.MainWindow, "通讯参数保存成功!");
                StatusMessage = $"通讯参数保存成功 · {DateTime.Now:HH:mm:ss}";
            }
            else if (SelectedIndex == 3)
            {
                if (LightSource is < 0 or > 255)
                {
                    StatusMessage = "光源亮度必须在 0 至 255 之间";
                    return;
                }
                SaveHardwareParam();
                MessageBox.Show(Application.Current.MainWindow, "硬件参数保存成功!");
                StatusMessage = $"硬件参数保存成功 · {DateTime.Now:HH:mm:ss}";
            }
        }
        catch (Exception ex)
        {
            logService.Error(LogCategory.Running, "参数保存失败", ex);
            StatusMessage = "参数保存失败，请查看运行日志";
        }
    }
    private void SelectionChangedAction()
    {
        paramService.LoadVisionParam();
        LoadParam();
    }
    /// <summary>
    /// 保存视觉参数
    /// </summary>
    private void SaveVisionParam()
    {
        string? recipeName = RecipeParam.RecipeParamConfig!.CurrentRecipeName;
        string recipePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Param", recipeName);
        if (!Directory.Exists(recipePath))
        {
            Directory.CreateDirectory(recipePath);
        }

        VisionParam.VisionParamConfig.RecipeName = recipeName;

        VisionParam.VisionParamConfig.DefectLong = DefectLong;
        VisionParam.VisionParamConfig.DefectWidth = DefectWidth;
        VisionParam.VisionParamConfig.DefectHeight = DefectHeight;

        XMLHelper.Save(VisionParam.VisionParamConfig, paramService.VisionParamFilePath);
    }
    /// <summary>
    /// 保存机种参数
    /// </summary>
    private void SaveRecipeParam()
    {
        RecipeParam.RecipeParamConfig!.CurrentRecipeName = CurrentRecipeName.Trim();
        RecipeParam.RecipeParamConfig!.AllRecipeName ??= new List<RecipeParam.RecipeEntry>();
        if (!RecipeParam.RecipeParamConfig!.AllRecipeName.Any(recipe =>
                string.Equals(recipe.RecipeName,RecipeParam.RecipeParamConfig!.CurrentRecipeName,StringComparison.OrdinalIgnoreCase)))
        {
            RecipeParam.RecipeParamConfig!.AllRecipeName.Add(new RecipeParam.RecipeEntry
            {
                RecipeName = RecipeParam.RecipeParamConfig!.CurrentRecipeName
            });
            RecipeNames.Add(RecipeParam.RecipeParamConfig!.CurrentRecipeName);
        }

        XMLHelper.Save(RecipeParam.RecipeParamConfig!, paramService.RecipeParamFilePath);
    }
    /// <summary>
    /// 保存硬件参数
    /// </summary>
    private void SaveHardwareParam()
    {
        HardwareParam.HardwareParamConfig.LightSource = LightSource;
        HardwareParam.HardwareParamConfig.CamExposureTime = CameraExposureTime;
        HardwareParam.HardwareParamConfig.CamGian = CameraGain;
        HardwareParam.HardwareParamConfig.CamXCalibration = CameraXCalibration;
        HardwareParam.HardwareParamConfig.CamYCalibration = CameraYCalibration;

        XMLHelper.Save(HardwareParam.HardwareParamConfig, paramService.HardParamFilePath);
    }
    /// <summary>
    /// 保存通讯参数
    /// </summary>
    private void SaveCommunicationParam()
    {
        CommunicationParam.CommParamConfig.PLCIPAddress = PlcIpAddress.Trim();
        CommunicationParam.CommParamConfig.PLCPort = PlcPort;
        CommunicationParam.CommParamConfig.MESPort = MesPort;
        XMLHelper.Save(CommunicationParam.CommParamConfig, paramService.CommunicationParamFilePath);
    }
}
