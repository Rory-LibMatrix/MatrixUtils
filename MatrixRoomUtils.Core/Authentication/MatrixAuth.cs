using System.Net.Http.Json;
using System.Text.Json;
using MatrixRoomUtils.Core.Extensions;
using MatrixRoomUtils.Core.Responses;
using MatrixRoomUtils.Core.StateEventTypes;

namespace MatrixRoomUtils.Core.Authentication;

public class MatrixAuth {
    [Obsolete("This is possibly broken and should not be used.", true)]
    public static async Task<LoginResponse> Login(string homeserver, string username, string password) {
        Console.WriteLine($"Logging in to {homeserver} as {username}...");
        homeserver = (new RemoteHomeServer(homeserver)).FullHomeServerDomain;
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
}