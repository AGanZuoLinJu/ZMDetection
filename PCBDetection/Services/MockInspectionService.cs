using PCBDetection.Models;
using PCBDetection.Services.Interfaces;

namespace PCBDetection.Services;

public sealed class MockInspectionService : IInspectionService
{
    private readonly Random random = new();
    private int sequence;

    public event EventHandler<InspectionResult>? InspectionCompleted;

    public InspectionResult RunInspection()
    {
        sequence++;

        // 依次产生 1、2、3、4、0 个缺陷，便于稳定验证表格刷新和 OK 场景。
        var defectCount = sequence % 5;
        var cycleTime = random.Next(820, 1380);
        var boardId = $"PCB-{DateTime.Now:yyyyMMdd}-{sequence:0000}";
        var defectNames = new[] { "焊点连锡", "元件偏移", "缺件", "焊点虚焊" };
        var defects = Enumerable.Range(0, defectCount)
            .Select(index => new DefectDetail(
                defectNames[index % defectNames.Length],
                $"D{index + 1:00}",
                random.Next(40, 2400),
                random.Next(40, 2000),
                random.Next(24, 190),
                random.Next(24, 140)))
            .ToArray();

        var result = new InspectionResult(
            boardId,
            defectCount == 0,
            defectCount,
            cycleTime,
            "PCB_TOP_AOI_V1",
            boardId,
            message: "Mock inspection finished",
            defects: defects);
        InspectionCompleted?.Invoke(this, result);
        return result;
    }

    public Task<InspectionResult> RunInspectionAsync(InspectionRequest request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var result = RunInspection();
        return Task.FromResult(new InspectionResult(
            result.BoardId,
            result.IsOk,
            result.DefectCount,
            result.CycleTimeMilliseconds,
            request.RecipeName,
            string.IsNullOrWhiteSpace(request.PanelId) ? result.BoardId : request.PanelId,
            request.Frame?.ImagePath ?? result.ImagePath,
            result.Message,
            result.Defects));
    }

    public Task StartAutoInspectionAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.CompletedTask;
    }

    public Task StopAutoInspectionAsync()
    {
        return Task.CompletedTask;
    }

    public void Reset()
    {
        sequence = 0;
    }
}
