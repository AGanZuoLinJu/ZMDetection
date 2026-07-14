using System.IO.Ports;
using System.Text;
using System.Threading;
using ZMDetection.Models;

namespace ZMDetection.Services;

public sealed class FGLightService : ILightService
{
    #region <<<串口相关属性
    public string SerialPortName { get; private set; } = "0";
    public int BaudRate { get; } = 9600;
    public int DataBit { get; } = 0;
    #endregion

    private readonly ILogService logService;
    private SerialPort? FGLightControlPort;
    public FGLightService(ILogService logService)
    {
        this.logService = logService;
    }

    public bool Status { get; private set; } = false;
    public Task InitializeAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.Run(() =>
        {
            string[] serialPortNames = SerialPort.GetPortNames();
            if (serialPortNames.Length == 0)
            {
                logService.Error(LogCategory.Running, "串口数量为0!");
                return;
            }

            try
            {
                SerialPortName = serialPortNames[0];                //默认COM0 需要根据实际情况确定
                FGLightControlPort = new SerialPort
                {
                    PortName = SerialPortName,
                    BaudRate = BaudRate,
                    DataBits = DataBit,
                    StopBits = StopBits.One,
                    Parity = Parity.None,
                };
                FGLightControlPort.Open();

                Status = true;
            }
            catch (Exception ex)
            {
                logService.Error(LogCategory.Running, "串口打开失败!", ex);
            }

        }, cancellationToken);
    }
    /// <summary>
    /// 打开光源
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task TurnOnAsync(int channel,CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.Run(() =>
        {
            try
            {
                ChangeLightValue(channel, HardwareParam.HardwareParamConfig.LightSource, cancellationToken);
            }
            catch (Exception ex)
            {
                logService.Error(LogCategory.Running, "打开光源失败!", ex);
            }
        }, cancellationToken);
    }
    /// <summary>
    /// 关闭光源
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task TurnOffAsync(int channel,CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.Run(() =>
        {
            try
            {
                if (Status)
                {
                    ChangeLightValue(channel, 0, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                logService.Error(LogCategory.Running, "关闭光源失败!", ex);
            }
        }, cancellationToken);
    }
    /// <summary>
    /// 修改光源亮度(FG-DPC400W控制器串口协议)
    /// </summary>
    /// <param name="channel">通道</param>
    /// <param name="lightValue">光源亮度</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task ChangeLightValue(int channel, int lightValue,CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.Run(() =>
        {
            string sendSerialPortMsg = "";
            HardwareParam.HardwareParamConfig.LightSource = lightValue;

            if (Status)
            {
                try
                {
                    if (Status)
                    {
                        string head = "$3";
                        string strChannel = channel.ToString();
                        string value = lightValue.ToString("X3");       
                        string mergeStr = head + strChannel + value;
                        byte[] data = Encoding.UTF8.GetBytes(mergeStr);

                        int xorSum = data[0];
                        for(int i =0; i < data.Length - 1;i++)
                        {
                            xorSum ^= data[i + 1];
                        }
                        sendSerialPortMsg = mergeStr + xorSum.ToString("X2");
                        FGLightControlPort?.Write(sendSerialPortMsg);
                    }
                }
                catch (Exception ex)
                {
                    logService.Error(LogCategory.Running, "修改光源亮度失败!", ex);
                }
            }
        }, cancellationToken);
    }
    public Task ReleaseAsync()
    {
        return Task.Run(() =>
        {
            if (Status)
            {
                try
                {
                    FGLightControlPort?.Close();
                    FGLightControlPort?.Dispose();
                }
                catch(Exception ex)
                {
                    logService.Error(LogCategory.Running, "释放光源控制器失败!", ex);
                }

                Status = false;
            }
        });
    }
}
