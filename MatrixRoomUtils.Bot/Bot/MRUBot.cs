using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using MatrixRoomUtils.Bot;
using MatrixRoomUtils.Bot.Interfaces;
using MatrixRoomUtils.Core;
using MatrixRoomUtils.Core.Extensions;
using MatrixRoomUtils.Core.Helpers;
using MatrixRoomUtils.Core.Services;
using MatrixRoomUtils.Core.StateEventTypes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

public class MRUBot : IHostedService {
    private readonly HomeserverProviderService _homeserverProviderService;
    private readonly ILogger<MRUBot> _logger;
    private readonly MRUBotConfiguration _configuration;
    private readonly IEnumerable<ICommand> _commands;

    public MRUBot(HomeserverProviderService homeserverProviderService, ILogger<MRUBot> logger,
        MRUBotConfiguration configuration, IServiceProvider services) {
        logger.LogInformation("MRUBot hosted service instantiated!");
        _homeserverProviderService = homeserverProviderService;
        _logger = logger;
        _configuration = configuration;
        _logger.LogInformation("Getting commands...");
        _commands = services.GetServices<ICommand>();
        _logger.LogInformation($"Got {_commands.Count()} commands!");
    }

    /// <summary>Triggered when the application host is ready to start the service.</summary>
    /// <param name="cancellationToken">Indicates that the start process has been aborted.</param>
    [SuppressMessage("ReSharper", "FunctionNeverReturns")]
    public async Task StartAsync(CancellationToken cancellationToken) {
        Directory.GetFiles("bot_data/cache").ToList().ForEach(File.Delete);
        AuthenticatedHomeServer hs;
        try {
            hs = await _homeserverProviderService.GetAuthenticatedWithToken(_configuration.Homeserver,
                _configuration.AccessToken);
        }
        catch (Exception e) {
            _logger.LogError(e.Message);
            throw;
        }

        await (await hs.GetRoom("!DoHEdFablOLjddKWIp:rory.gay")).JoinAsync();

        // foreach (var room in await hs.GetJoinedRooms()) {
        //     if(room.RoomId is "!OGEhHVWSdvArJzumhm:matrix.org") continue;
        //     foreach (var stateEvent in await room.GetStateAsync<List<StateEvent>>("")) {
        //         var _ = stateEvent.GetType;
        //     }
        //     _logger.LogInformation($"Got room state for {room.RoomId}!");
        // }

        hs.SyncHelper.InviteReceivedHandlers.Add(async Task (args) => {
            var inviteEvent =
                args.Value.InviteState.Events.FirstOrDefault(x =>
                    x.Type == "m.room.member" && x.StateKey == hs.WhoAmI.UserId);
            _logger.LogInformation(
                $"Got invite to {args.Key} by {inviteEvent.Sender} with reason: {(inviteEvent.TypedContent as RoomMemberEventData).Reason}");
            if (inviteEvent.Sender == "@emma:rory.gay") {
                try {
                    await (await hs.GetRoom(args.Key)).JoinAsync(reason: "I was invited by Emma (Rory&)!");
                }
                catch (Exception e) {
                    _logger.LogError(e.ToString());
                    await (await hs.GetRoom(args.Key)).LeaveAsync(reason: "I was unable to join the room: " + e);
                }
            }
        });
        hs.SyncHelper.TimelineEventHandlers.Add(async @event => {
            _logger.LogInformation(
                $"Got timeline event in {@event.RoomId}: {@event.ToJson(indent: false, ignoreNull: true)}");

            var room = await hs.GetRoom(@event.RoomId);
            // _logger.LogInformation(eventResponse.ToJson(indent: false));
            if (@event is { Type: "m.room.message", TypedContent: MessageEventData message }) {
                if (message is { MessageType: "m.text" } && message.Body.StartsWith(_configuration.Prefix)) {
                    
                        var command = _commands.FirstOrDefault(x => x.Name == message.Body.Split(' ')[0][_configuration.Prefix.Length..]);
                        if (command == null) {
                            await room.SendMessageEventAsync("m.room.message",
                                new MessageEventData() {
                                    MessageType = "m.text",
                                    Body = "Command not found!"
                                });
                            return;
                        }
                        var ctx = new CommandContext() {
                            Room = room,
                            MessageEvent = @event
                        };
                        if (await command.CanInvoke(ctx)) {
                            await command.Invoke(ctx);
                        }
                        else {
                            await room.SendMessageEventAsync("m.room.message",
                                new MessageEventData() {
                                    MessageType = "m.text",
                                    Body = "You do not have permission to run this command!"
                                });
                        }
                }
            }
        });
        await hs.SyncHelper.RunSyncLoop(cancellationToken);
    }

    /// <summary>Triggered when the application host is performing a graceful shutdown.</summary>
    /// <param name="cancellationToken">Indicates that the shutdown process should no longer be graceful.</param>
    public async Task StopAsync(CancellationToken cancellationToken) {
        _logger.LogInformation("Shutting down bot!");
    }
}