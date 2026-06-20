using PCBDetection.Models;

namespace PCBDetection.Services.Interfaces;

public interface ILightService
{
    string Status { get; }

    Task<DeviceStatus> InitializeAsync(CancellationToken cancellationToken);

    Task<DeviceStatus> TurnOnAsync(CancellationToken cancellationToken);

    Task<DeviceStatus> TurnOffAsync(CancellationToken cancellationToken);

    Task<DeviceStatus> ReleaseAsync();
}
