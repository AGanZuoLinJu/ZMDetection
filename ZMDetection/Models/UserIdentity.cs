namespace ZMDetection.Models;

public sealed class UserIdentity
{
    public UserIdentity(string username, string displayName, UserRole role)
    {
        Username = username;
        DisplayName = displayName;
        Role = role;
    }

    public string Username { get; }

    public string DisplayName { get; }

    public UserRole Role { get; }
}
