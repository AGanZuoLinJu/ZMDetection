using PCBDetection.Models;

namespace PCBDetection.Services.Interfaces;

public interface ICommunicationService
{
    event EventHandler<string>? MessageReceived;

    string Name { get; }

    string ConnectionStatus { get; }

    Task<DeviceStatus> ConnectAsync(CancellationToken cancellationToken);

    Task<DeviceStatus> DisconnectAsync(CancellationToken cancellationToken);

    Task SendAsync(string message, CancellationToken cancellationToken);
}

public interface IPlcService : ICommunicationService
{
}

public interface IMesService : ICommunicationService
{
}
