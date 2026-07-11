using System.ComponentModel;
using ZMDetection.Models;

namespace ZMDetection.Services;

public interface IUserSession : INotifyPropertyChanged
{
    event EventHandler? SessionExpired;

    bool IsAuthenticated { get; }

    string Username { get; }

    string DisplayName { get; }

    UserRole? Role { get; }

    string RoleDisplayName { get; }

    string PermissionDescription { get; }

    string AvatarText { get; }

    bool CanAccessParameterSettings { get; }

    void SignIn(UserIdentity user);
}
