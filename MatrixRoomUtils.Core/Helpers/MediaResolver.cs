namespace MatrixRoomUtils.Core.Helpers; 

public class MediaResolver {
    public static string ResolveMediaUri(string homeserver, string mxc) => 
        mxc.Replace("mxc://", $"{homeserver}/_matrix/media/v3/download/");
}