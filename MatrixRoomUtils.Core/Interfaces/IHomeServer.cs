using System.Net.Http.Json;
using System.Text.Json;
using MatrixRoomUtils.Core.Extensions;
using MatrixRoomUtils.Core.StateEventTypes;

namespace MatrixRoomUtils.Core.Interfaces;

public class IHomeServer {
    private readonly Dictionary<string, object> _profileCache = new();
    public string HomeServerDomain { get; set; }
    public string FullHomeServerDomain { get; set; }

    protected internal MatrixHttpClient _httpClient { get; set; } = new();

    public async Task<ProfileResponse> GetProfile(string mxid, bool debounce = false, bool cache = true) {
        // if (cache) {
        //     if (debounce) await Task.Delay(Random.Shared.Next(100, 500));
        //     if (_profileCache.ContainsKey(mxid)) {
        //         while (_profileCache[mxid] == null) {
        //             Console.WriteLine($"Waiting for profile cache for {mxid}, currently {_profileCache[mxid]?.ToJson() ?? "null"} within {_profileCache.Count} profiles...");
        //             await Task.Delay(Random.Shared.Next(50, 500));
        //         }
        //
        //         return _profileCache[mxid];
        //     }
        // }
        if(mxid is null) throw new ArgumentNullException(nameof(mxid));
        if (_profileCache.ContainsKey(mxid)) {
            if (_profileCache[mxid] is SemaphoreSlim s) await s.WaitAsync();
            if (_profileCache[mxid] is ProfileResponse p) return p;
        }
        _profileCache[mxid] = new SemaphoreSlim(1);
        
        var resp = await _httpClient.GetAsync($"/_matrix/client/v3/profile/{mxid}");
        var data = await resp.Content.ReadFromJsonAsync<ProfileResponse>();
        if (!resp.IsSuccessStatusCode) Console.WriteLine("Profile: " + data);
        _profileCache[mxid] = data;
        
        return data;
    }
}