// See https://aka.ms/new-console-template for more information

using MatrixRoomUtils.Bot;
using MatrixRoomUtils.Bot.Bot;
using MatrixRoomUtils.Bot.Bot.Interfaces;
using MatrixRoomUtils.Core.Extensions;
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
    foreach (var commandClass in new ClassCollector<ICommand>().ResolveFromAllAccessibleAssemblies()) {
        Console.WriteLine($"Adding command {commandClass.Name}");
        services.AddScoped(typeof(ICommand), commandClass);
    }
    services.AddHostedService<MRUBot>();
}).UseConsoleLifetime().Build();

await host.RunAsync();