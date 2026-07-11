using ZMDetection.Models;

namespace ZMDetection.Services;

public sealed class InspectionService : IInspectionService
{
    private readonly IAIDetectionService aiDetectionService;

    public InspectionService(IAIDetectionService aiDetectionService)
    {
        this.aiDetectionService = aiDetectionService;
    }
    public bool Status { get; private set; } = false;
    /// <summary>
    /// 初始化检测
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Status = false;

        if (aiDetectionService.Status)
        {
            await aiDetectionService.ReleaseAsync();
        }

        await aiDetectionService.InitializeAsync(cancellationToken);
        Status = true;
    }
    /// <summary>
    /// 执行检测
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public async Task<InspectionResult> RunInspectionAsync(object inputImg,CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!Status)
        {
            throw new InvalidOperationException("检测服务尚未初始化!");
        }

        return await aiDetectionService.DetectAsync(inputImg,cancellationToken);
    }
    /// <summary>
    /// 释放检测
    /// </summary>
    /// <returns></returns>
    public async Task ReleaseAsync()
    {
        if (aiDetectionService.Status)
        {
            await aiDetectionService.ReleaseAsync();
        }

        Status = false;
    }
}
