namespace ZMDetection.Models;

public sealed class ProductionStatisticsChangedEventArgs : EventArgs
{
    public ProductionStatisticsChangedEventArgs(DateTime date)
    {
        Date = date.Date;
    }

    public DateTime Date { get; }
}
