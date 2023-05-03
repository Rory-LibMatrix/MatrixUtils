using System.Net.Http.Json;
using System.Text.Json;
using MatrixRoomUtils.Core.Extensions;

namespace MatrixRoomUtils.Core.Interfaces;

public class IHomeServer
{
    public string HomeServerDomain { get; set; }
    public string FullHomeServerDomain { get; set; }

    private protected HttpClient _httpClient { get; set; } = new();

    public async Task<string> ResolveHomeserverFromWellKnown(string homeserver)
    {
        if (RuntimeCache.HomeserverResolutionCache.ContainsKey(homeserver))
        {
            if (RuntimeCache.HomeserverResolutionCache[homeserver].ResolutionTime < DateTime.Now.AddHours(1))
            {
                Console.WriteLine($"Found cached homeserver: {RuntimeCache.HomeserverResolutionCache[homeserver].Result}");
                return RuntimeCache.HomeserverResolutionCache[homeserver].Result;
            }
            RuntimeCache.HomeserverResolutionCache.Remove(homeserver);
        }
        //throw new NotImplementedException();

        string result = null;
        Console.WriteLine($"Resolving homeserver: {homeserver}");
        if (!homeserver.StartsWith("http")) homeserver = "https://" + homeserver;
        if (await _httpClient.CheckSuccessStatus($"{homeserver}/.well-known/matrix/client"))
        {
            Console.WriteLine($"Got successful response for client well-known...");
            var resp = await _httpClient.GetFromJsonAsync<JsonElement>($"{homeserver}/.well-known/matrix/client");
            Console.WriteLine($"Response: {resp.ToString()}");
            var hs = resp.GetProperty("m.homeserver").GetProperty("base_url").GetString();
            result = hs;
        }
        else
        {
            Console.WriteLine($"No client well-known...");
            if (await _httpClient.CheckSuccessStatus($"{homeserver}/.well-known/matrix/server"))
            {
                var resp = await _httpClient.GetFromJsonAsync<JsonElement>($"{homeserver}/.well-known/matrix/server");
                var hs = resp.GetProperty("m.server").GetString();
                result = hs;
            }
            else
            {
                Console.WriteLine($"No server well-known...");
                if (await _httpClient.CheckSuccessStatus($"{homeserver}/_matrix/client/versions")) result = homeserver;
                else
                {
                    Console.WriteLine("No homeserver on shortname...");
                    if (await _httpClient.CheckSuccessStatus($"{homeserver.Replace("//", "//matrix.")}/_matrix/client/versions")) result = homeserver.Replace("//", "//matrix.");
                    else Console.WriteLine($"Failed to resolve homeserver, not on {homeserver}, nor do client or server well-knowns exist!");
                }
            }
        }

        if (result != null)
        {
            Console.WriteLine($"Resolved homeserver: {homeserver} -> {result}");
            RuntimeCache.HomeserverResolutionCache.TryAdd(homeserver, new()
            {
                Result = result,
                ResolutionTime = DateTime.Now
            });
            return result;
        }
        throw new InvalidDataException($"Failed to resolve homeserver, not on {homeserver}, nor do client or server well-knowns exist!");
    }
}