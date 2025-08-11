using ArcaneLibs.Extensions;
using LibMatrix.EventTypes.Spec.State.RoomInfo;
using LibMatrix.Helpers;
using LibMatrix.Homeservers;
using LibMatrix.RoomTypes;
using LibMatrix.Utilities.Bot.Interfaces;

namespace MatrixUtils.RoomUpgradeCLI.Commands;

public class NewFileCommand(ILogger<NewFileCommand> logger, IHost host, RuntimeContext ctx, AuthenticatedHomeserverGeneric hs) : IHostedService {
    public async Task StartAsync(CancellationToken cancellationToken) {
        var rb = ctx.Args.Contains("--upgrade") ? new RoomUpgradeBuilder() : new RoomBuilder();
        if (ctx.Args.Length <= 1) {
            await PrintHelp();
            return;
        }
        var filename = ctx.Args[1];
        if (filename.StartsWith("--")) {
            Console.WriteLine("Filename cannot start with --, please provide a valid filename.");
            await PrintHelp();
        }
        for (int i = 2; i < ctx.Args.Length; i++) {
            switch (ctx.Args[i]) {
                case "--alias":
                    rb.AliasLocalPart = ctx.Args[++i];
                    break;
                case "--avatar-url":
                    rb.Avatar!.Url = ctx.Args[++i];
                    break;
                case "--copy-avatar": {
                    var room = hs.GetRoom(ctx.Args[++i]);
                    rb.Avatar = await room.GetAvatarUrlAsync() ?? throw new ArgumentException($"Room {room.RoomId} does not have an avatar");
                    break;
                }
                case "--copy-powerlevels": {
                    var room = hs.GetRoom(ctx.Args[++i]);
                    rb.PowerLevels = await room.GetPowerLevelsAsync() ?? throw new ArgumentException($"Room {room.RoomId} does not have power levels???");
                    break;
                }
                case "--invite-admin":
                    var inviteAdmin = ctx.Args[++i];
                    if (!inviteAdmin.StartsWith('@')) {
                        throw new ArgumentException("Invalid user reference: " + inviteAdmin);
                    }

                    rb.Invites.Add(inviteAdmin, "Marked explicitly as admin to be invited");
                    break;
                case "--invite":
                    var inviteUser = ctx.Args[++i];
                    if (!inviteUser.StartsWith('@')) {
                        throw new ArgumentException("Invalid user reference: " + inviteUser);
                    }

                    rb.Invites.Add(inviteUser, "Marked explicitly to be invited");
                    break;
                case "--name":
                    var nameEvt = rb.Name = new() { Name = "" };
                    while (i + 1 < ctx.Args.Length && !ctx.Args[i + 1].StartsWith("--")) {
                        nameEvt.Name += (nameEvt.Name.Length > 0 ? " " : "") + ctx.Args[++i];
                    }

                    break;
                case "--topic":
                    var topicEvt = rb.Topic = new() { Topic = "" };
                    while (i + 1 < ctx.Args.Length && !ctx.Args[i + 1].StartsWith("--")) {
                        topicEvt.Topic += (topicEvt.Topic.Length > 0 ? " " : "") + ctx.Args[++i];
                    }

                    break;
                case "--federate":
                    rb.IsFederatable = bool.Parse(ctx.Args[++i]);
                    break;
                case "--public":
                case "--invite-only":
                case "--knock":
                case "--restricted":
                case "--knock_restricted":
                case "--private":
                    rb.JoinRules.JoinRule = ctx.Args[i].Replace("--", "").ToLowerInvariant() switch {
                        "public" => RoomJoinRulesEventContent.JoinRules.Public,
                        "invite-only" => RoomJoinRulesEventContent.JoinRules.Invite,
                        "knock" => RoomJoinRulesEventContent.JoinRules.Knock,
                        "restricted" => RoomJoinRulesEventContent.JoinRules.Restricted,
                        "knock_restricted" => RoomJoinRulesEventContent.JoinRules.KnockRestricted,
                        "private" => RoomJoinRulesEventContent.JoinRules.Private,
                        _ => throw new ArgumentException("Unknown join rule: " + ctx.Args[i])
                    };
                    break;
                case "--join-rule":
                    if (i + 1 >= ctx.Args.Length || !ctx.Args[i + 1].StartsWith("--")) {
                        throw new ArgumentException("Expected join rule after --join-rule");
                    }

                    rb.JoinRules.JoinRule = ctx.Args[++i].ToLowerInvariant() switch {
                        "public" => RoomJoinRulesEventContent.JoinRules.Public,
                        "invite" => RoomJoinRulesEventContent.JoinRules.Invite,
                        "knock" => RoomJoinRulesEventContent.JoinRules.Knock,
                        "restricted" => RoomJoinRulesEventContent.JoinRules.Restricted,
                        "knock_restricted" => RoomJoinRulesEventContent.JoinRules.KnockRestricted,
                        "private" => RoomJoinRulesEventContent.JoinRules.Private,
                        _ => throw new ArgumentException("Unknown join rule: " + ctx.Args[i])
                    };
                    break;
                case "--history-visibility":
                    rb.HistoryVisibility = new RoomHistoryVisibilityEventContent {
                        HistoryVisibility = ctx.Args[++i].ToLowerInvariant() switch {
                            "shared" => RoomHistoryVisibilityEventContent.HistoryVisibilityTypes.Shared,
                            "invited" => RoomHistoryVisibilityEventContent.HistoryVisibilityTypes.Invited,
                            "joined" => RoomHistoryVisibilityEventContent.HistoryVisibilityTypes.Joined,
                            "world_readable" => RoomHistoryVisibilityEventContent.HistoryVisibilityTypes.WorldReadable,
                            _ => throw new ArgumentException("Unknown history visibility: " + ctx.Args[i])
                        }
                    };
                    break;
                case "--type":
                    rb.Type = ctx.Args[++i];
                    break;
                case "--version":
                    rb.Version = ctx.Args[++i];
                    // if (!RoomBuilder.V12PlusRoomVersions.Contains(rb.Version)) {
                        // logger.LogWarning("Using room version {Version} which is not v12 or higher, this may cause issues with some features.", rb.Version);
                    // }
                    break;
                // upgrade options
                case "--invite-members":
                    if (rb is not RoomUpgradeBuilder upgradeBuilder) {
                        throw new InvalidOperationException("Invite members can only be used with room upgrades");
                    }

                    upgradeBuilder.UpgradeOptions.InviteMembers = true;
                    break;
                case "--invite-powerlevel-users":
                    if (rb is not RoomUpgradeBuilder upgradeBuilderInvite) {
                        throw new InvalidOperationException("Invite powerlevel users can only be used with room upgrades");
                    }

                    upgradeBuilderInvite.UpgradeOptions.InvitePowerlevelUsers = true;
                    break;
                case "--migrate-bans":
                    if (rb is not RoomUpgradeBuilder upgradeBuilderBan) {
                        throw new InvalidOperationException("Migrate bans can only be used with room upgrades");
                    }

                    upgradeBuilderBan.UpgradeOptions.MigrateBans = true;
                    break;
                case "--migrate-empty-state-events":
                    if (rb is not RoomUpgradeBuilder upgradeBuilderEmpty) {
                        throw new InvalidOperationException("Migrate empty state events can only be used with room upgrades");
                    }

                    upgradeBuilderEmpty.UpgradeOptions.MigrateEmptyStateEvents = true;
                    break;
                case "--upgrade-unstable-values":
                    if (rb is not RoomUpgradeBuilder upgradeBuilderUnstable) {
                        throw new InvalidOperationException("Update unstable values can only be used with room upgrades");
                    }

                    upgradeBuilderUnstable.UpgradeOptions.UpgradeUnstableValues = true;
                    break;
                case "--msc4321-policy-list-upgrade":
                    if (rb is not RoomUpgradeBuilder upgradeBuilderPolicy) {
                        throw new InvalidOperationException("MSC4321 policy list upgrade can only be used with room upgrades");
                    }

                    upgradeBuilderPolicy.UpgradeOptions.Msc4321PolicyListUpgradeOptions.Enable = true;
                    upgradeBuilderPolicy.UpgradeOptions.Msc4321PolicyListUpgradeOptions.UpgradeType = ctx.Args[++i].ToLowerInvariant() switch {
                        "move" => RoomUpgradeBuilder.Msc4321PolicyListUpgradeOptions.Msc4321PolicyListUpgradeType.Move,
                        "transition" => RoomUpgradeBuilder.Msc4321PolicyListUpgradeOptions.Msc4321PolicyListUpgradeType.Transition,
                        _ => throw new ArgumentException("Unknown MSC4321 policy list upgrade type: " + ctx.Args[i])
                    };
                    break;

                case "--upgrade":
                    if (rb is not RoomUpgradeBuilder upgradeBuilderUpgrade) {
                        throw new InvalidOperationException("Upgrade can only be used with room upgrades");
                    }
                    upgradeBuilderUpgrade.OldRoomId = ctx.Args[++i];
                    break;
                case "--help":
                    await PrintHelp();
                    return;
                default:
                    throw new ArgumentException("Unknown argument: " + ctx.Args[i]);
            }
        }

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
        Console.WriteLine("  --upgrade <roomId>                                 Create a room upgrade file instead of a new room file - WARNING: incompatible with non-upgrade options");
        Console.WriteLine("  --invite-members                                   Invite members during room upgrade");
        Console.WriteLine("  --invite-powerlevel-users                          Invite users with power levels during room upgrade");
        Console.WriteLine("  --migrate-bans                                     Migrate bans during room upgrade");
        Console.WriteLine("  --migrate-empty-state-events                       Migrate empty state events during room upgrade");
        Console.WriteLine("  --upgrade-unstable-values                          Upgrade unstable values during room upgrade");
        Console.WriteLine("  --msc4321-policy-list-upgrade <move|transition>    Upgrade MSC4321 policy list");
        Console.WriteLine("WARNING: The --upgrade option is incompatible with options listed under \"New room\", please use the equivalent options in the `apply-upgrade` command instead.");
        await host.StopAsync();
    }
}