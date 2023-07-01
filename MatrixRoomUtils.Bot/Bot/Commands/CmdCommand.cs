using MatrixRoomUtils.Bot.Bot.Interfaces;

namespace MatrixRoomUtils.Bot.Bot.Commands;

public class CmdCommand : ICommand {
    public string Name { get; } = "cmd";
    public string Description { get; } = "Runs a command on the host system";

    public async Task<bool> CanInvoke(CommandContext ctx) {
        return ctx.MessageEvent.Sender.EndsWith(":rory.gay");
    }

    public async Task Invoke(CommandContext ctx) {
        var cmd = "\"";
        foreach (var arg in ctx.Args) cmd += arg + " ";

        cmd = cmd.Trim();
        cmd += "\"";

        await ctx.Room.SendMessageEventAsync("m.room.message", new() {
            Body = $"Command being executed: `{cmd}`"
        });

        var output = ArcaneLibs.Util.GetCommandOutputSync(
                Environment.OSVersion.Platform == PlatformID.Unix ? "/bin/sh" : "cmd.exe",
                (Environment.OSVersion.Platform == PlatformID.Unix ? "-c " : "/c ") + cmd)
            .Replace("`", "\\`")
            .Split("\n").ToList();
        foreach (var _out in output) Console.WriteLine($"{_out.Length:0000} {_out}");

        var msg = "";
        while (output.Count > 0) {
            Console.WriteLine("Adding: " + output[0]);
            msg += output[0] + "\n";
            output.RemoveAt(0);
            if ((output.Count > 0 && (msg + output[0]).Length > 64000) || output.Count == 0) {
                await ctx.Room.SendMessageEventAsync("m.room.message", new() {
                    FormattedBody = $"```ansi\n{msg}\n```",
                    // Body = Markdig.Markdown.ToHtml(msg),
                    Format = "org.matrix.custom.html"
                });
                msg = "";
            }
        }
    }
}