using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Interop;

namespace ZMDetection.Services
{
    public sealed class TCPServerService : ITCPServerService
    {
        private readonly ILogService _logService;
        private TcpListener? _listener;
        private CancellationTokenSource? _cancellationTokenSource;
        private int _listeningPort;

        private readonly ConcurrentDictionary<string, TcpClient> _clients = new ConcurrentDictionary<string, TcpClient>();

        public TCPServerService(ILogService logService)
        {
            _logService = logService;
        }

        public bool IsListening { get; private set; }
        // 事件定义
        public event EventHandler<string>? ClientConnected;
        public event EventHandler<string>? ClientDisconnected;
        public event EventHandler<(string ClientId, byte[] Data)>? DataReceived;
        public event EventHandler<Exception>? ErrorOccurred;

        public async Task StartListeningAsync(int port)
        {
            if (IsListening) return;

            _logService.Info(Models.LogCategory.Communication,$"TCP服务端正在启动，监听端口 {port}");

            try
            {
                _listener = new TcpListener(IPAddress.Any, port);
                _listener.Start();
                _listeningPort = port;
                IsListening = true;
                _cancellationTokenSource = new CancellationTokenSource();

                _logService.Info(Models.LogCategory.Communication,$"TCP服务端已开始监听 0.0.0.0:{port}");

                _ = AcceptClientsAsync(_cancellationTokenSource.Token);
                await Task.Delay(10);
            }
            catch (Exception ex)
            {
                _logService.Error(Models.LogCategory.Communication,$"TCP服务端监听端口 {port} 失败：{ex.Message}");
                ErrorOccurred?.Invoke(this, ex);
                throw;
            }
        }
        private async Task AcceptClientsAsync(CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    TcpClient client = await _listener!.AcceptTcpClientAsync();

                    string clientId = client.Client.RemoteEndPoint.ToString();

                    _clients.TryAdd(clientId, client);
                    _logService.Info(Models.LogCategory.Communication,$"客户端 {clientId} 已连接到本地端口 {_listeningPort}");
                    ClientConnected?.Invoke(this, clientId);

                    _ = ReceiveFromClientAsync(clientId, client, token);
                }
            }
            catch (ObjectDisposedException)
            {
                //监听被停止时会触发此异常，属于正常关闭流程
            }
            catch (SocketException) when (token.IsCancellationRequested)
            {
                //停止监听时 AcceptTcpClientAsync 会被中断，属于正常关闭流程
            }
            catch (Exception ex)
            {
                _logService.Error(Models.LogCategory.Communication,$"TCP服务端接受客户端连接失败：{ex.Message}");
                ErrorOccurred?.Invoke(this, ex);
            }
        }
        /// <summary>
        /// 从客户端中接收消息
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="client"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        private async Task ReceiveFromClientAsync(string clientId, TcpClient client, CancellationToken token)
        {
            NetworkStream stream = client.GetStream();
            byte[] buffer = new byte[4096];

            try
            {
                while (!token.IsCancellationRequested)
                {
                    int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, token);

                    if (bytesRead == 0)
                    {
                        //返回0表示客户端已主动安全断开连接
                        break;
                    }

                    byte[] actualData = new byte[bytesRead];
                    Array.Copy(buffer, actualData, bytesRead);

                    _logService.Info(Models.LogCategory.Communication,$"接收[{clientId}]:{FormatData(actualData)}");
                    DataReceived?.Invoke(this, (clientId, actualData));
                }
            }
            catch (OperationCanceledException) when (token.IsCancellationRequested)
            {
                //服务端停止时取消接收，属于正常关闭流程
            }
            catch (ObjectDisposedException) when (token.IsCancellationRequested)
            {
                //服务端停止时连接被释放，属于正常关闭流程
            }
            catch (Exception ex)
            {
                _logService.Error(Models.LogCategory.Communication,$"服务端接收客户端 {clientId} 数据失败：{ex.Message}");
                ErrorOccurred?.Invoke(this, ex);
            }
            finally
            {
                CleanupClient(clientId);
            }
        }
        private void CleanupClient(string clientId)
        {
            if (_clients.TryRemove(clientId, out TcpClient client))
            {
                client.Close();
                _logService.Info(Models.LogCategory.Communication,$"客户端 {clientId} 已从本地端口 {_listeningPort} 断开");
                ClientDisconnected?.Invoke(this, clientId);
            }
        }
        public void StopListening()
        {
            if (!IsListening) return;

            _logService.Info(Models.LogCategory.Communication,$"TCP服务端正在停止监听端口 {_listeningPort}");
            IsListening = false;

            _cancellationTokenSource?.Cancel();
            _listener?.Stop();

            foreach (var clientId in _clients.Keys)
            {
                CleanupClient(clientId);
            }

            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;
            _listener = null;

            _logService.Info(Models.LogCategory.Communication,$"TCP服务端已停止监听端口 {_listeningPort}");
        }
        public async Task SendToClientAsync(string clientId, byte[] data)
        {
            if (_clients.TryGetValue(clientId, out TcpClient client))
            {
                try
                {
                    NetworkStream stream = client.GetStream();
                    await stream.WriteAsync(data, 0, data.Length);
                    await stream.FlushAsync();
                    //_logService.Info(Models.LogCategory.Communication,$"发送[{clientId}]:{FormatData(data)}");
                }
                catch (Exception ex)
                {
                    _logService.Error(Models.LogCategory.Communication,$"服务端向客户端 {clientId} 发送数据失败：{ex.Message}");
                    ErrorOccurred?.Invoke(this, ex);
                    CleanupClient(clientId); 
                }
            }
        }
        public async Task SendToClientAsync(string clientId, string sendMsg)
        {
            if (_clients.TryGetValue(clientId, out TcpClient client))
            {
                try
                {
                    byte[] data = Encoding.UTF8.GetBytes(sendMsg);
                    await SendToClientAsync(clientId, data);
                    _logService.Info(Models.LogCategory.Communication, $"发送[{clientId}]:{sendMsg}");
                }
                catch (Exception ex)
                {
                    _logService.Error(Models.LogCategory.Communication, $"服务端向客户端 {clientId} 发送数据失败：{ex.Message}");
                    ErrorOccurred?.Invoke(this, ex);
                    CleanupClient(clientId);
                }
            }
        }
        /// <summary>
        /// 广播发送 向所有端口发送消息
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task BroadcastAsync(byte[] data)
        {
            var tasks = new List<Task>();
            foreach (var clientId in _clients.Keys)
            {
                tasks.Add(SendToClientAsync(clientId, data));
            }

            await Task.WhenAll(tasks);
        }
        private static string FormatData(byte[] data)
        {
            const int maximumLogBytes = 256;
            byte[] displayedData = data.Take(maximumLogBytes).ToArray();
            string text = Encoding.UTF8.GetString(displayedData);
            text = new string(text.Select(character =>
                char.IsControl(character) ? '.' : character).ToArray());
            string hex = BitConverter.ToString(displayedData).Replace("-", " ");
            string suffix = data.Length > maximumLogBytes ? "（内容已截断）" : string.Empty;

            //return $"文本=\"{text}\"，HEX={hex}{suffix}";
            return text;
        }
    }
}
