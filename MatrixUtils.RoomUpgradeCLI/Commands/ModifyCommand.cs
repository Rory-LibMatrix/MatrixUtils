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
        Console.WriteLine("  --help                                             Show this help message");
        Console.WriteLine("  --version <version>                                Set the room version (e.g. 9, 10, 11, 12)");
        Console.WriteLine("-- New room options --");
        Console.WriteLine("  --alias <alias>                                    Set the room alias (local part)");
        Console.WriteLine("  --avatar-url <url>                                 Set the room avatar URL");
        Console.WriteLine("  --copy-avatar <roomId>                             Copy the avatar from an existing room");
        Console.WriteLine("  --copy-powerlevels <roomId>                        Copy power levels from an existing room");
        Console.WriteLine("  --invite-admin <userId>                            Invite a user as an admin (userId must start with '@')");
        Console.WriteLine("  --invite <userId>                                  Invite a user (userId must start with '@')");
        Console.WriteLine("  --name <name>                                      Set the room name (can be multiple words)");
        Console.WriteLine("  --topic <topic>                                    Set the room topic (can be multiple words)");
        Console.WriteLine("  --federate <true|false>                            Set whether the room is federatable");
        Console.WriteLine("  --public                                           Set the room join rule to public");
        Console.WriteLine("  --invite-only                                      Set the room join rule to invite-only");
        Console.WriteLine("  --knock                                            Set the room join rule to knock");
        Console.WriteLine("  --restricted                                       Set the room join rule to restricted");
        Console.WriteLine("  --knock_restricted                                 Set the room join rule to knock_restricted");
        Console.WriteLine("  --private                                          Set the room join rule to private");
        Console.WriteLine("  --join-rule <rule>                                 Set the room join rule (public, invite, knock, restricted, knock_restricted, private)");
        Console.WriteLine("  --history-visibility <visibility>                  Set the room history visibility (shared, invited, joined, world_readable)");
        Console.WriteLine("  --type <type>                                      Set the room type (e.g. m.space, m.room, support.feline.policy.list.msc.v1 etc.)");
        // upgrade opts
        Console.WriteLine("-- Upgrade options --");
        Console.WriteLine(
            "  --upgrade <roomId>                                 Create a room upgrade file instead of a new room file - WARNING: incompatible with non-upgrade options");
        Console.WriteLine("  --invite-members                                   Invite members during room upgrade");
        Console.WriteLine("  --invite-powerlevel-users                          Invite users with power levels during room upgrade");
        Console.WriteLine("  --migrate-bans                                     Migrate bans during room upgrade");
        Console.WriteLine("  --migrate-empty-state-events                       Migrate empty state events during room upgrade");
        Console.WriteLine("  --upgrade-unstable-values                          Upgrade unstable values during room upgrade");
        Console.WriteLine("  --msc4321-policy-list-upgrade <move|transition>    Upgrade MSC4321 policy list");
        Console.WriteLine("  --force-upgrade                                    Force upgrade even if you don't have the required permissions");
        Console.WriteLine(
            "WARNING: The --upgrade option is incompatible with options listed under \"New room\", please use the equivalent options in the `modify` command instead.");
        await host.StopAsync();
    }
}