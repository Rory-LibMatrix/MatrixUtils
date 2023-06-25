using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using MatrixRoomUtils.Bot;
using MatrixRoomUtils.Core;
using MatrixRoomUtils.Core.Extensions;
using MatrixRoomUtils.Core.Helpers;
using MatrixRoomUtils.Core.Services;
using MatrixRoomUtils.Core.StateEventTypes;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

public class MRUBot : IHostedService {
    private readonly HomeserverProviderService _homeserverProviderService;
    private readonly ILogger<MRUBot> _logger;
    private readonly MRUBotConfiguration _configuration;

    public MRUBot(HomeserverProviderService homeserverProviderService, ILogger<MRUBot> logger,
        MRUBotConfiguration configuration) {
        Console.WriteLine("MRUBot hosted service instantiated!");
        _homeserverProviderService = homeserverProviderService;
        _logger = logger;
        _configuration = configuration;
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

        hs.SyncHelper.InviteReceived += async (_, args) => {
            // Console.WriteLine($"Got invite to {args.Key}:");
            // foreach (var stateEvent in args.Value.InviteState.Events) {
            // Console.WriteLine($"[{stateEvent.Sender}: {stateEvent.StateKey}::{stateEvent.Type}] " +
            // ObjectExtensions.ToJson(stateEvent.Content, indent: false, ignoreNull: true));
            // }

            var inviteEvent =
                args.Value.InviteState.Events.FirstOrDefault(x =>
                    x.Type == "m.room.member" && x.StateKey == hs.WhoAmI.UserId);
            Console.WriteLine(
                $"Got invite to {args.Key} by {inviteEvent.Sender} with reason: {(inviteEvent.TypedContent as MemberEventData).Reason}");
            if (inviteEvent.Sender == "@emma:rory.gay") {
                try {
                    await (await hs.GetRoom(args.Key)).JoinAsync(reason: "I was invited by Emma (Rory&)!");
                }
                catch (Exception e) {
                    Console.WriteLine(e);
                    await (await hs.GetRoom(args.Key)).LeaveAsync(reason: "I was unable to join the room: " + e);
                }
            }
        };
        hs.SyncHelper.TimelineEventReceived += async (_, @event) => {
            Console.WriteLine(
                $"Got timeline event in {@event.RoomId}: {@event.ToJson(indent: false, ignoreNull: true)}");

            // Console.WriteLine(eventResponse.ToJson(indent: false));
            if (@event is { Type: "m.room.message", TypedContent: MessageEventData message }) {
                if (message is { MessageType: "m.text", Body: "!ping" }) {
                    Console.WriteLine(
                        $"Got ping from {@event.Sender} in {@event.RoomId} with message id {@event.EventId}!");
                    await (await hs.GetRoom(@event.RoomId)).SendMessageEventAsync("m.room.message",
                        new MessageEventData() { MessageType = "m.text", Body = "pong!" });
                }
            }
        };

        await hs.SyncHelper.RunSyncLoop(cancellationToken);
    }

    /// <summary>Triggered when the application host is performing a graceful shutdown.</summary>
    /// <param name="cancellationToken">Indicates that the shutdown process should no longer be graceful.</param>
    public async Task StopAsync(CancellationToken cancellationToken) {
        Console.WriteLine("Shutting down bot!");
    }
}