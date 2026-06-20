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

        var defectCount = random.Next(0, 5);
        var cycleTime = random.Next(820, 1380);
        var boardId = $"PCB-{DateTime.Now:yyyyMMdd}-{sequence:0000}";

        var result = new InspectionResult(boardId, defectCount == 0, defectCount, cycleTime, "PCB_TOP_AOI_V1", boardId, message: "Mock inspection finished");
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
            result.Message));
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
