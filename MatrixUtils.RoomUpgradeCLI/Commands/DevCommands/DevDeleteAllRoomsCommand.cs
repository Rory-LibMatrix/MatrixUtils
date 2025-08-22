using LibMatrix.Homeservers;

namespace MatrixUtils.RoomUpgradeCLI.Commands;

public class DevDeleteAllRoomsCommand(ILogger<DevDeleteAllRoomsCommand> logger, IHost host, RuntimeContext ctx, AuthenticatedHomeserverGeneric hs) : IHostedService {
    public async Task StartAsync(CancellationToken cancellationToken) {
        var synapse = hs as AuthenticatedHomeserverSynapse;
        await foreach (var room in synapse.Admin.SearchRoomsAsync())
        {
            try
            {
                await synapse.Admin.DeleteRoom(room.RoomId, new() { ForcePurge = true });
                Console.WriteLine($"Deleted room: {room.RoomId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to delete room {room.RoomId}: {ex.Message}");
            }
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