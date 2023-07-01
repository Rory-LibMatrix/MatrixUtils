using MatrixRoomUtils.Bot.Bot.Interfaces;

namespace MatrixRoomUtils.Bot.Bot.Commands; 

public class PingCommand : ICommand {
    public PingCommand() {
    }

    public string Name { get; } = "ping";
    public string Description { get; } = "Pong!";

    public async Task Invoke(CommandContext ctx) {
        await ctx.Room.SendMessageEventAsync("m.room.message", new() {
            Body = "pong!"
        });
    }
}