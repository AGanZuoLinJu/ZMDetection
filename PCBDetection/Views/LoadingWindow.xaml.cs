using System.Windows;
using PCBDetection.ViewModels;

namespace PCBDetection.Views;

public partial class LoadingWindow : Window
{
    public LoadingWindow(LoadingWindowViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
