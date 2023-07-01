using System.Text.Json.Serialization;
using MatrixRoomUtils.Core.Extensions;

namespace MatrixRoomUtils.Core;

public class MatrixException : Exception {
    [JsonPropertyName("errcode")]
    public string ErrorCode { get; set; }

    [JsonPropertyName("error")]
    public string Error { get; set; }

    [JsonPropertyName("soft_logout")]
    public bool? SoftLogout { get; set; }

    [JsonPropertyName("retry_after_ms")]
    public int? RetryAfterMs { get; set; }

    public override string Message =>
        $"{ErrorCode}: {ErrorCode switch {
            // common
            "M_FORBIDDEN" => $"You do not have permission to perform this action: {Error}",
            "M_UNKNOWN_TOKEN" => $"The access token specified was not recognised: {Error}{(SoftLogout == true ? " (soft logout)" : "")}",
            "M_MISSING_TOKEN" => $"No access token was specified: {Error}",
            "M_BAD_JSON" => $"Request contained valid JSON, but it was malformed in some way: {Error}",
            "M_NOT_JSON" => $"Request did not contain valid JSON: {Error}",
            "M_NOT_FOUND" => $"The requested resource was not found: {Error}",
            "M_LIMIT_EXCEEDED" => $"Too many requests have been sent in a short period of time. Wait a while then try again: {Error}",
            "M_UNRECOGNISED" => $"The server did not recognise the request: {Error}",
            "M_UNKOWN" => $"The server encountered an unexpected error: {Error}",
            // endpoint specific
            "M_UNAUTHORIZED" => $"The request did not contain valid authentication information for the target of the request: {Error}",
            "M_USER_DEACTIVATED" => $"The user ID associated with the request has been deactivated: {Error}",
            "M_USER_IN_USE" => $"The user ID associated with the request is already in use: {Error}",
            "M_INVALID_USERNAME" => $"The requested user ID is not valid: {Error}",
            "M_ROOM_IN_USE" => $"The room alias requested is already taken: {Error}",
            "M_INVALID_ROOM_STATE" => $"The room associated with the request is not in a valid state to perform the request: {Error}",
            "M_THREEPID_IN_USE" => $"The threepid requested is already associated with a user ID on this server: {Error}",
            "M_THREEPID_NOT_FOUND" => $"The threepid requested is not associated with any user ID: {Error}",
            "M_THREEPID_AUTH_FAILED" => $"The provided threepid and/or token was invalid: {Error}",
            "M_THREEPID_DENIED" => $"The homeserver does not permit the third party identifier in question: {Error}",
            "M_SERVER_NOT_TRUSTED" => $"The homeserver does not trust the identity server: {Error}",
            "M_UNSUPPORTED_ROOM_VERSION" => $"The room version is not supported: {Error}",
            "M_INCOMPATIBLE_ROOM_VERSION" => $"The room version is incompatible: {Error}",
            "M_BAD_STATE" => $"The request was invalid because the state was invalid: {Error}",
            "M_GUEST_ACCESS_FORBIDDEN" => $"Guest access is forbidden: {Error}",
            "M_CAPTCHA_NEEDED" => $"Captcha needed: {Error}",
            "M_CAPTCHA_INVALID" => $"Captcha invalid: {Error}",
            "M_MISSING_PARAM" => $"Missing parameter: {Error}",
            "M_INVALID_PARAM" => $"Invalid parameter: {Error}",
            "M_TOO_LARGE" => $"The request or entity was too large: {Error}",
            "M_EXCLUSIVE" => $"The resource being requested is reserved by an application service, or the application service making the request has not created the resource: {Error}",
            "M_RESOURCE_LIMIT_EXCEEDED" => $"Exceeded resource limit: {Error}",
            "M_CANNOT_LEAVE_SERVER_NOTICE_ROOM" => $"Cannot leave server notice room: {Error}",
            _ => $"Unknown error: {new { ErrorCode, Error, SoftLogout, RetryAfterMs }.ToJson(ignoreNull: true)}"
        }}";
}