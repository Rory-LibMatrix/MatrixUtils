using System.Net.Http.Json;
using System.Text.Json;
using MatrixRoomUtils.Core.Extensions;
using MatrixRoomUtils.Core.Interfaces;

namespace MatrixRoomUtils.Core;

public class RemoteHomeServer : IHomeServer {
    public RemoteHomeServer(string canonicalHomeServerDomain) {
        HomeServerDomain = canonicalHomeServerDomain;
        _httpClient = new MatrixHttpClient();
        _httpClient.Timeout = TimeSpan.FromSeconds(5);
    }

}