using Microsoft.Extensions.Configuration;

namespace ModerationBot;

public class ModerationBotConfiguration {
    public ModerationBotConfiguration(IConfiguration config) => config.GetRequiredSection("ModerationBot").Bind(this);

    public string Homeserver { get; set; }
    public string AccessToken { get; set; }
}
