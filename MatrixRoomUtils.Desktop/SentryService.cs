using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Sentry;

namespace MatrixRoomUtils.Desktop;

public class SentryService : IDisposable {
    private IDisposable? _sentrySdkDisposable;
    public SentryService(IServiceScopeFactory scopeFactory, ILogger<SentryService> logger) {
        MRUDesktopConfiguration config = scopeFactory.CreateScope().ServiceProvider.GetRequiredService<MRUDesktopConfiguration>();
        if (config.SentryDsn is null) {
            logger.LogWarning("Sentry DSN is not set, skipping Sentry initialisation");
            return;
        }
        _sentrySdkDisposable = SentrySdk.Init(o => {
            o.Dsn = config.SentryDsn;
            // When configuring for the first time, to see what the SDK is doing:
            o.Debug = true;
            // Set traces_sample_rate to 1.0 to capture 100% of transactions for performance monitoring.
            // We recommend adjusting this value in production.
            o.TracesSampleRate = 1.0;
            // Enable Global Mode if running in a client app
            o.IsGlobalModeEnabled = true;
        });
    }

    /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
    public void Dispose() => _sentrySdkDisposable?.Dispose();
}
