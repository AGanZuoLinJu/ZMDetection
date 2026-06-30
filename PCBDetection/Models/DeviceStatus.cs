namespace PCBDetection.Models;

public sealed class DeviceStatus
{
    public DeviceStatus(string name, bool state)
    {
        Name = name;
        State = state;
    }

    public string Name { get; }
    public bool State { get; }
    public string DisplayText => State ? "Online" : "Offline";
}
