namespace MatrixRoomUtils.Core;

public struct StateEventStruct
{
    public object content { get; set; }
    public long origin_server_ts { get; set; }
    public string sender { get; set; }
    public string state_key { get; set; }
    public string type { get; set; }
    public string event_id { get; set; }
    public string user_id { get; set; }
    public string replaces_state { get; set; }
}