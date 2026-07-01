using PCBDetection.Services.Interfaces;
using Prism.Mvvm;

namespace PCBDetection.Services;

public sealed class ApplicationStatus : BindableBase, IApplicationStatus
{
    private bool cameraStatus = false;
    private bool plcStatus = false;
    private bool mesStatus = false;
    private bool lightStatus = false;
    private bool aiStatus = false;
    private bool isInspectionRunning;
    private string currentRecipe = "PCB_TOP_AOI_V1";

    public bool CameraStatus
    {
        get => cameraStatus;
        private set => SetProperty(ref cameraStatus, value);
    }

    public bool PlcStatus
    {
        get => plcStatus;
        private set => SetProperty(ref plcStatus, value);
    }

    public bool MesStatus
    {
        get => mesStatus;
        private set => SetProperty(ref mesStatus, value);
    }

    public bool LightStatus
    {
        get => lightStatus;
        private set => SetProperty(ref lightStatus, value);
    }

    public bool AiStatus
    {
        get => aiStatus;
        private set => SetProperty(ref aiStatus, value);
    }

    public bool IsInspectionRunning
    {
        get => isInspectionRunning;
        private set => SetProperty(ref isInspectionRunning, value);
    }

    public string CurrentRecipe
    {
        get => currentRecipe;
        private set => SetProperty(ref currentRecipe, value);
    }

    public void SetCameraStatus(bool value) => CameraStatus = value;
    public void SetPlcStatus(bool value) => PlcStatus = value;
    public void SetMesStatus(bool value) => MesStatus = value;
    public void SetLightStatus(bool value) => LightStatus = value;
    public void SetAiStatus(bool value) => AiStatus = value;
    public void SetInspectionRunning(bool value) => IsInspectionRunning = value;
    public void SetCurrentRecipe(string value) => CurrentRecipe = value;
}
