using System.Collections;
using System.Diagnostics.CodeAnalysis;
using ArcaneLibs.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MatrixRoomUtils.Desktop;

public class MRUDesktopConfiguration {
    private static ILogger<MRUDesktopConfiguration> _logger;

    [RequiresUnreferencedCode("Uses reflection binding")]
    public MRUDesktopConfiguration(ILogger<MRUDesktopConfiguration> logger, IConfiguration config, HostBuilderContext host) {
        _logger = logger;
        logger.LogInformation("Loading configuration for environment: {}...", host.HostingEnvironment.EnvironmentName);
        config.GetSection("MRUDesktop").Bind(this);
        DataStoragePath = ExpandPath(DataStoragePath);
        CacheStoragePath = ExpandPath(CacheStoragePath);
    }

    public string DataStoragePath { get; set; } = "";
    public string CacheStoragePath { get; set; } = "";
    public string? SentryDsn { get; set; }

    private static string ExpandPath(string path, bool retry = true) {
        _logger.LogInformation("Expanding path `{}`", path);

        if (path.StartsWith('~')) {
            path = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), path[1..]);
        }

        Environment.GetEnvironmentVariables().Cast<DictionaryEntry>().OrderByDescending(x => x.Key.ToString()!.Length).ToList().ForEach(x => {
            path = path.Replace($"${x.Key}", x.Value.ToString());
        });

        _logger.LogInformation("Expanded path to `{}`", path);
        var tries = 0;
        while (retry && path.ContainsAnyOf("~$".Split())) {
            if (tries++ > 100)
                throw new Exception($"Path `{path}` contains unrecognised environment variables");
            path = ExpandPath(path, false);
        }

        return path;
    }
}
