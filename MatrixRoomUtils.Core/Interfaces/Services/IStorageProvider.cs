namespace MatrixRoomUtils.Core.Interfaces.Services; 

public interface IStorageProvider {
    // save 
    public async Task SaveAll() {
        Console.WriteLine($"StorageProvider<{GetType().Name}> does not implement Save()!");
    }

    public async Task SaveObject<T>(string key, T value) {
        Console.WriteLine($"StorageProvider<{GetType().Name}> does not implement SaveObject<T>(key, value)!");
    }

    // delete
    public async Task DeleteObject(string key) {
        Console.WriteLine($"StorageProvider<{GetType().Name}> does not implement DeleteObject(key)!");
    }
}