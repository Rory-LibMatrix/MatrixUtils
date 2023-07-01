using System.Text;
using MatrixRoomUtils.Bot.Bot.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace MatrixRoomUtils.Bot.Bot.Commands; 

public class HelpCommand : ICommand {
    private readonly IServiceProvider _services;
    public HelpCommand(IServiceProvider services) {
        _services = services;
    }

    public string Name { get; } = "help";
    public string Description { get; } = "Displays this help message";

    public async Task Invoke(CommandContext ctx) {
        var sb = new StringBuilder();
        sb.AppendLine("Available commands:");
        var commands = _services.GetServices<ICommand>().ToList();
        foreach (var command in commands) {
            sb.AppendLine($"- {command.Name}: {command.Description}");
        }

        await ctx.Room.SendMessageEventAsync("m.room.message", new() {
            Body = sb.ToString(),
        });
    }
}