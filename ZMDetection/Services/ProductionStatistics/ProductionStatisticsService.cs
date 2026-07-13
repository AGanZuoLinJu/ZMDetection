using ZMDetection.Models;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

namespace ZMDetection.Services;

public sealed class ProductionStatisticsService : IProductionStatisticsService
{
    private readonly object syncRoot = new();
    private readonly Dictionary<DateTime, DailyStatisticsRecord> records = new();
    private readonly ILogService logService;
    private readonly string statisticsDirectory;

    public ProductionStatisticsService(ILogService logService)
    {
        this.logService = logService;
        statisticsDirectory = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "Data",
            "ProductionStatistics");
    }

    public event EventHandler<ProductionStatisticsChangedEventArgs>? StatisticsChanged;

    public ProductionStatisticsSnapshot Current => GetByDate(DateTime.Today);
    /// <summary>
    /// 加载产量数据
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task LoadAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        GetByDate(DateTime.Today);
        return Task.CompletedTask;
    }
    /// <summary>
    /// 获取当天的产量数据
    /// </summary>
    /// <param name="date"></param>
    /// <returns></returns>
    public ProductionStatisticsSnapshot GetByDate(DateTime date)
    {
        lock (syncRoot)
        {
            return CreateSnapshot(GetOrLoadRecord(date.Date));
        }
    }
    /// <summary>
    /// 获取指定天数的产量数据
    /// </summary>
    /// <param name="endDate"></param>
    /// <param name="days"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public IReadOnlyList<ProductionStatisticsSnapshot> GetRange(DateTime endDate, int days)
    {
        if (days <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(days), "天数必须大于 0。");
        }

        lock (syncRoot)
        {
            DateTime firstDate = endDate.Date.AddDays(-(days - 1));
            var result = new List<ProductionStatisticsSnapshot>(days);
            for (int index = 0; index < days; index++)
            {
                result.Add(CreateSnapshot(GetOrLoadRecord(firstDate.AddDays(index))));
            }

            return result;
        }
    }

    public void ApplyResult(InspectionResult result)
    {
        DateTime date = DateTime.Today;
        lock (syncRoot)
        {
            DailyStatisticsRecord record = GetOrLoadRecord(date);
            if (result.IsOk)
            {
                record.OkCount++;
            }
            else
            {
                record.NgCount++;
            }

            record.DefectCount += result.DefectCount;
            record.LastCycleTimeMilliseconds = result.CycleTimeMilliseconds;
            record.TotalCycleTimeMilliseconds += result.CycleTimeMilliseconds;

            foreach (DefectDetail defect in result.Defects)
            {
                string name = string.IsNullOrWhiteSpace(defect.Name) ? "未分类" : defect.Name.Trim();
                DefectCountRecord? defectRecord = record.Defects.FirstOrDefault(item =>
                    string.Equals(item.Name, name, StringComparison.OrdinalIgnoreCase));
                if (defectRecord == null)
                {
                    record.Defects.Add(new DefectCountRecord { Name = name, Count = 1 });
                }
                else
                {
                    defectRecord.Count++;
                }
            }

            int unclassifiedCount = Math.Max(0, result.DefectCount - result.Defects.Count);
            if (unclassifiedCount > 0)
            {
                DefectCountRecord? unclassified = record.Defects.FirstOrDefault(item => item.Name == "未分类");
                if (unclassified == null)
                {
                    record.Defects.Add(new DefectCountRecord { Name = "未分类", Count = unclassifiedCount });
                }
                else
                {
                    unclassified.Count += unclassifiedCount;
                }
            }

            SaveRecord(record);
        }

        StatisticsChanged?.Invoke(this, new ProductionStatisticsChangedEventArgs(date));
    }

    public void Reset()
    {
        DateTime date = DateTime.Today;
        lock (syncRoot)
        {
            var record = new DailyStatisticsRecord { Date = date };
            records[date] = record;
            SaveRecord(record);
        }

        StatisticsChanged?.Invoke(this, new ProductionStatisticsChangedEventArgs(date));
    }

    public Task SaveCsvAsync(string filePath, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        ProductionStatisticsSnapshot snapshot = Current;
        var csv = "OK,NG,Defects,Yield,CycleTimeMs" + Environment.NewLine +
                  $"{snapshot.OkCount},{snapshot.NgCount},{snapshot.DefectCount},{snapshot.YieldRate},{snapshot.LastCycleTimeMilliseconds}";
        File.WriteAllText(filePath, csv);
        return Task.CompletedTask;
    }

    private DailyStatisticsRecord GetOrLoadRecord(DateTime date)
    {
        date = date.Date;
        if (records.TryGetValue(date, out DailyStatisticsRecord? cached))
        {
            return cached;
        }

        DailyStatisticsRecord loaded = LoadRecord(date);
        records[date] = loaded;
        return loaded;
    }

    private DailyStatisticsRecord LoadRecord(DateTime date)
    {
        string filePath = GetFilePath(date);
        if (!File.Exists(filePath))
        {
            return new DailyStatisticsRecord { Date = date };
        }

        try
        {
            using var stream = File.OpenRead(filePath);
            var serializer = new DataContractJsonSerializer(typeof(DailyStatisticsRecord));
            var record = serializer.ReadObject(stream) as DailyStatisticsRecord;
            if (record == null)
            {
                throw new SerializationException("统计文件内容为空。");
            }

            record.Date = date;
            record.Defects ??= new List<DefectCountRecord>();
            return record;
        }
        catch (Exception ex)
        {
            PreserveCorruptedFile(filePath);
            logService.Error(LogCategory.Running, $"生产统计文件读取失败: {filePath}", ex);
            return new DailyStatisticsRecord { Date = date };
        }
    }

    private void SaveRecord(DailyStatisticsRecord record)
    {
        Directory.CreateDirectory(statisticsDirectory);
        string filePath = GetFilePath(record.Date);
        string tempPath = filePath + ".tmp";

        try
        {
            using (var stream = File.Create(tempPath))
            {
                var serializer = new DataContractJsonSerializer(typeof(DailyStatisticsRecord));
                serializer.WriteObject(stream, record);
            }

            if (File.Exists(filePath))
            {
                File.Replace(tempPath, filePath, null);
            }
            else
            {
                File.Move(tempPath, filePath);
            }
        }
        catch (Exception ex)
        {
            if (File.Exists(tempPath))
            {
                File.Delete(tempPath);
            }

            logService.Error(LogCategory.Running, $"生产统计文件保存失败: {filePath}", ex);
        }
    }

    private void PreserveCorruptedFile(string filePath)
    {
        try
        {
            string backupPath = $"{filePath}.corrupt-{DateTime.Now:yyyyMMddHHmmss}";
            File.Copy(filePath, backupPath, false);
        }
        catch (Exception ex)
        {
            logService.Error(LogCategory.Running, $"生产统计损坏文件备份失败: {filePath}", ex);
        }
    }

    private string GetFilePath(DateTime date) =>
        Path.Combine(statisticsDirectory, $"{date:yyyy-MM-dd}.json");

    private static ProductionStatisticsSnapshot CreateSnapshot(DailyStatisticsRecord record)
    {
        IReadOnlyDictionary<string, int> defects = record.Defects
            .Where(item => !string.IsNullOrWhiteSpace(item.Name) && item.Count > 0)
            .GroupBy(item => item.Name, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.First().Name, group => group.Sum(item => item.Count));

        return new ProductionStatisticsSnapshot(
            record.Date,
            record.OkCount,
            record.NgCount,
            record.DefectCount,
            record.LastCycleTimeMilliseconds,
            record.TotalCycleTimeMilliseconds,
            defects);
    }

    [DataContract]
    private sealed class DailyStatisticsRecord
    {
        [DataMember(Order = 1)]
        public DateTime Date { get; set; }

        [DataMember(Order = 2)]
        public int OkCount { get; set; }

        [DataMember(Order = 3)]
        public int NgCount { get; set; }

        [DataMember(Order = 4)]
        public int DefectCount { get; set; }

        [DataMember(Order = 5)]
        public double LastCycleTimeMilliseconds { get; set; }

        [DataMember(Order = 6)]
        public double TotalCycleTimeMilliseconds { get; set; }

        [DataMember(Order = 7)]
        public List<DefectCountRecord> Defects { get; set; } = new();
    }

    [DataContract]
    private sealed class DefectCountRecord
    {
        [DataMember(Order = 1)]
        public string Name { get; set; } = string.Empty;

        [DataMember(Order = 2)]
        public int Count { get; set; }
    }
}
