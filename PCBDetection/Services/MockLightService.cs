using PCBDetection.Models;
using PCBDetection.Services.Interfaces;

namespace PCBDetection.Services;

public sealed class MockLightService : ILightService
{
    public string Status { get; private set; } = "Offline";

    public Task<DeviceStatus> InitializeAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Status = "Ready";
        return Task.FromResult(new DeviceStatus("Light", Status, "Mock light controller initialized"));
    }

    public Task<DeviceStatus> TurnOnAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Status = "On";
        return Task.FromResult(new DeviceStatus("Light", Status));
    }

    public Task<DeviceStatus> TurnOffAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Status = "Off";
        return Task.FromResult(new DeviceStatus("Light", Status));
    }

    public Task<DeviceStatus> ReleaseAsync()
    {
        Status = "Released";
        return Task.FromResult(new DeviceStatus("Light", Status));
    }
}
