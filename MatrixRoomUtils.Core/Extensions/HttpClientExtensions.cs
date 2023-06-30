using System.Reflection;
using System.Text.Json;

namespace MatrixRoomUtils.Core.Extensions;

public static class HttpClientExtensions {
    public static async Task<bool> CheckSuccessStatus(this HttpClient hc, string url) {
        //cors causes failure, try to catch
        try {
            var resp = await hc.GetAsync(url);
            return resp.IsSuccessStatusCode;
        }
        catch (Exception e) {
            Console.WriteLine($"Failed to check success status: {e.Message}");
            return false;
        }
    }
}

public class MatrixHttpClient : HttpClient {
    public override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) {
        try
        {
            HttpRequestOptionsKey<bool> WebAssemblyEnableStreamingResponseKey = new HttpRequestOptionsKey<bool>("WebAssemblyEnableStreamingResponse");
            request.Options.Set(WebAssemblyEnableStreamingResponseKey, true);
            // var asm = Assembly.Load("Microsoft.AspNetCore.Components.WebAssembly");
            // var browserHttpHandlerType = asm.GetType("Microsoft.AspNetCore.Components.WebAssembly.Http.WebAssemblyHttpRequestMessageExtensions", true);
            // var browserHttpHandlerMethod = browserHttpHandlerType.GetMethod("SetBrowserResponseStreamingEnabled", BindingFlags.Public | BindingFlags.Static);
            // browserHttpHandlerMethod?.Invoke(null, new object[] {request, true});
        }
        catch (Exception e)
        {
            Console.WriteLine("Failed to set browser response streaming:");
            Console.WriteLine(e);
        }
        var a = await base.SendAsync(request, cancellationToken);
        if (!a.IsSuccessStatusCode) {
            Console.WriteLine($"Failed to send request: {a.StatusCode}");
            var content = await a.Content.ReadAsStringAsync(cancellationToken);
            if (content.StartsWith('{')) {
                var ex = JsonSerializer.Deserialize<MatrixException>(content);
                if (ex?.RetryAfterMs is not null) {
                    await Task.Delay(ex.RetryAfterMs.Value, cancellationToken);
                    typeof(HttpRequestMessage).GetField("_sendStatus", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(request, 0);
                    return await SendAsync(request, cancellationToken);
                }
                throw ex!;
            }
            throw new InvalidDataException("Encountered invalid data:\n" + content);
        }
        return a;
    }
}