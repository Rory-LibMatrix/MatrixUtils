using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using LibMatrix.Services;
using MatrixUtils.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MatrixUtils.Desktop;

public partial class App : Application {
    public IHost host { get; set; }

    public override void OnFrameworkInitializationCompleted() {
        host = Host.CreateDefaultBuilder().ConfigureServices((ctx, services) => {
            services.AddSingleton<RMUDesktopConfiguration>();
            services.AddSingleton<SentryService>();
            services.AddSingleton<TieredStorageService>(x =>
                new TieredStorageService(
                    cacheStorageProvider: new FileStorageProvider(x.GetService<RMUDesktopConfiguration>()!.CacheStoragePath),
                    dataStorageProvider: new FileStorageProvider(x.GetService<RMUDesktopConfiguration>()!.DataStoragePath)
                )
            );
            services.AddSingleton(new RoryLibMatrixConfiguration {
                AppName = "MatrixUtils.Desktop"
            });
            services.AddRoryLibMatrixServices();
            // foreach (var commandClass in new ClassCollector<ICommand>().ResolveFromAllAccessibleAssemblies()) {
            // Console.WriteLine($"Adding command {commandClass.Name}");
            // services.AddScoped(typeof(ICommand), commandClass);
            // }
            services.AddSingleton<RMUStorageWrapper>();
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