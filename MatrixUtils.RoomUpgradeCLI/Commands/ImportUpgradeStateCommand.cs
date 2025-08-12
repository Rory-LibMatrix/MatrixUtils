using System.Text.Json;
using ArcaneLibs.Extensions;
using LibMatrix.Helpers;
using LibMatrix.Homeservers;

namespace MatrixUtils.RoomUpgradeCLI.Commands;

public class ImportUpgradeStateCommand(ILogger<ImportUpgradeStateCommand> logger, IHost host, RuntimeContext ctx, AuthenticatedHomeserverGeneric hs) : IHostedService {
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

        var rb = await JsonSerializer.DeserializeAsync<RoomUpgradeBuilder>(File.OpenRead(filename));
        await rb!.ImportAsync(hs.GetRoom(rb.OldRoomId));
        await File.WriteAllTextAsync(filename, rb.ToJson(), cancellationToken);

        await host.StopAsync(cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken) { }

    private async Task PrintHelp() {
        Console.WriteLine("Usage: import-upgrade-state [filename]");
        Console.WriteLine("Options:");
        Console.WriteLine("  --help     Show this help message");
        await host.StopAsync();
    }
}