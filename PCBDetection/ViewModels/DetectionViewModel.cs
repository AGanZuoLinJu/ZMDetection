using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using HalconDotNet;
using PCBDetection.Models;
using PCBDetection.Services;
using Prism.Commands;
using Prism.Mvvm;

namespace PCBDetection.ViewModels;

public sealed class DetectionViewModel : BindableBase
{
    #region <<<Services
    private readonly IInspectionWorkflowService workflowService;                //检测流程
    private readonly IProductionStatisticsService statisticsService;            //数据统计
    private readonly ILogService logService;
    private readonly IApplicationStatus applicationStatus;                      //软件状态
    #endregion

    #region <<<数据统计相关
    private int okCount;
    private int ngCount;
    private int defectCount;
    public int OkCount
    {
        get => okCount;
        private set
        {
            if (SetProperty(ref okCount, value))
            {
                RaisePropertyChanged(nameof(YieldRate));
                RaisePropertyChanged(nameof(TotalCount));
            }
        }
    }
    public int NgCount
    {
        get => ngCount;
        private set
        {
            if (SetProperty(ref ngCount, value))
            {
                RaisePropertyChanged(nameof(YieldRate));
                RaisePropertyChanged(nameof(TotalCount));
            }
        }
    }
    public int DefectCount
    {
        get => defectCount;
        private set => SetProperty(ref defectCount, value);
    }
    public double YieldRate => statisticsService.Current.YieldRate;
    public int TotalCount => OkCount + NgCount;
    #endregion

    private CancellationTokenSource? runCancellation;
    private string inspectionResult = "NA";
    private string currentBoardId = "--";
    private double cycleTime;
    private bool isRunning;

    #region <<<构造函数
    public DetectionViewModel(
        IInspectionWorkflowService workflowService,
        IProductionStatisticsService statisticsService,
        ILogService logService,
        IApplicationStatus applicationStatus)
    {
        this.workflowService = workflowService;
        this.statisticsService = statisticsService;
        this.logService = logService;
        this.applicationStatus = applicationStatus;

        CurrentRecipe = applicationStatus.CurrentRecipe;

        ToggleInspectionCommand = new DelegateCommand(async () => await ToggleInspectionAsync());
        ClearLogViewCommand = new DelegateCommand(() => LogItems.Clear());

        var visibleHistory = logService.GetHistory(LogCategory.Running)
            .Concat(logService.GetHistory(LogCategory.Communication))
            .OrderByDescending(item => item.Timestamp)
            .Take(80);

        foreach (var item in visibleHistory)
        {
            LogItems.Add(item);
        }

        logService.LogAdded += OnLogAdded;
        ApplyStatistics(statisticsService.Current);
    }
    #endregion

    public string InspectionResult
    {
        get => inspectionResult;
        private set => SetProperty(ref inspectionResult, value);
    }
    public string CurrentBoardId
    {
        get => currentBoardId;
        private set => SetProperty(ref currentBoardId, value);
    }
    public string CurrentRecipe { get; }
    public double CycleTime
    {
        get => cycleTime;
        private set
        {
            if (SetProperty(ref cycleTime, value))
            {
                RaisePropertyChanged(nameof(CycleTimeProgress));
            }
        }
    }
    public double CycleTimeProgress => Math.Max(0, Math.Min(CycleTime / 1600d * 100, 100));
    public bool IsRunning
    {
        get => isRunning;
        private set
        {
            if (SetProperty(ref isRunning, value))
            {
                RaisePropertyChanged(nameof(InspectionButtonText));
            }
        }
    }
    public string InspectionButtonText => IsRunning ? "停止检测" : "开始检测";

    #region <<<Command
    public DelegateCommand ToggleInspectionCommand { get; }
    public ObservableCollection<LogItem> LogItems { get; } = new();
    public ObservableCollection<DefectDetail> Defects { get; } = new();
    public DelegateCommand ClearLogViewCommand { get; }
    #endregion

