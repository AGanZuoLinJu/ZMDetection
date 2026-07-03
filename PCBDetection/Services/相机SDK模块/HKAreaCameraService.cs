using System.Runtime.InteropServices;
using System.Windows.Media.Media3D;
using HalconDotNet;
using MvCamCtrl.NET;
using PCBDetection.Models;

namespace PCBDetection.Services;
/// <summary>
/// 海康面阵相机
/// </summary>
public sealed class HKAreaCameraService : ICameraService
{
    private readonly ILogService _logService;
    private readonly string cameraName;
    private string serialNumber;
    public event EventHandler<object>? ImageCaptured;
    public string CameraName => cameraName;
    public string SerialNumber => serialNumber;
    private bool _connectionStatus;
    public bool ConnectionStatus
    {
        get
        {
            if (m_MyCamera != null)
                _connectionStatus = m_MyCamera.MV_CC_IsDeviceConnected_NET();
            else
                _connectionStatus = false;
            return _connectionStatus;
        }
    }
    public object? GetImage { get; private set; } = null;

    #region <<<全局变量
    MyCamera.MV_CC_DEVICE_INFO_LIST m_pDeviceList;
    private readonly MyCamera m_MyCamera = new();
    MyCamera.MV_FRAME_OUT stFrameOut = new MyCamera.MV_FRAME_OUT();
    int nRet = MyCamera.MV_OK;
    IntPtr pImageBuf = IntPtr.Zero;
    int nImageBufSize = 0;
    HObject Hobj = new HObject();
    IntPtr pTemp = IntPtr.Zero;
    #endregion

    public HKAreaCameraService(ILogService logService)
        : this(logService, "HK-Area-CAM-01", string.Empty)
    {
    }

    public HKAreaCameraService(
        ILogService logService,
        string cameraName,
        string serialNumber)
    {
        _logService = logService;
        this.cameraName = cameraName;
        this.serialNumber = serialNumber;
    }
    public Task InitializeAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.Run(() =>
        {
            int nRet;
            _logService.Info(LogCategory.Camera, "正在枚举相机...");
            // ch:创建设备列表 || en: Create device list
            nRet = MyCamera.MV_CC_EnumDevices_NET(MyCamera.MV_GIGE_DEVICE | MyCamera.MV_USB_DEVICE, ref m_pDeviceList);
            _logService.Info(LogCategory.Camera,$"枚举相机成功,相机数量为[{m_pDeviceList.nDeviceNum}]");
            if (MyCamera.MV_OK != nRet)
            {
                _logService.Error(LogCategory.Camera, "Enum Devices Fail");
                throw new InvalidOperationException($"{CameraName} 枚举设备失败，错误码: {nRet}");
            }
            if (m_pDeviceList.nDeviceNum == 0)
            {
                _logService.Error(LogCategory.Camera, "枚举相机数量为0!");
                throw new InvalidOperationException("未枚举到 HK 相机。");
            }

            MyCamera.MV_CC_DEVICE_INFO device = FindConfiguredDevice();

            //打开设备
            nRet = -1;
            nRet = m_MyCamera.MV_CC_CreateDevice_NET(ref device);
            if (MyCamera.MV_OK != nRet)
            {
                throw new InvalidOperationException($"{CameraName} 创建设备失败，错误码: {nRet}");
            }

            // ch:打开设备 | en:Open device
            nRet = m_MyCamera.MV_CC_OpenDevice_NET();
            if (MyCamera.MV_OK != nRet)
            {
                _logService.Error(LogCategory.Camera,"打开相机失败!" + nRet.ToString());
                throw new InvalidOperationException($"{CameraName} 打开设备失败，错误码: {nRet}");
            }

            // ch:设置触发模式为off || en:set trigger mode as off
            m_MyCamera.MV_CC_SetEnumValue_NET("AcquisitionMode", 2);
            m_MyCamera.MV_CC_SetEnumValue_NET("TriggerMode", 0);
        }, cancellationToken);
    }
    public Task DestroyCamera(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.Run(async () =>
        {
            await Task.Delay(1000);
        }, cancellationToken);
    }
    public Task StartGrabbingAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.Run(async () =>
        {
            if (ConnectionStatus)
            {
                await Task.Delay(1000);
            }
        }, cancellationToken);
    }
    public Task StopGrabbingAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.Run(async () =>
        {
            if (ConnectionStatus)
            {
                await Task.Delay(1000);
            }
        }, cancellationToken);
    }
    public Task<object> GetOneFrameImageAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.Run(() =>
        {
            return new object();
        }, cancellationToken);
    }

    #region <<<其他方法
    private MyCamera.MV_CC_DEVICE_INFO FindConfiguredDevice()
    {
        for (int index = 0; index < (int)m_pDeviceList.nDeviceNum; index++)
        {
            var device = (MyCamera.MV_CC_DEVICE_INFO)Marshal.PtrToStructure(
                m_pDeviceList.pDeviceInfo[index],
                typeof(MyCamera.MV_CC_DEVICE_INFO));
            string detectedSerialNumber = GetSerialNumber(device);

            if (string.IsNullOrWhiteSpace(serialNumber) ||
                string.Equals(
                    serialNumber,
                    detectedSerialNumber,
                    StringComparison.OrdinalIgnoreCase))
            {
                serialNumber = detectedSerialNumber;
                _logService.Info(LogCategory.Camera,$"{CameraName} 匹配到设备，序列号: {serialNumber}");
                return device;
            }
        }

        throw new InvalidOperationException($"{CameraName} 未找到指定序列号的相机: {serialNumber}");
    }
    private static string GetSerialNumber(MyCamera.MV_CC_DEVICE_INFO device)
    {
        if (device.nTLayerType == MyCamera.MV_GIGE_DEVICE)
        {
            IntPtr buffer = Marshal.UnsafeAddrOfPinnedArrayElement(device.SpecialInfo.stGigEInfo,0);
            var info = (MyCamera.MV_GIGE_DEVICE_INFO)Marshal.PtrToStructure(buffer,typeof(MyCamera.MV_GIGE_DEVICE_INFO));
            return DecodeText(info.chSerialNumber);
        }

        if (device.nTLayerType == MyCamera.MV_USB_DEVICE)
        {
            IntPtr buffer = Marshal.UnsafeAddrOfPinnedArrayElement(
                device.SpecialInfo.stUsb3VInfo,
                0);
            var info = (MyCamera.MV_USB3_DEVICE_INFO)Marshal.PtrToStructure(
                buffer,
                typeof(MyCamera.MV_USB3_DEVICE_INFO));
            return DecodeText(info.chSerialNumber);
        }

        return string.Empty;
    }
    private static string DecodeText(string value) => value.TrimEnd('\0', ' ');
    #endregion
}

