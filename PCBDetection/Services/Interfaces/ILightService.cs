using PCBDetection.Models;

namespace PCBDetection.Services.Interfaces;

public interface ILightService
{
    bool Status { get; }

    Task InitializeAsync(CancellationToken cancellationToken);

    Task TurnOnAsync(CancellationToken cancellationToken);

    Task TurnOffAsync(CancellationToken cancellationToken);

    Task ReleaseAsync();
}
