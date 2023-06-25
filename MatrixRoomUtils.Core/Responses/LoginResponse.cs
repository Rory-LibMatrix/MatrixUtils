using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using MatrixRoomUtils.Core.StateEventTypes;

namespace MatrixRoomUtils.Core.Responses;

public class LoginResponse {
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; }

    [JsonPropertyName("device_id")]
    public string DeviceId { get; set; }

    [JsonPropertyName("home_server")]
    public string HomeServer { get; set; }

    [JsonPropertyName("user_id")]
    public string UserId { get; set; }

    public async Task<ProfileResponse> GetProfile() {
        var hc = new HttpClient();
        var resp = await hc.GetAsync($"{HomeServer}/_matrix/client/v3/profile/{UserId}");
        var data = await resp.Content.ReadFromJsonAsync<JsonElement>();
        if (!resp.IsSuccessStatusCode) Console.WriteLine("Profile: " + data);
        return data.Deserialize<ProfileResponse>();
    }

    public async Task<string> GetCanonicalHomeserverUrl() => (await new RemoteHomeServer(HomeServer).Configure()).FullHomeServerDomain;
}