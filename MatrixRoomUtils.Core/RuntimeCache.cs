using MatrixRoomUtils.Core.Responses;

namespace MatrixRoomUtils.Core;

public class RuntimeCache
{
    public static bool WasLoaded = false;
    public static string? LastUsedToken { get; set; }
    public static AuthenticatedHomeServer CurrentHomeServer { get; set; }
    public static Dictionary<string, UserInfo> LoginSessions { get; set; } = new();

    public static Dictionary<string, HomeServerResolutionResult> HomeserverResolutionCache { get; set; } = new();
    // public static Dictionary<string, (DateTime cachedAt, ProfileResponse response)> ProfileCache { get; set; } = new();
}


public class UserInfo
{
    public ProfileResponse Profile { get; set; } = new();
    public LoginResponse LoginResponse { get; set; }
    public string AccessToken { get => LoginResponse.AccessToken; }
}

public class HomeServerResolutionResult
{
    public string Result { get; set; }
    public DateTime ResolutionTime { get; set; }
}
