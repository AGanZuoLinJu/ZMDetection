using PCBDetection.Models;

namespace PCBDetection.Services;

public interface ICommunicationService
{
    event EventHandler<string>? MessageReceived;
    string Name { get; }
    bool ConnectionStatus { get; }
    Task ConnectAsync(CancellationToken cancellationToken);
    Task DisconnectAsync(CancellationToken cancellationToken);
    Task SendAsync(string message, CancellationToken cancellationToken);
}

public interface IMCCommunicationService : ICommunicationService
{

}


