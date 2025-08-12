using LibMatrix.EventTypes.Spec.State.RoomInfo;
using LibMatrix.Helpers;
using LibMatrix.Homeservers;

namespace MatrixUtils.RoomUpgradeCLI.Extensions;

public static class RoomBuilderExtensions {
    public static async Task ApplyRoomUpgradeCLIArgs(this RoomBuilder rb, AuthenticatedHomeserverGeneric hs, string[] args, bool isNewState = false) {
        for (int i = 0; i < args.Length; i++) {
            Console.WriteLine($"Parsing arg {i}: {args[i]}");
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
                    rb.IsFederatable = bool.Parse(args[++i]);
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

                    upgradeBuilderForce.UpgradeOptions.ForceUpgrade = true;
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
                    // await PrintHelp();
                    return;
                default:
                    throw new ArgumentException("Unknown argument: " + args[i]);
            }
        }
    }
}