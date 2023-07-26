using MatrixRoomUtils.Core.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace MatrixRoomUtils.Core.Services;

public static class ServiceInstaller {

    public static IServiceCollection AddRoryLibMatrixServices(this IServiceCollection services, RoryLibMatrixConfiguration? config = null) {
        //Check required services
        if (!services.Any(x => x.ServiceType == typeof(TieredStorageService)))
            throw new Exception("[MRUCore/DI] No TieredStorageService has been registered!");
        //Add config
        if(config is not null)
            services.AddSingleton(config);
        else {
            services.AddSingleton(new RoryLibMatrixConfiguration());
        }
        //Add services
        services.AddSingleton<HomeserverProviderService>();
        services.AddSingleton<HomeserverResolverService>();
        // services.AddScoped<MatrixHttpClient>();
        return services;
    }


}

public class RoryLibMatrixConfiguration {
    public string AppName { get; set; } = "Rory&::LibMatrix";
}
