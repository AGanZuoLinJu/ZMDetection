using System.Windows;
using LiveChartsCore;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using Prism.Commands;
using Prism.Mvvm;
using SkiaSharp;
using ZMDetection.Models;
using ZMDetection.Services;

namespace ZMDetection.ViewModels;

public sealed class ProductionStatisticsViewModel : BindableBase
{
    private static readonly SKColor OkColor = new(23, 163, 74);
    private static readonly SKColor NgColor = new(220, 38, 38);
    private static readonly SKColor PrimaryColor = new(47, 111, 235);
    private static readonly SKColor MutedColor = new(107, 114, 128);
    private readonly IProductionStatisticsService statisticsService;
    private DateTime? selectedDate = DateTime.Today;
    private int totalCount;
    private int okCount;
    private int ngCount;
    private int defectCount;
    private double yieldRate;
    private double averageCycleTime;
    private bool hasDefectData;
    private ISeries[] dailyResultSeries = Array.Empty<ISeries>();
    private ISeries[] defectSeries = Array.Empty<ISeries>();
    private ISeries[] trendSeries = Array.Empty<ISeries>();
    private Axis[] defectXAxes = Array.Empty<Axis>();
    private Axis[] defectYAxes = Array.Empty<Axis>();
    private Axis[] trendXAxes = Array.Empty<Axis>();
    private Axis[] trendYAxes = Array.Empty<Axis>();

    public ProductionStatisticsViewModel(IProductionStatisticsService statisticsService)
    {
        this.statisticsService = statisticsService;
        TodayCommand = new DelegateCommand(() => SelectedDate = DateTime.Today);
        WeakEventManager<IProductionStatisticsService, ProductionStatisticsChangedEventArgs>.AddHandler(
            statisticsService,
            nameof(IProductionStatisticsService.StatisticsChanged),
            OnStatisticsChanged);
        RefreshCharts();
    }
    public DateTime? SelectedDate
    {
        get => selectedDate;
        set
        {
            DateTime normalized = (value ?? DateTime.Today).Date;
            if (SetProperty(ref selectedDate, normalized))
            {
                RefreshCharts();
            }
        }
    }

    public int TotalCount
    {
        get => totalCount;
        private set => SetProperty(ref totalCount, value);
    }

    public int OkCount
    {
        get => okCount;
        private set => SetProperty(ref okCount, value);
    }

    public int NgCount
    {
        get => ngCount;
        private set => SetProperty(ref ngCount, value);
    }

    public int DefectCount
    {
        get => defectCount;
        private set => SetProperty(ref defectCount, value);
    }

    public double YieldRate
    {
        get => yieldRate;
        private set => SetProperty(ref yieldRate, value);
    }

    public double AverageCycleTime
    {
        get => averageCycleTime;
        private set => SetProperty(ref averageCycleTime, value);
    }

    public bool HasDefectData
    {
        get => hasDefectData;
        private set => SetProperty(ref hasDefectData, value);
    }

    public ISeries[] DailyResultSeries
    {
        get => dailyResultSeries;
        private set => SetProperty(ref dailyResultSeries, value);
    }

    public ISeries[] DefectSeries
    {
        get => defectSeries;
        private set => SetProperty(ref defectSeries, value);
    }

    public ISeries[] TrendSeries
    {
        get => trendSeries;
        private set => SetProperty(ref trendSeries, value);
    }

    public Axis[] DefectXAxes
    {
        get => defectXAxes;
        private set => SetProperty(ref defectXAxes, value);
    }

    public Axis[] DefectYAxes
    {
        get => defectYAxes;
        private set => SetProperty(ref defectYAxes, value);
    }

    public Axis[] TrendXAxes
    {
        get => trendXAxes;
        private set => SetProperty(ref trendXAxes, value);
    }

    public Axis[] TrendYAxes
    {
        get => trendYAxes;
        private set => SetProperty(ref trendYAxes, value);
    }

