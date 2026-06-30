using System.Threading;
using PCBDetection.Models;
using PCBDetection.Services.Interfaces;

namespace PCBDetection.Services;

public sealed class MockLightService : ILightService
{
    public bool Status { get; private set; } = false;

    public Task InitializeAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.Run(() =>
        {
            Status = true;
        },cancellationToken);
    }

    public Task TurnOnAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.Run(async () =>
        {
            if (Status)
            {
                await Task.Delay(100);
            }
        }, cancellationToken);
    }

    public Task TurnOffAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.Run(async () =>
        {
            if (Status)
            {
                await Task.Delay(100);
            }
        }, cancellationToken);
    }

    public Task ReleaseAsync()
    {
        return Task.Run(() =>
        {
            if (Status)
            {
                Status = false;
            }
        });
    }
}
