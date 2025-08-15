using LibMatrix.Homeservers;

namespace MatrixUtils.RoomUpgradeCLI.Commands;

public class DevDeleteRoomCommand(ILogger<DevDeleteRoomCommand> logger, IHost host, RuntimeContext ctx, AuthenticatedHomeserverGeneric hs) : IHostedService {
    public async Task StartAsync(CancellationToken cancellationToken) {
        var synapse = hs as AuthenticatedHomeserverSynapse;
        if (ctx.Args.Length == 2) {
            var room = synapse.GetRoom(ctx.Args[1]);
            await synapse.Admin.DeleteRoom(room.RoomId, new() { Purge = true });
        }
        else {
            string line;
            do {
                line = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(line)) continue;
                var room = synapse.GetRoom(line);
                await synapse.Admin.DeleteRoom(room.RoomId, new() { Purge = true });
            } while (line is not null);
        }

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