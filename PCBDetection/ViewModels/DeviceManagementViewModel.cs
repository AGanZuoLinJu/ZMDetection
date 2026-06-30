using Prism.Mvvm;

namespace PCBDetection.ViewModels;

public sealed class DeviceManagementViewModel : BindableBase
{
    public string Title => "设备管理";

    public string Description => "Camera, PLC, light and AI device configuration will be managed here.";
}
