using PCBDetection.Models;

namespace PCBDetection.Services;

public interface ILightService
{
    bool Status { get; }

    Task InitializeAsync(CancellationToken cancellationToken);

    Task TurnOnAsync(CancellationToken cancellationToken);

    Task TurnOffAsync(CancellationToken cancellationToken);

    Task ReleaseAsync();
}
