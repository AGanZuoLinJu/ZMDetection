using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Security.Cryptography;
using System.Text;
using PCBDetection.Models;

namespace PCBDetection.Services;

public sealed class AuthenticationService : IAuthenticationService
{
    private const int PasswordIterations = 100000;
    private const int SaltSize = 16;
    private const int HashSize = 32;

    private readonly string usersFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config", "Users.json");
    private readonly SemaphoreSlim fileGate = new(1, 1);
    private readonly ILogService logService;

    public AuthenticationService(ILogService logService)
    {
        this.logService = logService;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        await fileGate.WaitAsync(cancellationToken);
        try
        {
            EnsureUsersFileExists();
        }
        finally
        {
            fileGate.Release();
        }
    }

    public async Task<AuthenticationResult> AuthenticateAsync(
        string username,
        string password,
        CancellationToken cancellationToken)
    {
        string normalizedUsername = username?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(normalizedUsername) ||
            string.IsNullOrEmpty(password))
        {
            return AuthenticationResult.Failure("请输入账号和密码。");
        }

        await fileGate.WaitAsync(cancellationToken);
        try
        {
            EnsureUsersFileExists();
            UserStore store = ReadUsersFile();
            StoredUser? storedUser = store.Users.FirstOrDefault(user =>
                string.Equals(
                    user.Username?.Trim(),
                    normalizedUsername,
                    StringComparison.OrdinalIgnoreCase));

            if (storedUser == null || !storedUser.Enabled)
            {
                logService.Warning(
                    LogCategory.Running,
                    $"登录失败，账号不存在或已停用: {normalizedUsername}");
                return AuthenticationResult.Failure("账号或密码错误。");
            }

            if (!Enum.TryParse(storedUser.Role, true, out UserRole role))
            {
                throw new SerializationException(
                    $"用户 {storedUser.Username} 的角色配置无效。");
            }

            byte[] salt = Convert.FromBase64String(storedUser.PasswordSalt);
            byte[] expectedHash = Convert.FromBase64String(storedUser.PasswordHash);
            byte[] actualHash = HashPassword(password, salt);

            if (!FixedTimeEquals(expectedHash, actualHash))
            {
                logService.Warning(
                    LogCategory.Running,
                    $"登录失败，密码错误: {normalizedUsername}");
                return AuthenticationResult.Failure("账号或密码错误。");
            }

            var identity = new UserIdentity(
                storedUser.Username,
                string.IsNullOrWhiteSpace(storedUser.DisplayName)
                    ? storedUser.Username
                    : storedUser.DisplayName,
                role);
            logService.Info(
                LogCategory.Running,
                $"用户登录成功: {identity.Username} ({GetRoleName(role)})");
            return AuthenticationResult.Success(identity);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logService.Error(
                LogCategory.Running,
                $"用户配置读取失败: {ex.Message}");
            return AuthenticationResult.Failure(
                "用户配置文件读取失败，请检查 Config/Users.json。");
        }
        finally
        {
            fileGate.Release();
        }
    }

    private void EnsureUsersFileExists()
    {
        if (File.Exists(usersFilePath))
        {
            return;
        }

        string? directory = Path.GetDirectoryName(usersFilePath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }
        //如果没有文件则新增三个默认账号
        var store = new UserStore
        {
            Version = 1,
            Users = new List<StoredUser>
            {
                CreateDefaultUser("admin", "管理员", UserRole.Administrator, "123456"),
                CreateDefaultUser("1", "工程师", UserRole.Engineer, "1111"),
                CreateDefaultUser("operator", "操作员", UserRole.Operator, "1111")
            }
        };

        WriteUsersFile(store);
        logService.Info(LogCategory.Running, "已创建默认用户配置文件。");
    }

    private UserStore ReadUsersFile()
    {
        var serializer = new DataContractJsonSerializer(typeof(UserStore));
        using var stream = File.OpenRead(usersFilePath);
        var store = serializer.ReadObject(stream) as UserStore;
        if (store?.Users == null || store.Users.Count == 0)
        {
            throw new SerializationException("用户列表为空。");
        }

        return store;
    }

    private void WriteUsersFile(UserStore store)
    {
        var serializer = new DataContractJsonSerializer(typeof(UserStore));
        using var stream = File.Create(usersFilePath);
        serializer.WriteObject(stream, store);
    }

    private static StoredUser CreateDefaultUser(
        string username,
        string displayName,
        UserRole role,
        string password)
    {
        byte[] salt = new byte[SaltSize];
        using (RandomNumberGenerator random = RandomNumberGenerator.Create())
        {
            random.GetBytes(salt);
        }

        return new StoredUser
        {
            Username = username,
            DisplayName = displayName,
            Role = role.ToString(),
            Enabled = true,
            PasswordSalt = Convert.ToBase64String(salt),
            PasswordHash = Convert.ToBase64String(HashPassword(password, salt))
        };
    }
    private static byte[] HashPassword(string password, byte[] salt)
    {
        using var deriveBytes = new Rfc2898DeriveBytes(
            password,
            salt,
            PasswordIterations,
            HashAlgorithmName.SHA256);
        return deriveBytes.GetBytes(HashSize);
    }
    private static bool FixedTimeEquals(byte[] expected, byte[] actual)
    {
        int difference = expected.Length ^ actual.Length;
        int length = Math.Min(expected.Length, actual.Length);
        for (int index = 0; index < length; index++)
        {
            difference |= expected[index] ^ actual[index];
        }

        return difference == 0;
    }

    private static string GetRoleName(UserRole role) => role switch
    {
        UserRole.Administrator => "管理员",
        UserRole.Engineer => "工程师",
        _ => "操作员"
    };

    [DataContract]
    private sealed class UserStore
    {
        [DataMember(Order = 1)]
        public int Version { get; set; }

        [DataMember(Order = 2)]
        public List<StoredUser> Users { get; set; } = new();
    }

    [DataContract]
    private sealed class StoredUser
    {
        [DataMember(Order = 1)]
        public string Username { get; set; } = string.Empty;

        [DataMember(Order = 2)]
        public string DisplayName { get; set; } = string.Empty;

        [DataMember(Order = 3)]
        public string Role { get; set; } = string.Empty;

        [DataMember(Order = 4)]
        public bool Enabled { get; set; }

        [DataMember(Order = 5)]
        public string PasswordSalt { get; set; } = string.Empty;

        [DataMember(Order = 6)]
        public string PasswordHash { get; set; } = string.Empty;
    }
}
