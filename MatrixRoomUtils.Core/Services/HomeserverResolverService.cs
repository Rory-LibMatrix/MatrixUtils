using System.Net.Http.Json;
using System.Text.Json;
using MatrixRoomUtils.Core.Extensions;
using Microsoft.Extensions.Logging;

namespace MatrixRoomUtils.Core.Services; 

public class HomeserverResolverService {
    private readonly MatrixHttpClient _httpClient = new();
    private readonly ILogger<HomeserverResolverService> _logger;

    private static Dictionary<string, object> _wellKnownCache = new();
    
    public HomeserverResolverService(ILogger<HomeserverResolverService> logger) {
        _logger = logger;
    }

    public async Task<string> ResolveHomeserverFromWellKnown(string homeserver) {
        var res = await _resolveHomeserverFromWellKnown(homeserver);
        if (!res.StartsWith("http")) res = "https://" + res;
        if (res.EndsWith(":443")) res = res.Substring(0, res.Length - 4);
        return res;
    }

    private async Task<string> _resolveHomeserverFromWellKnown(string homeserver) {
        if(homeserver is null) throw new ArgumentNullException(nameof(homeserver));
        if (_wellKnownCache.ContainsKey(homeserver)) {
            if (_wellKnownCache[homeserver] is SemaphoreSlim s) await s.WaitAsync();
            if (_wellKnownCache[homeserver] is string p) return p;
        }
        _wellKnownCache[homeserver] = new SemaphoreSlim(1);
        string? result = null;
        _logger.LogInformation($"Attempting to resolve homeserver: {homeserver}");
        if (!homeserver.StartsWith("http")) homeserver = "https://" + homeserver;
        result ??= await _tryResolveFromClientWellknown(homeserver);
        result ??= await _tryResolveFromServerWellknown(homeserver);
        result ??= await _tryCheckIfDomainHasHomeserver(homeserver);
        // if(!homeserver.Contains("http")) homeserver = "https://" + homeserver;
        // result ??= await _tryCheckIfSubDomainHasHomeserver(homeserver, "matrix");
        // result ??= await _tryCheckIfSubDomainHasHomeserver(homeserver, "chat");

        if(result is not null) {
            _logger.LogInformation($"Resolved homeserver: {homeserver} -> {result}");
            _wellKnownCache.TryAdd(homeserver, result);
            return result;
        }
        
        throw new InvalidDataException($"Failed to resolve homeserver for {homeserver}! Is it online and configured correctly?");
    }
    
    private async Task<string?> _tryResolveFromClientWellknown(string homeserver) {
        if (!homeserver.StartsWith("http")) homeserver = "https://" + homeserver;
        if (await _httpClient.CheckSuccessStatus($"{homeserver}/.well-known/matrix/client")) {
            var resp = await _httpClient.GetFromJsonAsync<JsonElement>($"{homeserver}/.well-known/matrix/client");
            var hs = resp.GetProperty("m.homeserver").GetProperty("base_url").GetString();
            return hs;
        }
        _logger.LogInformation("No client well-known...");
        return null;
    }
    private async Task<string?> _tryResolveFromServerWellknown(string homeserver) {
        if (!homeserver.StartsWith("http")) homeserver = "https://" + homeserver;
        if (await _httpClient.CheckSuccessStatus($"{homeserver}/.well-known/matrix/server")) {
            var resp = await _httpClient.GetFromJsonAsync<JsonElement>($"{homeserver}/.well-known/matrix/server");
            var hs = resp.GetProperty("m.server").GetString();
            return hs;
        }
        _logger.LogInformation("No server well-known...");
        return null;
    }

    private async Task<string?> _tryCheckIfDomainHasHomeserver(string homeserver) {
        _logger.LogInformation($"Checking if {homeserver} hosts a homeserver...");
        if (await _httpClient.CheckSuccessStatus($"{homeserver}/_matrix/client/versions"))
            return homeserver;
        _logger.LogInformation("No homeserver on shortname...");
        return null;
    }

    private async Task<string?> _tryCheckIfSubDomainHasHomeserver(string homeserver, string subdomain) {
        homeserver = homeserver.Replace("https://", $"https://{subdomain}.");
        return await _tryCheckIfDomainHasHomeserver(homeserver);
    }
}