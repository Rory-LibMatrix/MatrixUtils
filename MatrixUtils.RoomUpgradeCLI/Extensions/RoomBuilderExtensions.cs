using LibMatrix.EventTypes.Spec.State.RoomInfo;
using LibMatrix.Helpers;
using LibMatrix.Homeservers;

namespace MatrixUtils.RoomUpgradeCLI.Extensions;

public static class RoomBuilderExtensions {
    public static async Task ApplyRoomUpgradeCLIArgs(this RoomBuilder rb, AuthenticatedHomeserverGeneric hs, string[] args, bool isNewState = false) {
        for (int i = 0; i < args.Length; i++) {
            // Console.WriteLine($"Parsing arg {i}: {args[i]}");
            switch (args[i]) {
                case "--alias":
                    rb.AliasLocalPart = args[++i];
                    break;
                case "--avatar-url":
                    rb.Avatar!.Url = args[++i];
                    break;
                case "--copy-avatar": {
                    var room = hs.GetRoom(args[++i]);
                    rb.Avatar = await room.GetAvatarUrlAsync() ?? throw new ArgumentException($"Room {room.RoomId} does not have an avatar");
                    break;
                }
                case "--copy-powerlevels": {
                    var room = hs.GetRoom(args[++i]);
                    rb.PowerLevels = await room.GetPowerLevelsAsync() ?? throw new ArgumentException($"Room {room.RoomId} does not have power levels???");
                    break;
                }
                case "--invite-admin":
                    var inviteAdmin = args[++i];
                    if (!inviteAdmin.StartsWith('@')) {
                        throw new ArgumentException("Invalid user reference: " + inviteAdmin);
                    }

                    rb.Invites.Add(inviteAdmin, "Marked explicitly as admin to be invited");
                    break;
                case "--invite":
                    var inviteUser = args[++i];
                    if (!inviteUser.StartsWith('@')) {
                        throw new ArgumentException("Invalid user reference: " + inviteUser);
                    }

                    rb.Invites.Add(inviteUser, "Marked explicitly to be invited");
                    break;
                case "--name":
                    var nameEvt = rb.Name = new() { Name = "" };
                    while (i + 1 < args.Length && !args[i + 1].StartsWith("--")) {
                        nameEvt.Name += (nameEvt.Name.Length > 0 ? " " : "") + args[++i];
                    }

                    break;
                case "--topic":
                    var topicEvt = rb.Topic = new() { Topic = "" };
                    while (i + 1 < args.Length && !args[i + 1].StartsWith("--")) {
                        topicEvt.Topic += (topicEvt.Topic.Length > 0 ? " " : "") + args[++i];
                    }

                    break;
                case "--federate":
                    rb.IsFederatable = GetBoolArg(args, ref i, true);
                    break;
                case "--public":
                case "--invite-only":
                case "--knock":
                case "--restricted":
                case "--knock_restricted":
                case "--private":
                    rb.JoinRules.JoinRule = args[i].Replace("--", "").ToLowerInvariant() switch {
                        "public" => RoomJoinRulesEventContent.JoinRules.Public,
                        "invite-only" => RoomJoinRulesEventContent.JoinRules.Invite,
                        "knock" => RoomJoinRulesEventContent.JoinRules.Knock,
                        "restricted" => RoomJoinRulesEventContent.JoinRules.Restricted,
                        "knock_restricted" => RoomJoinRulesEventContent.JoinRules.KnockRestricted,
                        "private" => RoomJoinRulesEventContent.JoinRules.Private,
                        _ => throw new ArgumentException("Unknown join rule: " + args[i])
                    };
                    break;
                case "--join-rule":
                    if (i + 1 >= args.Length || !args[i + 1].StartsWith("--")) {
                        throw new ArgumentException("Expected join rule after --join-rule");
                    }

                    rb.JoinRules.JoinRule = args[++i].ToLowerInvariant() switch {
                        "public" => RoomJoinRulesEventContent.JoinRules.Public,
                        "invite" => RoomJoinRulesEventContent.JoinRules.Invite,
                        "knock" => RoomJoinRulesEventContent.JoinRules.Knock,
                        "restricted" => RoomJoinRulesEventContent.JoinRules.Restricted,
                        "knock_restricted" => RoomJoinRulesEventContent.JoinRules.KnockRestricted,
                        "private" => RoomJoinRulesEventContent.JoinRules.Private,
                        _ => throw new ArgumentException("Unknown join rule: " + args[i])
                    };
                    break;
                case "--history-visibility":
                    rb.HistoryVisibility = new RoomHistoryVisibilityEventContent {
                        HistoryVisibility = args[++i].ToLowerInvariant() switch {
                            "shared" => RoomHistoryVisibilityEventContent.HistoryVisibilityTypes.Shared,
                            "invited" => RoomHistoryVisibilityEventContent.HistoryVisibilityTypes.Invited,
                            "joined" => RoomHistoryVisibilityEventContent.HistoryVisibilityTypes.Joined,
                            "world_readable" => RoomHistoryVisibilityEventContent.HistoryVisibilityTypes.WorldReadable,
                            _ => throw new ArgumentException("Unknown history visibility: " + args[i])
                        }
                    };
                    break;
                case "--type":
                    rb.Type = args[++i];
                    break;
                case "--version":
                    rb.Version = args[++i];
                    // if (!RoomBuilder.V12PlusRoomVersions.Contains(rb.Version)) {
                    // logger.LogWarning("Using room version {Version} which is not v12 or higher, this may cause issues with some features.", rb.Version);
                    // }
                    break;
                case "--encryption":
                    if (args[i + 1].StartsWith("--")) {
                        rb.Encryption.Algorithm = "m.megolm.v1.aes-sha2";
                    }
                    else {
                        rb.Encryption.Algorithm = args[++i];
                        if (rb.Encryption.Algorithm == "null")
                            rb.Encryption.Algorithm = null; // disable encryption
                    }

                    break;
                // upgrade options
                case "--invite-members":
                    if (rb is not RoomUpgradeBuilder upgradeBuilder) {
                        throw new InvalidOperationException("Invite members can only be used with room upgrades");
                    }

                    upgradeBuilder.UpgradeOptions.InviteMembers = GetBoolArg(args, ref i, true);
                    break;
                case "--invite-powerlevel-users":
                case "--invite-power-level-users":
                    if (rb is not RoomUpgradeBuilder upgradeBuilderInvite) {
                        throw new InvalidOperationException("Invite powerlevel users can only be used with room upgrades");
                    }

                    upgradeBuilderInvite.UpgradeOptions.InvitePowerlevelUsers = GetBoolArg(args, ref i, true);
                    break;
                case "--synapse-admin-join-local-users":
                    rb.SynapseAdminAutoAcceptLocalInvites = GetBoolArg(args, ref i, true);
                    break;
                case "--migrate-bans":
                    if (rb is not RoomUpgradeBuilder upgradeBuilderBan) {
                        throw new InvalidOperationException("Migrate bans can only be used with room upgrades");
                    }

                    upgradeBuilderBan.UpgradeOptions.MigrateBans = GetBoolArg(args, ref i, true);
                    break;
                case "--migrate-empty-state-events":
                    if (rb is not RoomUpgradeBuilder upgradeBuilderEmpty) {
                        throw new InvalidOperationException("Migrate empty state events can only be used with room upgrades");
                    }

                    upgradeBuilderEmpty.UpgradeOptions.MigrateEmptyStateEvents = GetBoolArg(args, ref i, true);
                    break;
                case "--upgrade-unstable-values":
                    if (rb is not RoomUpgradeBuilder upgradeBuilderUnstable) {
                        throw new InvalidOperationException("Update unstable values can only be used with room upgrades");
                    }

                    upgradeBuilderUnstable.UpgradeOptions.UpgradeUnstableValues = GetBoolArg(args, ref i, true);
                    break;
                case "--msc4321-policy-list-upgrade":
                    if (rb is not RoomUpgradeBuilder upgradeBuilderPolicy) {
                        throw new InvalidOperationException("MSC4321 policy list upgrade can only be used with room upgrades");
                    }

                    upgradeBuilderPolicy.UpgradeOptions.Msc4321PolicyListUpgradeOptions.Enable = true;
                    upgradeBuilderPolicy.UpgradeOptions.Msc4321PolicyListUpgradeOptions.UpgradeType = args[++i].ToLowerInvariant() switch {
                        "move" => RoomUpgradeBuilder.Msc4321PolicyListUpgradeOptions.Msc4321PolicyListUpgradeType.Move,
                        "transition" => RoomUpgradeBuilder.Msc4321PolicyListUpgradeOptions.Msc4321PolicyListUpgradeType.Transition,
                        _ => throw new ArgumentException("Unknown MSC4321 policy list upgrade type: " + args[i])
                    };
                    break;
                case "--force-upgrade":
                    if (rb is not RoomUpgradeBuilder upgradeBuilderForce) {
                        throw new InvalidOperationException("Force upgrade can only be used with room upgrades");
                    }

                    upgradeBuilderForce.UpgradeOptions.ForceUpgrade = GetBoolArg(args, ref i, true);
                    break;
                case "--noop-upgrade":
                    if (rb is not RoomUpgradeBuilder upgradeBuilderNoop) {
                        throw new InvalidOperationException("No-op upgrade can only be used with room upgrades");
                    }

                    upgradeBuilderNoop.UpgradeOptions.NoopUpgrade = GetBoolArg(args, ref i, true);
                    break;
                case "--upgrade":
                    if (rb is not RoomUpgradeBuilder upgradeBuilderUpgrade) {
                        throw new InvalidOperationException("Upgrade can only be used with room upgrades");
                    }

                    if (isNewState) {
                        upgradeBuilderUpgrade.OldRoomId = args[++i];
                        Console.WriteLine($"Popping arg for --upgrade(isNewState={isNewState}): " + upgradeBuilderUpgrade.OldRoomId);
                    }

                    break;
                case "--help":
                    PrintHelpAndExit();
                    return;
                default:
                    throw new ArgumentException("Unknown argument: " + args[i]);
            }
        }
    }

