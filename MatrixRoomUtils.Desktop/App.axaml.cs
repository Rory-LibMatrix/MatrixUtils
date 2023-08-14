using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using LibMatrix.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Sentry;

namespace MatrixRoomUtils.Desktop;

public partial class App : Application {
    public IHost host { get; set; }

    public override void Initialize() {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted() {
        host = Host.CreateDefaultBuilder().ConfigureServices((ctx, services) => {
            services.AddSingleton<MRUDesktopConfiguration>();
            services.AddSingleton<SentryService>();
            services.AddSingleton<TieredStorageService>(x =>
                new(
                    cacheStorageProvider: new FileStorageProvider(x.GetService<MRUDesktopConfiguration>().CacheStoragePath),
                    dataStorageProvider: new FileStorageProvider(x.GetService<MRUDesktopConfiguration>().DataStoragePath)
                )
            );
            services.AddSingleton(new RoryLibMatrixConfiguration() {
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
        base.OnFrameworkInitializationCompleted();
    }
}
