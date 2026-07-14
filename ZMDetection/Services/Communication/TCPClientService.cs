using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ZMDetection.Services
{
    public sealed class TCPClientService : ITCPClientService
    {
        private readonly ILogService _logService;
        private TcpClient? _tcpClient;
        private NetworkStream? _stream;
        private CancellationTokenSource? _cts;
        private bool _isConnected;
        private string _localEndPoint = "未知";
        private string _remoteEndPoint = "未知";

        public TCPClientService(ILogService logService)
        {
            _logService = logService;
        }

        public bool IsConnected
        {
            get => _isConnected;
            private set
            {
                if (_isConnected != value)
                {
                    _isConnected = value;
                    ConnectionStatusChanged?.Invoke(this, _isConnected);
                }
            }
        }
        //事件通知
        public event EventHandler<byte[]>? DataReceived;
        public event EventHandler<bool>? ConnectionStatusChanged;
        public event EventHandler<Exception>? ErrorOccurred;
        public async Task<bool> ConnectAsync(string ip, int port)
        {
            if (IsConnected) return true;

            _logService.Info(Models.LogCategory.Communication,$"TCP客户端正在连接服务端 {ip}:{port}");

            try
            {
                _tcpClient = new TcpClient();
                _cts = new CancellationTokenSource();

                await _tcpClient.ConnectAsync(ip, port);

                _stream = _tcpClient.GetStream();
                _localEndPoint = _tcpClient.Client.LocalEndPoint?.ToString() ?? "未知";
                _remoteEndPoint = _tcpClient.Client.RemoteEndPoint?.ToString() ?? $"{ip}:{port}";
                IsConnected = true;

                _logService.Info(Models.LogCategory.Communication,$"TCP客户端连接成功，本地端点 {_localEndPoint}，远端端点 {_remoteEndPoint}");

                _ = ReceiveLoopAsync(_cts.Token);

                return true;
            }
            catch (Exception ex)
            {
                _logService.Error(Models.LogCategory.Communication,$"TCP客户端连接 {ip}:{port} 失败：{ex.Message}");
                ErrorOccurred?.Invoke(this, ex);
                Disconnect();
                return false;
            }
        }
        private async Task ReceiveLoopAsync(CancellationToken token)
        {
            byte[] buffer = new byte[4096];

            try
            {
                while (!token.IsCancellationRequested && _tcpClient!.Connected)
                {
                    int bytesRead = await _stream!.ReadAsync(buffer, 0, buffer.Length, token);

                    if (bytesRead == 0)
                    {
                        //返回0代表远程服务端断开了连接
                        break;
                    }
                    byte[] actualData = new byte[bytesRead];
                    Array.Copy(buffer, actualData, bytesRead);

                    _logService.Info(Models.LogCategory.Communication,$"接收<=[{_remoteEndPoint}]:{FormatData(actualData)}");
                    DataReceived?.Invoke(this, actualData);
                }
            }
            catch (IOException ex)
            {
                if (!token.IsCancellationRequested)
                {
                    _logService.Warning(Models.LogCategory.Communication,$"TCP客户端与 {_remoteEndPoint} 的连接异常中断：{ex.Message}");
                }
            }
            catch (ObjectDisposedException)
            {
                //正常关闭时释放了对象，属于正常流程
            }
            catch (Exception ex)
            {
                _logService.Error(Models.LogCategory.Communication,$"TCP客户端接收 {_remoteEndPoint} 数据失败：{ex.Message}");
                ErrorOccurred?.Invoke(this, ex);
            }
            finally
            {
                //跳出循环说明连接已失效，执行清理
                Disconnect();
            }
        }
        public async Task SendAsync(byte[] data)
        {
            if (!IsConnected || _stream == null)
            {
                throw new InvalidOperationException("未建立TCP连接,无法发送数据.");
            }
            try
            {
                await _stream.WriteAsync(data, 0, data.Length);
                await _stream.FlushAsync();
                //_logService.Info(Models.LogCategory.Communication,$"发送=>[{_remoteEndPoint}]:{FormatData(data)}");
            }
            catch (Exception ex)
            {
                _logService.Error(Models.LogCategory.Communication,$"TCP客户端向 {_remoteEndPoint} 发送数据失败：{ex.Message}");
                ErrorOccurred?.Invoke(this, ex);
                Disconnect();
                throw;
            }
        }
        public async Task SendAsync(string sendMsg)
        {
            if (!IsConnected || _stream == null)
            {
                throw new InvalidOperationException("未建立TCP连接,无法发送数据.");
            }
            try
            {
                byte[] data = Encoding.UTF8.GetBytes(sendMsg);
                await SendAsync(data);
                _logService.Info(Models.LogCategory.Communication, $"发送=>[{_remoteEndPoint}]:{sendMsg}");
            }
            catch (Exception ex)
            {
                _logService.Error(Models.LogCategory.Communication, $"TCP客户端向 {_remoteEndPoint} 发送数据失败：{ex.Message}");
                ErrorOccurred?.Invoke(this, ex);
                Disconnect();
                throw;
            }
        }
        public void Disconnect()
        {
            if (!IsConnected && _tcpClient == null) return;

            try
            {
                var wasConnected = IsConnected;
                var remoteEndPoint = _remoteEndPoint;
                IsConnected = false;

                _cts?.Cancel();
                _cts?.Dispose();
                _cts = null;

                _stream?.Close();
                _stream?.Dispose();
                _stream = null;

                _tcpClient?.Close();
                _tcpClient = null;

                if (wasConnected)
                {
                    _logService.Info(Models.LogCategory.Communication,$"TCP客户端已与 {remoteEndPoint} 断开连接");
                }
            }
            catch (Exception ex)
            {
                _logService.Error(Models.LogCategory.Communication,$"TCP客户端断开连接时发生错误：{ex.Message}");
                ErrorOccurred?.Invoke(this, ex);
            }
        }
        private static string FormatData(byte[] data)
        {
            const int maximumLogBytes = 256;
            byte[] displayedData = data.Take(maximumLogBytes).ToArray();
            string text = Encoding.UTF8.GetString(displayedData);
            text = new string(text.Select(character => char.IsControl(character) ? '.' : character).ToArray());
            string hex = BitConverter.ToString(displayedData).Replace("-", " ");
            string suffix = data.Length > maximumLogBytes ? "（内容已截断）" : string.Empty;

            //return $"文本=\"{text}\"，HEX={hex}{suffix}";
            return text;
        }
    }
}
