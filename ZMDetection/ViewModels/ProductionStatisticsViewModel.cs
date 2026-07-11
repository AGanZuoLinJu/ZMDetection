using Prism.Mvvm;

namespace ZMDetection.ViewModels;

public sealed class ProductionStatisticsViewModel : BindableBase
{
    public string Title => "生产统计";

    public string Description => "Production trends and historical reports will be displayed here.";
}
