using PCBDetection.Models;

namespace PCBDetection.Services.Interfaces;

public interface IStartupService
{
    Task InitializeAsync(IProgress<StartupProgress> progress, CancellationToken cancellationToken);

    Task ShutdownAsync(IProgress<StartupProgress> progress, CancellationToken cancellationToken);
}
