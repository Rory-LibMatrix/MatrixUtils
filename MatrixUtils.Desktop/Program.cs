using Avalonia;
using Microsoft.Extensions.Hosting;
using Tmds.DBus.Protocol;

namespace MatrixUtils.Desktop;

internal class Program {
    private static IHost appHost;
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    // [STAThread]
    public static Task Main(string[] args) {
        try {
            BuildAvaloniaApp()
                .StartWithClassicDesktopLifetime(args);
        }
        catch (DBusException e) { }
        catch (Exception e) {
            Console.WriteLine(e);
            throw;
        }

        return Task.CompletedTask;
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}