    #region <<<开始及停止运行
    private async Task ToggleInspectionAsync()
    {
        if (!IsRunning)
        {
            await StartInspectionAsync();
            return;
        }

        var confirmation = MessageBox.Show(
            Application.Current?.MainWindow,
            "确定要停止检测吗？",
            "停止确认",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question,
            MessageBoxResult.No);

        if (confirmation == MessageBoxResult.Yes)
        {
            await StopInspectionAsync();
        }
    }
    int i = 0;
    private HObject? ho_dispMainImage;
    /// <summary>
    /// 当前显示的图片
    /// </summary>
    public HObject? DisplayMainImage
    {
        get => ho_dispMainImage;
        private set
        {
            HObject? previousImage = ho_dispMainImage;
            if (SetProperty(ref ho_dispMainImage, value))
            {
                previousImage?.Dispose();
            }
        }
    }
    /// <summary>
    /// 开始运行检测流程
    /// </summary>
    /// <returns></returns>
    private async Task StartInspectionAsync()
    {
        runCancellation?.Cancel();
        var cancellation = new CancellationTokenSource();
        runCancellation = cancellation;
        IsRunning = true;
        applicationStatus.SetInspectionRunning(true);
        logService.Info(LogCategory.Running, "检测开始运行...");

        try
        {
            while (applicationStatus.IsInspectionRunning && !cancellation.Token.IsCancellationRequested)
            {
                var result = await workflowService.StartRunAsync(cancellation.Token);


                ApplyResult(result);

                await Task.Delay(1000, cancellation.Token);
            }
        }
        catch (OperationCanceledException)
        {
            //任务被取消 不做记录
        }
        catch (Exception ex)
        {
            logService.Error(LogCategory.Running, $"检测错误 : {ex.Message}");
        }
        finally
        {
            applicationStatus.SetInspectionRunning(false);
            IsRunning = false;
            if (ReferenceEquals(runCancellation, cancellation))
            {
                runCancellation = null;
            }

            cancellation.Dispose();
        }
    }
    /// <summary>
    /// 停止检测流程
    /// </summary>
    /// <returns></returns>
    private async Task StopInspectionAsync()
    {
        applicationStatus.SetInspectionRunning(false);
        runCancellation?.Cancel();
        await workflowService.StopAsync();
        IsRunning = false;
        logService.Info(LogCategory.Running, "检测停止运行...");
    }
    #endregion

    #region <<<其他方法
    private void ApplyResult(InspectionResult result)
    {
        CurrentBoardId = result.BoardId;
        InspectionResult = result.IsOk ? "OK" : "NG";
        DefectCount = result.DefectCount;
        Defects.Clear();
        foreach (var defect in result.Defects)
        {
            Defects.Add(defect);
        }

        ApplyStatistics(statisticsService.Current);

        if (!result.IsOk)
        {
            logService.Info(
                LogCategory.Defect,
                $"{result.BoardId} has {result.DefectCount} defects.");
        }
    }
    private void ApplyStatistics(ProductionStatisticsSnapshot snapshot)
    {
        OkCount = snapshot.OkCount;
        NgCount = snapshot.NgCount;
        CycleTime = snapshot.LastCycleTimeMilliseconds;
        RaisePropertyChanged(nameof(YieldRate));
    }
    private void OnLogAdded(object? sender, LogItem logItem)
    {
        if (logItem.Category != LogCategory.Running &&
            logItem.Category != LogCategory.Communication)
        {
            return;
        }

        void AddLog()
        {
            LogItems.Insert(0, logItem);
            while (LogItems.Count > 80)
            {
                LogItems.RemoveAt(LogItems.Count - 1);
            }
        }
        Application.Current?.Dispatcher.BeginInvoke((Action)AddLog);
    }
    private void ImgIsNotStretchDisp(HObject ho_image, HWindow dispWindow)
    {
        if (ho_image != null)
        {
            HTuple hv_Width, hv_Height;
            HTuple win_Width, win_Height, win_Col, win_Row, cwin_Width, cwin_Height;
            HOperatorSet.ClearWindow(dispWindow);
            HOperatorSet.GetImageSize(ho_image, out hv_Width, out hv_Height);
            HOperatorSet.GetWindowExtents(dispWindow, out win_Row, out win_Col, out win_Width, out win_Height);  //获取窗体大小规格
            cwin_Height = 1.0 * win_Height / win_Width * hv_Width;//宽不变计算高    
            if (cwin_Height > hv_Height)//宽不变高能容纳
            {
                cwin_Height = 1.0 * (cwin_Height - hv_Height) / 2;
                HOperatorSet.SetPart(dispWindow, -cwin_Height, 0, cwin_Height + hv_Height, hv_Width);//设置窗体的规格
            }
            else//高不变宽能容纳
            {
                cwin_Width = 1.0 * win_Width / win_Height * hv_Height;//高不变计算宽
                cwin_Width = 1.0 * (cwin_Width - hv_Width) / 2;
                HOperatorSet.SetPart(dispWindow, 0, -cwin_Width, hv_Height, cwin_Width + hv_Width);//设置窗体的规格
            }
            dispWindow.ClearWindow();
            HOperatorSet.DispObj(ho_image, dispWindow);
        }
        else
        {
            logService.Error(LogCategory.Running, "传入图片为空,无法显示图片!");
        }
    }
    #endregion
}
