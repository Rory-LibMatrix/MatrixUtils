using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MatrixRoomUtils.Core.Responses;

public class LoginResponse
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; }
    [JsonPropertyName("device_id")]
    public string DeviceId { get; set; }
    [JsonPropertyName("home_server")]
    public string HomeServer { get; set; }
    [JsonPropertyName("user_id")]
    public string UserId { get; set; }
    
    public async Task<ProfileResponse> GetProfile()
    {
        var hc = new HttpClient();
        var resp = await hc.GetAsync($"{HomeServer}/_matrix/client/r0/profile/{UserId}");
        var data = await resp.Content.ReadFromJsonAsync<JsonElement>();
        if(!resp.IsSuccessStatusCode) Console.WriteLine("Profile: " + data.ToString());
        return data.Deserialize<ProfileResponse>();
    }
    public async Task<string> GetCanonicalHomeserverUrl()
    {
        return (await new RemoteHomeServer(HomeServer).Configure()).FullHomeServerDomain;
    }
}