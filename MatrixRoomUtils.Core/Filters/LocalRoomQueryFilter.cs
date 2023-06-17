namespace MatrixRoomUtils.Core.Filters;

public class LocalRoomQueryFilter {
    public string RoomIdContains { get; set; } = "";
    public string NameContains { get; set; } = "";
    public string CanonicalAliasContains { get; set; } = "";
    public string VersionContains { get; set; } = "";
    public string CreatorContains { get; set; } = "";
    public string EncryptionContains { get; set; } = "";
    public string JoinRulesContains { get; set; } = "";
    public string GuestAccessContains { get; set; } = "";
    public string HistoryVisibilityContains { get; set; } = "";
    
    public bool Federatable { get; set; } = true;
    public bool Public { get; set; } = true;
    
    public int JoinedMembersGreaterThan { get; set; } = 0;
    public int JoinedMembersLessThan { get; set; } = int.MaxValue;

    public int JoinedLocalMembersGreaterThan { get; set; } = 0;
    public int JoinedLocalMembersLessThan { get; set; } = int.MaxValue;
    public int StateEventsGreaterThan { get; set; } = 0;
    public int StateEventsLessThan { get; set; } = int.MaxValue;

    
    public bool CheckFederation { get; set; }
    public bool CheckPublic { get; set; }
}