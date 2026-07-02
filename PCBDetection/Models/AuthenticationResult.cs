namespace PCBDetection.Models;

public sealed class AuthenticationResult
{
    private AuthenticationResult(bool succeeded, UserIdentity? user, string errorMessage)
    {
        Succeeded = succeeded;
        User = user;
        ErrorMessage = errorMessage;
    }

    public bool Succeeded { get; }

    public UserIdentity? User { get; }

    public string ErrorMessage { get; }

    public static AuthenticationResult Success(UserIdentity user) =>
        new(true, user, string.Empty);

    public static AuthenticationResult Failure(string errorMessage) =>
        new(false, null, errorMessage);
}
