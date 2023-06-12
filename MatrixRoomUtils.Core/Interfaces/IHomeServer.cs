using System.Net.Http.Json;
using System.Text.Json;
using MatrixRoomUtils.Core.Extensions;
using MatrixRoomUtils.Core.Responses;

namespace MatrixRoomUtils.Core.Interfaces;

public class IHomeServer
{
    private Dictionary<string, ProfileResponse?> _profileCache = new();
    public string HomeServerDomain { get; set; }
    public string FullHomeServerDomain { get; set; }

    private protected HttpClient _httpClient { get; set; } = new();

    public async Task<string> ResolveHomeserverFromWellKnown(string homeserver)
    {
        var res = await _resolveHomeserverFromWellKnown(homeserver);
        if(!res.StartsWith("http")) res = "https://" + res;
        if(res.EndsWith(":443")) res = res.Substring(0, res.Length - 4);
        return res;
    }
    private async Task<string> _resolveHomeserverFromWellKnown(string homeserver)
    {
        if (RuntimeCache.HomeserverResolutionCache.Count == 0)
        {
            Console.WriteLine("No cached homeservers, resolving...");
            await Task.Delay(Random.Shared.Next(1000, 5000));
        } 
        if (RuntimeCache.HomeserverResolutionCache.ContainsKey(homeserver))
        {
            if (RuntimeCache.HomeserverResolutionCache[homeserver].ResolutionTime < DateTime.Now.AddHours(1))
            {
                Console.WriteLine($"Found cached homeserver: {RuntimeCache.HomeserverResolutionCache[homeserver].Result}");
                return RuntimeCache.HomeserverResolutionCache[homeserver].Result;
            }
            Console.WriteLine($"Cached homeserver expired, removing: {RuntimeCache.HomeserverResolutionCache[homeserver].Result}");
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
    public async Task<ProfileResponse> GetProfile(string mxid, bool debounce = false, bool cache = true)
    {
        if (cache)
        {
            if(debounce) await Task.Delay(Random.Shared.Next(100, 500));
            if (_profileCache.ContainsKey(mxid))
            {
                while (_profileCache[mxid] == null)
                {
                    Console.WriteLine($"Waiting for profile cache for {mxid}, currently {_profileCache[mxid]?.ToJson() ?? "null"} within {_profileCache.Count} profiles...");
                    await Task.Delay(Random.Shared.Next(50, 500));
                }
                return _profileCache[mxid];
            }
        }

        _profileCache.Add(mxid, null);
        var resp = await _httpClient.GetAsync($"/_matrix/client/r0/profile/{mxid}");
        var data = await resp.Content.ReadFromJsonAsync<JsonElement>();
        if(!resp.IsSuccessStatusCode) Console.WriteLine("Profile: " + data.ToString());
        var profile = data.Deserialize<ProfileResponse>();
        _profileCache[mxid] = profile;
        return profile;
    }
    public string? ResolveMediaUri(string mxc)
    {
        return mxc.Replace("mxc://", $"{FullHomeServerDomain}/_matrix/media/r0/download/");
    }
}