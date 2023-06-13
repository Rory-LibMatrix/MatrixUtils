using System.Text.Json.Serialization;

namespace MatrixRoomUtils.Core.StateEventTypes;

public class PolicyRuleStateEventData {
    /// <summary>
    ///     Entity this ban applies to, can use * and ? as globs.
    /// </summary>
    [JsonPropertyName("entity")]
    public string Entity { get; set; }

    /// <summary>
    ///     Reason this user is banned
    /// </summary>
    [JsonPropertyName("reason")]
    public string? Reason { get; set; }

    /// <summary>
    ///     Suggested action to take
    /// </summary>
    [JsonPropertyName("recommendation")]
    public string? Recommendation { get; set; }

    /// <summary>
    ///     Expiry time in milliseconds since the unix epoch, or null if the ban has no expiry.
    /// </summary>
    [JsonPropertyName("support.feline.policy.expiry.rev.2")] //stable prefix: expiry, msc pending
    public long? Expiry { get; set; }

    //utils
    /// <summary>
    ///     Readable expiry time, provided for easy interaction
    /// </summary>
    [JsonPropertyName("gay.rory.matrix_room_utils.readable_expiry_time_utc")]
    public DateTime? ExpiryDateTime {
        get => Expiry == null ? null : DateTimeOffset.FromUnixTimeMilliseconds(Expiry.Value).DateTime;
        set => Expiry = ((DateTimeOffset)value).ToUnixTimeMilliseconds();
    }
}

public static class PolicyRecommendationTypes {
    /// <summary>
    ///     Ban this user
    /// </summary>
    public static string Ban = "m.ban";

    /// <summary>
    ///     Mute this user
    /// </summary>
    public static string Mute = "support.feline.policy.recommendation_mute"; //stable prefix: m.mute, msc pending
}