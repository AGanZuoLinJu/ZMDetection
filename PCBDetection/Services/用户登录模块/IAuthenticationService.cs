using PCBDetection.Models;

namespace PCBDetection.Services;

public interface IAuthenticationService
{
    Task InitializeAsync(CancellationToken cancellationToken);

    Task<AuthenticationResult> AuthenticateAsync(
        string username,
        string password,
        CancellationToken cancellationToken);
}
