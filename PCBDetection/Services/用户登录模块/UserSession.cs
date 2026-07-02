using PCBDetection.Models;
using Prism.Mvvm;
using System.Windows.Threading;

namespace PCBDetection.Services;

public sealed class UserSession : BindableBase, IUserSession
{
    private static readonly TimeSpan SessionDuration = TimeSpan.FromMinutes(5);
    private readonly DispatcherTimer expirationTimer;
    private readonly ILogService logService;
    private UserIdentity? currentUser;

    public UserSession(ILogService logService)
    {
        this.logService = logService;
        expirationTimer = new DispatcherTimer
        {
            Interval = SessionDuration
        };
        expirationTimer.Tick += OnExpirationTimerTick;
    }

    public event EventHandler? SessionExpired;

    public bool IsAuthenticated => currentUser != null;

    public string Username => currentUser?.Username ?? string.Empty;

    public string DisplayName => currentUser?.DisplayName ?? "未登录";

    public UserRole? Role => currentUser?.Role;

    public string RoleDisplayName => currentUser?.Role switch
    {
        UserRole.Administrator => "管理员",
        UserRole.Engineer => "工程师",
        UserRole.Operator => "操作员",
        _ => "参数设置受限"
    };

    public string PermissionDescription => IsAuthenticated
        ? RoleDisplayName
        : "参数设置受限";

    public string AvatarText => string.IsNullOrWhiteSpace(DisplayName)
        ? "未"
        : DisplayName.Substring(0, 1);

    public bool CanAccessParameterSettings =>
        currentUser?.Role is UserRole.Engineer or UserRole.Administrator;

    public void SignIn(UserIdentity user)
    {
        currentUser = user ?? throw new ArgumentNullException(nameof(user));
        NotifySessionChanged();
        expirationTimer.Stop();
        expirationTimer.Start();
    }

    private void OnExpirationTimerTick(object? sender, EventArgs e)
    {
        expirationTimer.Stop();
        if (currentUser == null)
        {
            return;
        }

        string expiredUsername = currentUser.Username;
        currentUser = null;
        NotifySessionChanged();
        logService.Info(
            LogCategory.Running,
            $"用户登录已超时并自动退出: {expiredUsername}");
        SessionExpired?.Invoke(this, EventArgs.Empty);
    }

    private void NotifySessionChanged()
    {
        RaisePropertyChanged(nameof(IsAuthenticated));
        RaisePropertyChanged(nameof(Username));
        RaisePropertyChanged(nameof(DisplayName));
        RaisePropertyChanged(nameof(Role));
        RaisePropertyChanged(nameof(RoleDisplayName));
        RaisePropertyChanged(nameof(PermissionDescription));
        RaisePropertyChanged(nameof(AvatarText));
        RaisePropertyChanged(nameof(CanAccessParameterSettings));
    }
}
