using System.Windows;
using ZMDetection.ViewModels;

namespace ZMDetection.Views;

public partial class LoginWindow : Window
{
    private readonly LoginWindowViewModel viewModel;
    public LoginWindow(LoginWindowViewModel viewModel)
    {
        InitializeComponent();
        this.viewModel = viewModel;
        DataContext = viewModel;
        Loaded += (_, _) =>
        {
            if (string.IsNullOrWhiteSpace(viewModel.Username))
            {
                UsernameTextBox.Focus();
            }
            else
            {
                PasswordInput.Focus();
            }
        };
    }

    private async void LoginButton_Click(object sender, RoutedEventArgs e)
    {
        if (await viewModel.LoginAsync(PasswordInput.Password))
        {
            DialogResult = true;
        }
        else
        {
            PasswordInput.SelectAll();
            PasswordInput.Focus();
        }
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
    }
}
