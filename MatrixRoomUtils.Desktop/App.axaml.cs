using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using MatrixRoomUtils.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MatrixRoomUtils.Desktop;

public partial class App : Application {
    public IHost host { get; set; }

    public override void Initialize() {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted() {
        host = Host.CreateDefaultBuilder().ConfigureServices((ctx, services) => {
            services.AddScoped<MRUDesktopConfiguration>();
            services.AddScoped<TieredStorageService>(x =>
                new(
                    cacheStorageProvider: new FileStorageProvider(x.GetService<MRUDesktopConfiguration>().CacheStoragePath),
                    dataStorageProvider: new FileStorageProvider(x.GetService<MRUDesktopConfiguration>().CacheStoragePath)
                )
            );
            services.AddRoryLibMatrixServices();
            // foreach (var commandClass in new ClassCollector<ICommand>().ResolveFromAllAccessibleAssemblies()) {
            // Console.WriteLine($"Adding command {commandClass.Name}");
            // services.AddScoped(typeof(ICommand), commandClass);
            // }
            services.AddScoped<MRUStorageWrapper>();
            services.AddScoped<MainWindow>();
            services.AddSingleton(this);
        }).UseConsoleLifetime().Build();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
            var scopeFac = host.Services.GetService<IServiceScopeFactory>();
            var scope = scopeFac.CreateScope();
            desktop.MainWindow = scope.ServiceProvider.GetRequiredService<MainWindow>();
        }
        base.OnFrameworkInitializationCompleted();
    }
}