    private static bool GetBoolArg(string[] args, ref int i, bool defaultValue) {
        if (i + 1 < args.Length && bool.TryParse(args[i + 1], out var result)) {
            i++;
            return result;
        }

        return defaultValue;
    }

    private static void PrintHelpAndExit() {
        Console.WriteLine("""
                            --help                                             Show this help message
                            --version <version>                                Set the room version (e.g. 9, 10, 11, 12)
                          -- New room options --
                            --federate [True|false]                            Set whether the room is federatable [WARNING: Cannot be updated later!]
                            --type <type>                                      Set the room type (e.g. m.space, m.room, support.feline.policy.list.msc.v1 etc.) [WARNING: Cannot be updated later!]
                            --alias <alias>                                    Set the room alias (local part)
                            --avatar-url <url>                                 Set the room avatar URL
                            --copy-avatar <roomId>                             Copy the avatar from an existing room
                            --copy-powerlevels <roomId>                        Copy power levels from an existing room
                            --invite <userId>                                  Invite a user (userId must start with '@')
                            --invite-admin <userId>                            Invite a user as an admin (userId must start with '@')
                            --synapse-admin-join-local-users [True|false]      Automatically accept local user invites during room creation (Synapse only, requires synapse admin access)
                            --name <name>                                      Set the room name (can be multiple words)
                            --topic <topic>                                    Set the room topic (can be multiple words)
                            --join-rule <rule>                                 Set the room join rule (public, invite, knock, restricted, knock_restricted, private)
                                                                               Aliases: --public, --invite, --knock, --restricted, --knock_restricted, --private
                            --history-visibility <visibility>                  Set the room history visibility (shared, invited, joined, world_readable)
                          -- Upgrade options --
                            --upgrade <roomId>                                 Create a room upgrade file instead of a new room file - WARNING: incompatible with non-upgrade options
                            --invite-members [True|false]                      Invite members during room upgrade
                            --invite-local-users [True|false]                  Invite local users during room upgrade (also see --synapse-admin-join-local-users)
                            --invite-powerlevel-users [True|false]             Invite users with power levels during room upgrade
                            --migrate-bans [True|false]                        Migrate bans during room upgrade
                            --migrate-empty-state-events [True|false]          Migrate empty state events during room upgrade
                            --upgrade-unstable-values [True|false]             Upgrade unstable values during room upgrade
                            --msc4321-policy-list-upgrade <move|transition>    Upgrade MSC4321 policy list
                            --force-upgrade [True|false]                       Force upgrade even if you don't have the required permissions
                            --noop-upgrade [True|false]                        Perform the upgrade, but do not tombstone the old room
                          WARNING: The --upgrade option is incompatible with options listed under "New room", please use the equivalent options in the `modify` command instead.
                          """);
        Environment.Exit(0);
    }
}