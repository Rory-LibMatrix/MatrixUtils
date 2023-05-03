using System.Net.Http.Json;
using System.Text.Json;
using MatrixRoomUtils.Extensions;

namespace MatrixRoomUtils;

public class IHomeServer
{
    public string HomeServerDomain { get; set; }
    public string FullHomeServerDomain { get; set; }

    private protected HttpClient _httpClient { get; set; } = new();
    public async Task<string> ResolveHomeserverFromWellKnown(string homeserver)
    {
        Console.WriteLine($"Resolving homeserver: {homeserver}");
        if (!homeserver.StartsWith("http")) homeserver = "https://" + homeserver;
        if (await _httpClient.CheckSuccessStatus($"{homeserver}/.well-known/matrix/client"))
        {
            Console.WriteLine($"Got successful response for client well-known...");
            var resp = await _httpClient.GetFromJsonAsync<JsonElement>($"{homeserver}/.well-known/matrix/client");
            Console.WriteLine($"Response: {resp.ToString()}");
            var hs = resp.GetProperty("m.homeserver").GetProperty("base_url").GetString();
            return hs;
        }
        Console.WriteLine($"No client well-known...");
        if (await _httpClient.CheckSuccessStatus($"{homeserver}/.well-known/matrix/server"))
        {
            var resp = await _httpClient.GetFromJsonAsync<JsonElement>($"{homeserver}/.well-known/matrix/server");
            var hs = resp.GetProperty("m.server").GetString();
            return hs;
        }
        Console.WriteLine($"No server well-known...");
        if (await _httpClient.CheckSuccessStatus($"{homeserver}/_matrix/client/versions")) return homeserver;
        Console.WriteLine($"Failed to resolve homeserver, not on {homeserver}, nor do client or server well-knowns exist!");
        throw new InvalidDataException($"Failed to resolve homeserver, not on {homeserver}, nor do client or server well-knowns exist!");
    }
}