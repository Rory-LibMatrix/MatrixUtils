using System.Web;
using LibMatrix.Homeservers;

namespace MatrixUtils.RoomUpgradeCLI.Commands;

public class DevGetRoomDirStateCommand(ILogger<DevGetRoomDirStateCommand> logger, IHost host, RuntimeContext ctx, AuthenticatedHomeserverGeneric hs) : IHostedService {
    public async Task StartAsync(CancellationToken cancellationToken) {
        var synapse = hs as AuthenticatedHomeserverSynapse;
        if (ctx.Args.Length == 2) {
            var res = await hs.ClientHttpClient.GetAsync(" /_matrix/client/v3/directory/list/room/" + HttpUtility.UrlEncode(ctx.Args[1]));
            if (res.IsSuccessStatusCode) {
                var data = await res.Content.ReadAsStringAsync();
                Console.WriteLine("Room Directory State for " + ctx.Args[1] + ":");
                Console.WriteLine(data);
            } else {
                Console.WriteLine("Failed to get room directory state for " + ctx.Args[1] + ": " + res.ReasonPhrase);
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