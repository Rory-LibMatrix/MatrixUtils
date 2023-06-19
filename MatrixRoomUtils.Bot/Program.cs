// See https://aka.ms/new-console-template for more information

using MatrixRoomUtils.Bot;
using MatrixRoomUtils.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

Console.WriteLine("Hello, World!");

using IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((_, services) => {
        services.AddScoped<TieredStorageService>(x =>
            new(
                cacheStorageProvider: new FileStorageProvider("data/cache/"),
                dataStorageProvider: new FileStorageProvider("data/data/")
            )
        );

        services.AddRoryLibMatrixServices();

        services.AddHostedService<MRUBot>();
    })
    .Build();


await host.RunAsync();