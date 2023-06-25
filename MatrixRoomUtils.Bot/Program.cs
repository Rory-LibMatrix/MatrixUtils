// See https://aka.ms/new-console-template for more information

using MatrixRoomUtils.Bot;
using MatrixRoomUtils.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

Console.WriteLine("Hello, World!");

var host = Host.CreateDefaultBuilder(args).ConfigureServices((_, services) => {
    services.AddScoped<TieredStorageService>(x =>
        new(
            cacheStorageProvider: new FileStorageProvider("bot_data/cache/"),
            dataStorageProvider: new FileStorageProvider("bot_data/data/")
        )
    );
    services.AddScoped<MRUBotConfiguration>();
    services.AddRoryLibMatrixServices();
    services.AddHostedService<MRUBot>();
}).UseConsoleLifetime().Build();

await host.RunAsync();