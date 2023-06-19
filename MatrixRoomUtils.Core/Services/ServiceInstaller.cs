using MatrixRoomUtils.Core.Interfaces.Services;
using Microsoft.Extensions.DependencyInjection;

namespace MatrixRoomUtils.Core.Services; 

public static class ServiceInstaller {
    
    public static IServiceCollection AddRoryLibMatrixServices(this IServiceCollection services) {
        if (!services.Any(x => x.ServiceType == typeof(TieredStorageService)))
            throw new Exception("[MRUCore/DI] No TieredStorageService has been registered!");
        services.AddScoped<HomeserverProviderService>();
        return services;
    }
    
    
}