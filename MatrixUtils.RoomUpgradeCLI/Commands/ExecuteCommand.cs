using System.Text.Json;
using System.Text.Json.Nodes;
using ArcaneLibs.Extensions;
using LibMatrix.Helpers;
using LibMatrix.Homeservers;

namespace MatrixUtils.RoomUpgradeCLI.Commands;

public class ExecuteCommand(ILogger<ExecuteCommand> logger, IHost host, RuntimeContext ctx, AuthenticatedHomeserverGeneric hs) : IHostedService {
    public async Task StartAsync(CancellationToken cancellationToken) {
        if (ctx.Args.Length <= 1) {
            await PrintHelp();
            return;
        }
        var filename = ctx.Args[1];
        if (filename.StartsWith("--")) {
            Console.WriteLine("Filename cannot start with --, please provide a valid filename.");
            await PrintHelp();
        }

        var rbj = await JsonSerializer.DeserializeAsync<JsonObject>(File.OpenRead(filename));
        var rb = rbj.ContainsKey(nameof(RoomUpgradeBuilder.OldRoomId))
            ? rbj.Deserialize<RoomUpgradeBuilder>()
            : rbj.Deserialize<RoomBuilder>();
        Console.WriteLine($"Executing room builder file of type {rb.GetType().Name}...");
        await rb!.Create(hs);

        await host.StopAsync(cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken) { }

    private async Task PrintHelp() {
        Console.WriteLine("Usage: execute [filename]");
        Console.WriteLine("Options:");
        Console.WriteLine("  --help     Show this help message");
        await host.StopAsync();
    }
}