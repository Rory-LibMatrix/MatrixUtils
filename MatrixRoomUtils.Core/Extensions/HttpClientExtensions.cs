namespace MatrixRoomUtils.Core.Extensions;

public static class HttpClientExtensions
{
    public static async Task<bool> CheckSuccessStatus(this HttpClient hc, string url)
    {
        //cors causes failure, try to catch
        try
        {
            var resp = await hc.GetAsync(url);
            return resp.IsSuccessStatusCode;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Failed to check success status: {e.Message}");
            return false;
        }
    }
}