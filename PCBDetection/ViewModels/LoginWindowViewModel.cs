using PCBDetection.Services;
using Prism.Mvvm;

namespace PCBDetection.ViewModels;

public sealed class LoginWindowViewModel : BindableBase
{
    private readonly IAuthenticationService authenticationService;
    private readonly IUserSession userSession;
    private string username;
    private string errorMessage = string.Empty;
    private bool isBusy;

    public LoginWindowViewModel(
        IAuthenticationService authenticationService,
        IUserSession userSession)
    {
        this.authenticationService = authenticationService;
        this.userSession = userSession;
        username = userSession.Username;
    }

    public string Username
    {
        get => username;
        set => SetProperty(ref username, value);
    }

    public string ErrorMessage
    {
        get => errorMessage;
        private set => SetProperty(ref errorMessage, value);
    }

    public bool IsBusy
    {
        get => isBusy;
        private set
        {
            if (SetProperty(ref isBusy, value))
            {
                RaisePropertyChanged(nameof(CanLogin));
            }
        }
    }

    public bool CanLogin => !IsBusy;

    public async Task<bool> LoginAsync(string password)
    {
        if (IsBusy)
        {
            return false;
        }

        ErrorMessage = string.Empty;
        IsBusy = true;
        try
        {
            var result = await authenticationService.AuthenticateAsync(
                Username,
                password,
                CancellationToken.None);

            if (!result.Succeeded || result.User == null)
            {
                ErrorMessage = result.ErrorMessage;
                return false;
            }

            userSession.SignIn(result.User);
            return true;
        }
        finally
        {
            IsBusy = false;
        }
    }
}
