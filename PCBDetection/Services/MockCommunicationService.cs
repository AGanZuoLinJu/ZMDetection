using PCBDetection.Models;
using PCBDetection.Services.Interfaces;

namespace PCBDetection.Services;

public sealed class MockPlcService : MockCommunicationService, IPlcService
{
    public MockPlcService() : base("PLC")
    {
    }
}

public sealed class MockMesService : MockCommunicationService, IMesService
{
    public MockMesService() : base("MES")
    {
    }
}

public abstract class MockCommunicationService : ICommunicationService
{
    protected MockCommunicationService(string name)
    {
        Name = name;
    }

    public event EventHandler<string>? MessageReceived;

    public string Name { get; }

    public string ConnectionStatus { get; private set; } = "Offline";

    public Task<DeviceStatus> ConnectAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ConnectionStatus = "Online";
        MessageReceived?.Invoke(this, $"{Name} mock connected");
        return Task.FromResult(new DeviceStatus(Name, ConnectionStatus));
    }

    public Task<DeviceStatus> DisconnectAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ConnectionStatus = "Offline";
        return Task.FromResult(new DeviceStatus(Name, ConnectionStatus));
    }

    public Task SendAsync(string message, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        MessageReceived?.Invoke(this, $"{Name} <= {message}");
        return Task.CompletedTask;
    }
}
