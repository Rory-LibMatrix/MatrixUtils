using MatrixRoomUtils.Authentication;

namespace MatrixRoomUtils.Extensions;

public static class StringExtensions
{
    public static async Task<string> GetMediaUrl(this string MxcUrl)
    {
        //MxcUrl: mxc://rory.gay/ocRVanZoUTCcifcVNwXgbtTg
        //target: https://matrix.rory.gay/_matrix/media/v3/download/rory.gay/ocRVanZoUTCcifcVNwXgbtTg
        
        var server = MxcUrl.Split('/')[2];
        var mediaId = MxcUrl.Split('/')[3];
        return $"{await MatrixAccount.ResolveHomeserverFromWellKnown(server)}/_matrix/media/v3/download/{server}/{mediaId}";
    }
    
}