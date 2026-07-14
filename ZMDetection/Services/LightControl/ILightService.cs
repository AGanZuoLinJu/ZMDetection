using ZMDetection.Models;

namespace ZMDetection.Services;

public interface ILightService
{
    bool Status { get; }

    Task InitializeAsync(CancellationToken cancellationToken);
    Task TurnOnAsync(int channel,CancellationToken cancellationToken);
    Task TurnOffAsync(int channel,CancellationToken cancellationToken);
    Task ChangeLightValue(int channel, int lightValue,CancellationToken cancellationToken);
    Task ReleaseAsync();
}
