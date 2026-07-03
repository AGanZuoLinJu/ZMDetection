using Prism.Mvvm;

namespace PCBDetection.Services;

public sealed class ApplicationStatus : BindableBase, IApplicationStatus
{
    private bool cameraStatus = false;
    private bool plcStatus = false;
    private bool mesStatus = false;
    private bool lightStatus = false;
    private bool detectionStatus = false;
    private bool paramStatus = false;
    private bool isInspectionRunning;

    public bool ParamStatus
    {
        get => paramStatus;
        private set => SetProperty(ref paramStatus, value);
    }
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
    public bool DetectionStatus
    {
        get => detectionStatus;
        private set => SetProperty(ref detectionStatus, value);
    }
    public bool IsInspectionRunning
    {
        get => isInspectionRunning;
        private set => SetProperty(ref isInspectionRunning, value);
    }

    public void SetCameraStatus(bool value) => CameraStatus = value;
    public void SetPlcStatus(bool value) => PlcStatus = value;
    public void SetMesStatus(bool value) => MesStatus = value;
    public void SetLightStatus(bool value) => LightStatus = value;
    public void SetDetectionStatus(bool value) => DetectionStatus = value;
    public void SetInspectionRunning(bool value) => IsInspectionRunning = value;
    public void SetParamStatus(bool value) => ParamStatus = value;
}
