using LibMatrix.Responses;

namespace MatrixUtils.Web.Classes;

public class UserAuth : LoginResponse {
    public UserAuth() { }
    public UserAuth(LoginResponse login) {
        Homeserver = login.Homeserver;
        UserId = login.UserId;
        AccessToken = login.AccessToken;
        DeviceId = login.DeviceId;
    }

    public string? Proxy { get; set; }
}
