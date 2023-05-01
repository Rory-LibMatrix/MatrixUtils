using System.Net.Http.Json;
using System.Text.Json;
using MatrixRoomUtils.Responses;

namespace MatrixRoomUtils.Authentication;

public class MatrixAccount
{
    public static async Task<LoginResponse> Login(string homeserver, string username, string password)
    {
        Console.WriteLine($"Logging in to {homeserver} as {username}...");
        homeserver = await ResolveHomeserverFromWellKnown(homeserver);
        var hc = new HttpClient();
        var payload = new
        {
            type = "m.login.password",
            identifier = new
            {
                type = "m.id.user",
                user = username
            },
            password = password,
            initial_device_display_name = "Rory&::MatrixRoomUtils"
        };
        Console.WriteLine($"Sending login request to {homeserver}...");
        var resp = await hc.PostAsJsonAsync($"{homeserver}/_matrix/client/r0/login", payload);
        Console.WriteLine($"Login: {resp.StatusCode}");
        var data = await resp.Content.ReadFromJsonAsync<JsonElement>();
        if (!resp.IsSuccessStatusCode) Console.WriteLine("Login: " + data.ToString());
        if (data.TryGetProperty("retry_after_ms", out var retryAfter))
        {
            Console.WriteLine($"Login: Waiting {retryAfter.GetInt32()}ms before retrying");
            await Task.Delay(retryAfter.GetInt32());
            return await Login(homeserver, username, password);
        }

        return data.Deserialize<LoginResponse>();
        //var token = data.GetProperty("access_token").GetString();
        //return token;
    }

    public static async Task<ProfileResponse> GetProfile(string homeserver, string mxid)
    {
        Console.WriteLine($"Fetching profile for {mxid} on {homeserver}...");
        homeserver = await ResolveHomeserverFromWellKnown(homeserver);
        var hc = new HttpClient();
        var resp = await hc.GetAsync($"{homeserver}/_matrix/client/r0/profile/{mxid}");
        var data = await resp.Content.ReadFromJsonAsync<JsonElement>();
        if (!resp.IsSuccessStatusCode) Console.WriteLine("Profile: " + data.ToString());
        return data.Deserialize<ProfileResponse>();
    }

    public static async Task<string> ResolveHomeserverFromWellKnown(string homeserver)
    {
        using var hc = new HttpClient();
        Console.WriteLine($"Resolving homeserver: {homeserver}");
        if (!homeserver.StartsWith("http")) homeserver = "https://" + homeserver;

        if (await CheckSuccessStatus($"{homeserver}/.well-known/matrix/client"))
        {
            var resp = await hc.GetFromJsonAsync<JsonElement>($"{homeserver}/.well-known/matrix/client");
            var hs = resp.GetProperty("m.homeserver").GetProperty("base_url").GetString();
            return hs;
        }
        Console.WriteLine($"No client well-known...");
        if (await CheckSuccessStatus($"{homeserver}/.well-known/matrix/server"))
        {
            var resp = await hc.GetFromJsonAsync<JsonElement>($"{homeserver}/.well-known/matrix/server");
            var hs = resp.GetProperty("m.server").GetString();
            return hs;
        }
        Console.WriteLine($"No server well-known...");
        if (await CheckSuccessStatus($"{homeserver}/_matrix/client/versions")) return homeserver;
        Console.WriteLine($"Failed to resolve homeserver, not on {homeserver}, nor do client or server well-knowns exist!");
        throw new InvalidDataException($"Failed to resolve homeserver, not on {homeserver}, nor do client or server well-knowns exist!");
    }

    private static async Task<bool> CheckSuccessStatus(string url)
    {
        //cors causes failure, try to catch
        try
        {
            using var hc = new HttpClient();
            var resp = await hc.GetAsync(url);
            return resp.IsSuccessStatusCode;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Failed to check success status: {e.Message}");
            return false;
        }
    }
}