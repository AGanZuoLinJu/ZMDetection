using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using ZMDetection.ViewModels;

namespace ZMDetection.Views
{
    /// <summary>
    /// AddNewRecipeWindow.xaml 的交互逻辑
    /// </summary>
    public partial class AddNewRecipeWindow : Window
    {
        public AddNewRecipeWindow(AddNewRecipeWindowViewModel viewModel)
        {
            InitializeComponent();
            this.DataContext = viewModel;
        }
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
