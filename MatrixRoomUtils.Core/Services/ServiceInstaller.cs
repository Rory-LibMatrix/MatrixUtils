using Microsoft.Extensions.DependencyInjection;

namespace MatrixRoomUtils.Core.Services; 

public static class ServiceInstaller {
    
    public static IServiceCollection AddRoryLibMatrixServices(this IServiceCollection services, RoryLibMatrixConfiguration? config = null) {
        //Check required services
        if (!services.Any(x => x.ServiceType == typeof(TieredStorageService)))
            throw new Exception("[MRUCore/DI] No TieredStorageService has been registered!");
        //Add config
        if(config != null)
            services.AddSingleton(config);
        else {
            services.AddSingleton(new RoryLibMatrixConfiguration());
        }
        //Add services
        services.AddScoped<HomeserverProviderService>();
        services.AddScoped<HomeserverResolverService>();
        services.AddScoped<HttpClient>();
        return services;
    }
    
    
}

public class RoryLibMatrixConfiguration {
    public string AppName { get; set; } = "Rory&::LibMatrix";
}