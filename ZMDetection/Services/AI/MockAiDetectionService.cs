using ZMDetection.Models;

namespace ZMDetection.Services;

public sealed class MockAiDetectionService : IAIDetectionService
{
    private readonly Random random = new();
    private int sequence;
    private readonly string[] DefectTypeNames =
    {
        "元器件多件","元器件缺件","元器件偏移","板面破损","板面露铜"
    };
    public bool Status { get; private set; }

    public Task InitializeAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Status = true;
        return Task.CompletedTask;
    }

    public Task<InspectionResult> DetectAsync(object inputImg, string id, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!Status)
        {
            throw new InvalidOperationException("AI 服务尚未初始化。");
        }

        sequence++;
        int defectCount = sequence % DefectTypeNames.Length + 1;
        string[] defectNames = { "焊点连锡", "元件偏移", "缺件", "焊点虚焊" };
        DefectDetail[] defects = Enumerable.Range(0, defectCount)
            .Select(index => new DefectDetail(
                DefectTypeNames[index % DefectTypeNames.Length],
                $"AI-{index + 1:00}",
                random.Next(40, 2400),
                random.Next(40, 2000),
                random.Next(24, 190),
                random.Next(24, 140)))
            .ToArray();

        return Task.FromResult(new InspectionResult(
            id,
            defectCount == 0,
            defectCount,
            random.Next(650, 1100),
            inputImg,
            "AI检测完成",
            defects));
    }

    public Task ReleaseAsync()
    {
        Status = false;
        return Task.CompletedTask;
    }
}
