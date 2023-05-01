namespace MatrixRoomUtils;

public class StateEvent
{
    //example:
    /*
       {
    "content": {
      "avatar_url": "mxc://matrix.org/BnmEjNvGAkStmAoUiJtEbycT",
      "displayname": "X ⊂ Shekhinah | she/her | you",
      "membership": "join"
    },
    "origin_server_ts": 1682668449785,
    "room_id": "!wDPwzxYCNPTkHGHCFT:the-apothecary.club",
    "sender": "@kokern:matrix.org",
    "state_key": "@kokern:matrix.org",
    "type": "m.room.member",
    "unsigned": {
      "replaces_state": "$7BWfzN15LN8FFUing1hiUQWFfxnOusrEHYFNiOnNrlM",
      "prev_content": {
        "avatar_url": "mxc://matrix.org/hEQbGywixsjpxDrWvUYEFNur",
        "displayname": "X ⊂ Shekhinah | she/her | you",
        "membership": "join"
      },
      "prev_sender": "@kokern:matrix.org"
    },
    "event_id": "$6AGoMCaxqcOeIIDbez1f0VKwLkOEq3EiVLdlsoxDpNg",
    "user_id": "@kokern:matrix.org",
    "replaces_state": "$7BWfzN15LN8FFUing1hiUQWFfxnOusrEHYFNiOnNrlM",
    "prev_content": {
      "avatar_url": "mxc://matrix.org/hEQbGywixsjpxDrWvUYEFNur",
      "displayname": "X ⊂ Shekhinah | she/her | you",
      "membership": "join"
    }
  }
     */
    public dynamic content { get; set; }
    public long origin_server_ts { get; set; }
    public string room_id { get; set; }
    public string sender { get; set; }
    public string state_key { get; set; }
    public string type { get; set; }
    public dynamic unsigned { get; set; }
    public string event_id { get; set; }
    public string user_id { get; set; }
    public string replaces_state { get; set; }
    public dynamic prev_content { get; set; }
}

public class StateEvent<T> : StateEvent where T : class
{
    public T content { get; set; }
}