    public DelegateCommand TodayCommand { get; }
    /// <summary>
    /// 刷新图表数据
    /// </summary>
    private void RefreshCharts()
    {
        DateTime date = (SelectedDate ?? DateTime.Today).Date;
        ProductionStatisticsSnapshot day = statisticsService.GetByDate(date);
        IReadOnlyList<ProductionStatisticsSnapshot> range = statisticsService.GetRange(date, 7);

        TotalCount = day.TotalCount;
        OkCount = day.OkCount;
        NgCount = day.NgCount;
        DefectCount = day.DefectCount;
        YieldRate = day.YieldRate;
        AverageCycleTime = day.AverageCycleTimeMilliseconds;

        //创建良率统计环形图
        DailyResultSeries = new ISeries[]
        {
            CreatePieSeries("OK", day.OkCount, OkColor),
            CreatePieSeries("NG", day.NgCount, NgColor)
        };
        //缺陷统计图表
        //根据缺陷名选择缺陷数据
        KeyValuePair<string, int>[]? defects = day.DefectsByName
            .Where(item => item.Value > 0)
            .OrderByDescending(item => item.Value)
            .ThenBy(item => item.Key)
            .ToArray();
        HasDefectData = defects.Length > 0;
        DefectSeries = HasDefectData
            ? new ISeries[]
            {
                new RowSeries<int>
                {
                    Name = "缺陷数",
                    Values = defects.Select(item => item.Value).ToArray(),
                    Fill = new SolidColorPaint(PrimaryColor),
                    Stroke = null,
                    DataLabelsPaint = new SolidColorPaint(MutedColor),
                    DataLabelsPosition = DataLabelsPosition.End,
                    DataLabelsFormatter = point => point.Coordinate.PrimaryValue.ToString("N0"),
                    XToolTipLabelFormatter = point =>
                    {
                        if (point.Index < 0 || point.Index >= defects.Length)
                        {
                            return $"缺陷数：{point.Model:N0}";
                        }

                        string defectName = defects[point.Index].Key;
                        int count = point.Model;
                        return $"{defectName}：{count}";
                    }
                }
            }
            : Array.Empty<ISeries>();
        DefectXAxes = new[]
        {
            new Axis
            {
                MinLimit = 0,
                MinStep = 1,
                Labeler = value => value.ToString("N0"),
                SeparatorsPaint = new SolidColorPaint(new SKColor(229, 231, 235))
            }
        };
        DefectYAxes = new[]
        {
            new Axis
            {
                Labels = defects.Select(item => item.Key).ToArray(),
                LabelsPaint = new SolidColorPaint(MutedColor),
                SeparatorsPaint = null,
                MinStep = 1,
                ForceStepToMin = true
            }
        };

        //生产良率图表
        TrendSeries = new ISeries[]
        {
            new ColumnSeries<int>
            {
                Name = "OK",
                Values = range.Select(item => item.OkCount).ToArray(),
                Fill = new SolidColorPaint(OkColor),
                Stroke = null
            },
            new ColumnSeries<int>
            {
                Name = "NG",
                Values = range.Select(item => item.NgCount).ToArray(),
                Fill = new SolidColorPaint(NgColor),
                Stroke = null
            },
            new LineSeries<double>
            {
                Name = "良率",
                Values = range.Select(item => item.YieldRate).ToArray(),
                ScalesYAt = 1,
                Fill = null,
                Stroke = new SolidColorPaint(PrimaryColor, 3),
                GeometryFill = new SolidColorPaint(SKColors.White),
                GeometryStroke = new SolidColorPaint(PrimaryColor, 3),
                GeometrySize = 9
            }
        };
        TrendXAxes = new[]
        {
            new Axis
            {
                Labels = range.Select(item => item.Date.ToString("MM-dd")).ToArray(),
                LabelsPaint = new SolidColorPaint(MutedColor),
                SeparatorsPaint = null
            }
        };
        TrendYAxes = new[]
        {
            new Axis
            {
                Name = "数量",
                MinLimit = 0,
                MinStep = 1,
                Labeler = value => value.ToString("N0"),
                SeparatorsPaint = new SolidColorPaint(new SKColor(229, 231, 235))
            },
            new Axis
            {
                Name = "良率",
                Position = AxisPosition.End,
                MinLimit = 0,
                MaxLimit = 100,
                Labeler = value => $"{value:N2}%",
                SeparatorsPaint = null
            }
        };
    }

    private static PieSeries<int> CreatePieSeries(string name, int value, SKColor color)
    {
        return new PieSeries<int>
        {
            Name = name,
            Values = new[] { value },
            Fill = new SolidColorPaint(color),
            Stroke = null,
            InnerRadius = 65,
            DataLabelsPaint = new SolidColorPaint(SKColors.White),
            DataLabelsPosition = PolarLabelsPosition.Middle,
            DataLabelsFormatter = point => point.Coordinate.PrimaryValue <= 0
                ? string.Empty
                : point.Coordinate.PrimaryValue.ToString("N0")
        };
    }

    private void OnStatisticsChanged(object? sender, ProductionStatisticsChangedEventArgs args)
    {
        DateTime selected = (SelectedDate ?? DateTime.Today).Date;
        if (args.Date < selected.AddDays(-6) || args.Date > selected)
        {
            return;
        }

        void Refresh() => RefreshCharts();
        if (Application.Current?.Dispatcher.CheckAccess() == true)
        {
            Refresh();
        }
        else
        {
            Application.Current?.Dispatcher.BeginInvoke((Action)Refresh);
        }
    }
}
