using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ZMDetection.Services
{
    public interface ITCPServerService
    {
        bool IsListening { get; }
        event EventHandler<string> ClientConnected; 
        event EventHandler<string> ClientDisconnected;
        event EventHandler<(string ClientId, byte[] Data)> DataReceived;
        event EventHandler<Exception> ErrorOccurred;
        Task StartListeningAsync(int port);
        void StopListening();
        Task SendToClientAsync(string clientId, byte[] data);
        Task BroadcastAsync(byte[] data); 
    }
    public interface ITCPClientService
    {
        bool IsConnected { get; }
        event EventHandler<byte[]> DataReceived;
        event EventHandler<bool> ConnectionStatusChanged;
        event EventHandler<Exception> ErrorOccurred;
        Task<bool> ConnectAsync(string ip, int port);
        void Disconnect();
        Task SendAsync(byte[] data);
    }
}
