using System.ComponentModel;

namespace PCBDetection.Services.Interfaces;

public interface IApplicationStatus : INotifyPropertyChanged
{
    bool CameraStatus { get; }

    bool PlcStatus { get; }

    bool MesStatus { get; }

    bool LightStatus { get; }

    bool AiStatus { get; }

    string CurrentRecipe { get; }

    void SetCameraStatus(bool value);

    void SetPlcStatus(bool value);

    void SetMesStatus(bool value);

    void SetLightStatus(bool value);

    void SetAiStatus(bool value);

    void SetCurrentRecipe(string value);
}
