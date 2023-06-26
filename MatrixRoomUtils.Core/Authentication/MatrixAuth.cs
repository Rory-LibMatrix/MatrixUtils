using System.Net.Http.Json;
using System.Text.Json;
using MatrixRoomUtils.Core.Extensions;
using MatrixRoomUtils.Core.Responses;
using MatrixRoomUtils.Core.StateEventTypes;

namespace MatrixRoomUtils.Core.Authentication;

public class MatrixAuth {
    public static async Task<LoginResponse> Login(string homeserver, string username, string password) {
        Console.WriteLine($"Logging in to {homeserver} as {username}...");
        homeserver = (await new RemoteHomeServer(homeserver).Configure()).FullHomeServerDomain;
        var hc = new MatrixHttpClient();
        var payload = new {
            type = "m.login.password",
            identifier = new {
                type = "m.id.user",
                user = username
            },
            password,
            initial_device_display_name = "Rory&::MatrixRoomUtils"
        };
        Console.WriteLine($"Sending login request to {homeserver}...");
        var resp = await hc.PostAsJsonAsync($"{homeserver}/_matrix/client/v3/login", payload);
        Console.WriteLine($"Login: {resp.StatusCode}");
        var data = await resp.Content.ReadFromJsonAsync<JsonElement>();
        if (!resp.IsSuccessStatusCode) Console.WriteLine("Login: " + data);

        Console.WriteLine($"Login: {data.ToJson()}");
        return data.Deserialize<LoginResponse>();
        //var token = data.GetProperty("access_token").GetString();
        //return token;
    }

    [Obsolete("Migrate to IHomeServer instance")]
    public static async Task<ProfileResponse> GetProfile(string homeserver, string mxid) => await (await new RemoteHomeServer(homeserver).Configure()).GetProfile(mxid);

    private static async Task<bool> CheckSuccessStatus(string url) {
        //cors causes failure, try to catch
        try {
            using var hc = new HttpClient();
            var resp = await hc.GetAsync(url);
            return resp.IsSuccessStatusCode;
        }
        catch (Exception e) {
            Console.WriteLine($"Failed to check success status: {e.Message}");
            return false;
        }
    }
}