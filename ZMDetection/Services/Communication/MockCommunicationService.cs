using ZMDetection.Models;

namespace ZMDetection.Services;

public abstract class MockCommunicationService : ICommunicationService
{
    protected MockCommunicationService(string name)
    {
        Name = name;
    }
    public event EventHandler<string>? MessageReceived;
    public string Name { get; }
    public bool ConnectionStatus { get; private set; } = false;
    public Task ConnectAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ConnectionStatus = true;
        return Task.Run(async () =>
        {
            await Task.Delay(1000);
        },cancellationToken);
    }
    public Task DisconnectAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ConnectionStatus = false;
        return Task.Run(async () =>
        {
            await Task.Delay(1000);
        }, cancellationToken);
    }
    public Task SendAsync(string message, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        MessageReceived?.Invoke(this, $"{Name} <= {message}");
        return Task.CompletedTask;
    }
}
