using PCBDetection.Models;

namespace PCBDetection.Services;

public sealed class CameraManager : ICameraManager
{
    private readonly IReadOnlyList<ICameraService> cameras;
    private readonly Dictionary<string, ICameraService> camerasByName;
    private ILogService? logService;
    public CameraManager(IEnumerable<ICameraService> cameraServices, ILogService? logService = null)
    {
        cameras = cameraServices.ToArray();
        if (cameras.Count == 0)
        {
            throw new InvalidOperationException("注册的相机数量为0!");
        }

        camerasByName = new Dictionary<string, ICameraService>(StringComparer.OrdinalIgnoreCase);
        foreach (ICameraService camera in cameras)
        {
            if (string.IsNullOrWhiteSpace(camera.CameraName))
            {
                throw new InvalidOperationException("相机名称不能为空!");
            }

            if (camerasByName.ContainsKey(camera.CameraName))
            {
                throw new InvalidOperationException($"相机名称重复: {camera.CameraName}");
            }
            camerasByName.Add(camera.CameraName, camera);
        }

        this.logService = logService;
        this.logService?.Info(LogCategory.Camera, $"添加相机服务完成,相机数量为[{cameras.Count}]");
    }
    public IReadOnlyList<ICameraService> Cameras => cameras;
    /// <summary>
    /// 根据相机名获取对应相机
    /// </summary>
    /// <param name="cameraName"></param>
    /// <returns></returns>
    /// <exception cref="KeyNotFoundException"></exception>
    public ICameraService GetCamera(string cameraName)
    {
        if (TryGetCamera(cameraName, out ICameraService? camera))
        {
            return camera!;
        }

        throw new KeyNotFoundException($"未找到相机: {cameraName}");
    }
    public bool TryGetCamera(string cameraName, out ICameraService? camera)
    {
        if (string.IsNullOrWhiteSpace(cameraName))
        {
            camera = null;
            return false;
        }

        return camerasByName.TryGetValue(cameraName.Trim(), out camera);
    }
}
