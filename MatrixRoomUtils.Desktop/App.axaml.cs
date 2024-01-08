using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using LibMatrix.Services;
using MatrixRoomUtils.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MatrixRoomUtils.Desktop;

public partial class App : Application {
    public IHost host { get; set; }

    public override void OnFrameworkInitializationCompleted() {
        host = Host.CreateDefaultBuilder().ConfigureServices((ctx, services) => {
            services.AddSingleton<MRUDesktopConfiguration>();
            services.AddSingleton<SentryService>();
            services.AddSingleton<TieredStorageService>(x =>
                new TieredStorageService(
                    cacheStorageProvider: new FileStorageProvider(x.GetService<MRUDesktopConfiguration>()!.CacheStoragePath),
                    dataStorageProvider: new FileStorageProvider(x.GetService<MRUDesktopConfiguration>()!.DataStoragePath)
                )
            );
            services.AddSingleton(new RoryLibMatrixConfiguration {
                AppName = "MatrixRoomUtils.Desktop"
            });
            services.AddRoryLibMatrixServices();
            // foreach (var commandClass in new ClassCollector<ICommand>().ResolveFromAllAccessibleAssemblies()) {
            // Console.WriteLine($"Adding command {commandClass.Name}");
            // services.AddScoped(typeof(ICommand), commandClass);
            // }
            services.AddSingleton<MRUStorageWrapper>();
            services.AddSingleton<MainWindow>();
            services.AddSingleton(this);
        }).UseConsoleLifetime().Build();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
            var scopeFac = host.Services.GetService<IServiceScopeFactory>();
            var scope = scopeFac.CreateScope();
            desktop.MainWindow = scope.ServiceProvider.GetRequiredService<MainWindow>();
        }
        
        if(Environment.GetEnvironmentVariable("AVALONIA_THEME")?.Equals("dark", StringComparison.OrdinalIgnoreCase) ?? false)
            RequestedThemeVariant = ThemeVariant.Dark;
        
        base.OnFrameworkInitializationCompleted();
    }
}