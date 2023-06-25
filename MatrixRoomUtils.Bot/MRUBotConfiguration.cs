using Microsoft.Extensions.Configuration;

namespace MatrixRoomUtils.Bot; 

public class MRUBotConfiguration {
    public MRUBotConfiguration(IConfiguration config) {
        config.GetRequiredSection("Bot").Bind(this);
    }
    public string Homeserver { get; set; } = "";
    public string AccessToken { get; set; } = "";
}