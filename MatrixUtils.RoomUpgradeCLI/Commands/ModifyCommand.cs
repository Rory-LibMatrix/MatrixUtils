using System.Text.Json;
using ArcaneLibs.Extensions;
using LibMatrix.Helpers;
using LibMatrix.Homeservers;
using MatrixUtils.RoomUpgradeCLI.Extensions;

namespace MatrixUtils.RoomUpgradeCLI.Commands;

public class ModifyCommand(ILogger<ModifyCommand> logger, IHost host, RuntimeContext ctx, AuthenticatedHomeserverGeneric hs) : IHostedService {
    public async Task StartAsync(CancellationToken cancellationToken) {
        if (ctx.Args.Length <= 2 || ctx.Args.Contains("--help")) {
            await PrintHelp();
            return;
        }

        var filename = ctx.Args[1];
        if (filename.StartsWith("--")) {
            Console.WriteLine("Filename cannot start with --, please provide a valid filename.");
            await PrintHelp();
        }

        var rb = ctx.Args.Contains("--upgrade")
            ? await JsonSerializer.DeserializeAsync<RoomUpgradeBuilder>(File.OpenRead(filename), cancellationToken: cancellationToken)
            : await JsonSerializer.DeserializeAsync<RoomBuilder>(File.OpenRead(filename), cancellationToken: cancellationToken);
        await rb!.ApplyRoomUpgradeCLIArgs(hs, ctx.Args[2..], isNewState: false);
        await File.WriteAllTextAsync(filename, rb.ToJson(), cancellationToken);

        await host.StopAsync(cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken) { }

    private async Task PrintHelp() {
        Console.WriteLine("Usage: new [filename] [options]");
        Console.WriteLine("Options:");

        await host.StopAsync();
    }
}