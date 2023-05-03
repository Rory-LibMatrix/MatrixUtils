using System.Net.Http.Json;
using System.Text.Json;
using MatrixRoomUtils.Core.Responses;

namespace MatrixRoomUtils.Core.Authentication;

public class MatrixAuth
{
    public static async Task<LoginResponse> Login(string homeserver, string username, string password)
    {
        Console.WriteLine($"Logging in to {homeserver} as {username}...");
        homeserver = (await new RemoteHomeServer(homeserver).Configure()).FullHomeServerDomain;
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

    public static async Task<ProfileResponse> GetProfile(string homeserver, string mxid) => 
        await (await new RemoteHomeServer(homeserver).Configure()).GetProfile(mxid);

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