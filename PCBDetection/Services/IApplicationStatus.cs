using System.ComponentModel;

namespace PCBDetection.Services;

public interface IApplicationStatus : INotifyPropertyChanged
{
    bool CameraStatus { get; }

    bool PlcStatus { get; }

    bool MesStatus { get; }

    bool LightStatus { get; }

    bool DetectionStatus { get; }

    bool IsInspectionRunning { get; }

    string CurrentRecipe { get; }

    void SetCameraStatus(bool value);

    void SetPlcStatus(bool value);

    void SetMesStatus(bool value);

    void SetLightStatus(bool value);

    void SetDetectionStatus(bool value);

    void SetInspectionRunning(bool value);

    void SetCurrentRecipe(string value);
}
