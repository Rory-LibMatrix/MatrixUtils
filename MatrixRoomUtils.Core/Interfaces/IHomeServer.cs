using System.Net.Http.Json;
using System.Text.Json;
using MatrixRoomUtils.Core.Extensions;
using MatrixRoomUtils.Core.StateEventTypes;

namespace MatrixRoomUtils.Core.Interfaces;

public class IHomeServer {
    private readonly Dictionary<string, ProfileResponse?> _profileCache = new();
    public string HomeServerDomain { get; set; }
    public string FullHomeServerDomain { get; set; }

    protected internal MatrixHttpClient _httpClient { get; set; } = new();


    public async Task<ProfileResponse> GetProfile(string mxid, bool debounce = false, bool cache = true) {
        if (cache) {
            if (debounce) await Task.Delay(Random.Shared.Next(100, 500));
            if (_profileCache.ContainsKey(mxid)) {
                while (_profileCache[mxid] == null) {
                    Console.WriteLine($"Waiting for profile cache for {mxid}, currently {_profileCache[mxid]?.ToJson() ?? "null"} within {_profileCache.Count} profiles...");
                    await Task.Delay(Random.Shared.Next(50, 500));
                }

                return _profileCache[mxid];
            }
        }

        _profileCache.Add(mxid, null);
        var resp = await _httpClient.GetAsync($"/_matrix/client/v3/profile/{mxid}");
        var data = await resp.Content.ReadFromJsonAsync<JsonElement>();
        if (!resp.IsSuccessStatusCode) Console.WriteLine("Profile: " + data);
        var profile = data.Deserialize<ProfileResponse>();
        _profileCache[mxid] = profile;
        return profile;
    }

    public string? ResolveMediaUri(string mxc) => mxc.Replace("mxc://", $"{FullHomeServerDomain}/_matrix/media/v3/download/");
}