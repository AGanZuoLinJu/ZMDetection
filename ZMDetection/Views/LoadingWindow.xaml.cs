using System.Windows;
using ZMDetection.ViewModels;

namespace ZMDetection.Views;

public partial class LoadingWindow : Window
{
    public LoadingWindow(LoadingWindowViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
