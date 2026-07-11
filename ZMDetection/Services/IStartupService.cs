using ZMDetection.Models;

namespace ZMDetection.Services;

public interface IStartupService
{
    Task InitializeAsync(IProgress<StartupProgress> progress, CancellationToken cancellationToken);

    Task ShutdownAsync(IProgress<StartupProgress> progress, CancellationToken cancellationToken);
}
