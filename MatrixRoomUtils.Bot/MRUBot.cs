using System.CodeDom.Compiler;
using System.Diagnostics.CodeAnalysis;
using MatrixRoomUtils.Core;
using MatrixRoomUtils.Core.Extensions;
using MatrixRoomUtils.Core.Helpers;
using MatrixRoomUtils.Core.Responses;
using MatrixRoomUtils.Core.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

public class MRUBot : IHostedService {
    private readonly HomeserverProviderService _homeserverProviderService;
    private readonly ILogger<MRUBot> _logger;

    public MRUBot(HomeserverProviderService homeserverProviderService, ILogger<MRUBot> logger) {
        Console.WriteLine("MRUBot hosted service instantiated!");
        _homeserverProviderService = homeserverProviderService;
        _logger = logger;
    }

    /// <summary>Triggered when the application host is ready to start the service.</summary>
    /// <param name="cancellationToken">Indicates that the start process has been aborted.</param>
    [SuppressMessage("ReSharper", "FunctionNeverReturns")]
    public async Task StartAsync(CancellationToken cancellationToken) {
        var hs = await _homeserverProviderService.GetAuthenticatedWithToken("rory.gay", "syt_bXJ1Y29yZXRlc3Q_XKUmPswDGZBiLAmFfAut_1iO0KD");
        await (await hs.GetRoom("!DoHEdFablOLjddKWIp:rory.gay")).JoinAsync();
        // #pragma warning disable CS4014
        //         Task.Run(async Task? () => {
        // #pragma warning restore CS4014
        //             while (true) {
        //                 var rooms = await hs.GetJoinedRooms();
        //                 foreach (var room in rooms) {
        //                     var states = await room.GetStateAsync<List<StateEventResponse>>("");
        //                     foreach (var state in states) {
        //                         // Console.WriteLine(
        //                         //     $"{state.RoomId}: {state.Type}::{state.StateKey} = {ObjectExtensions.ToJson(state.Content, indent: false)}");
        //                     }
        //                 }
        //
        //                 await Task.Delay(1000, cancellationToken);
        //             }
        //         }, cancellationToken);
        #pragma warning disable CS4014
                Task.Run(async Task? () => {
        #pragma warning restore CS4014
                    SyncResult? sync = null;
                    while (true) {
                        sync = await hs.SyncHelper.Sync(sync?.NextBatch);
                        _logger.LogInformation($"Got sync, next batch: {sync?.NextBatch}!");
                    }
                }, cancellationToken);
        
    }

    /// <summary>Triggered when the application host is performing a graceful shutdown.</summary>
    /// <param name="cancellationToken">Indicates that the shutdown process should no longer be graceful.</param>
    public async Task StopAsync(CancellationToken cancellationToken) {
        Console.WriteLine("Shutting down bot!");
    }
}