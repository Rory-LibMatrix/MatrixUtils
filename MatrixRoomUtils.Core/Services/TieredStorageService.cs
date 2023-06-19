using MatrixRoomUtils.Core.Interfaces.Services;

namespace MatrixRoomUtils.Core.Services; 

public class TieredStorageService {
    public IStorageProvider CacheStorageProvider { get; }
    public IStorageProvider DataStorageProvider { get; }

    public TieredStorageService(IStorageProvider cacheStorageProvider, IStorageProvider dataStorageProvider) {
        CacheStorageProvider = cacheStorageProvider;
        DataStorageProvider = dataStorageProvider;
    }
}