namespace PCBDetection.Models;

public sealed class DeviceStatus
{
    public DeviceStatus(string name, string state, string message = "")
    {
        Name = name;
        State = state;
        Message = message;
    }

    public string Name { get; }

    public string State { get; }

    public string Message { get; }

    public string DisplayText => string.IsNullOrWhiteSpace(Message) ? State : $"{State} - {Message}";
}
