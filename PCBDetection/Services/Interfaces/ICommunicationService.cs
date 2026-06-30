using PCBDetection.Models;

namespace PCBDetection.Services.Interfaces;

public interface ICommunicationService
{
    event EventHandler<string>? MessageReceived;

    string Name { get; }

    bool ConnectionStatus { get; }

    Task ConnectAsync(CancellationToken cancellationToken);

    Task DisconnectAsync(CancellationToken cancellationToken);

    Task SendAsync(string message, CancellationToken cancellationToken);
}

public interface IPlcService : ICommunicationService
{
}

public interface IMesService : ICommunicationService
{
}
