namespace MatrixRoomUtils.Core.Interfaces.Services; 

public interface IStorageProvider {
    // save all children of a type with reflection
    public Task SaveAllChildren<T>(string key, T value) {
        Console.WriteLine($"StorageProvider<{GetType().Name}> does not implement SaveAllChildren<T>(key, value)!");
        return Task.CompletedTask;
    }
    
    // load all children of a type with reflection
    public Task<T?> LoadAllChildren<T>(string key) {
        Console.WriteLine($"StorageProvider<{GetType().Name}> does not implement LoadAllChildren<T>(key)!");
        return Task.FromResult(default(T));
    }


    public Task SaveObject<T>(string key, T value) {
        Console.WriteLine($"StorageProvider<{GetType().Name}> does not implement SaveObject<T>(key, value)!");
        return Task.CompletedTask;
    }
    
    // load
    public Task<T?> LoadObject<T>(string key) {
        Console.WriteLine($"StorageProvider<{GetType().Name}> does not implement LoadObject<T>(key)!");
        return Task.FromResult(default(T));
    }
    
    // check if exists
    public Task<bool> ObjectExists(string key) {
        Console.WriteLine($"StorageProvider<{GetType().Name}> does not implement ObjectExists(key)!");
        return Task.FromResult(false);
    }
    
    // get all keys
    public Task<List<string>> GetAllKeys() {
        Console.WriteLine($"StorageProvider<{GetType().Name}> does not implement GetAllKeys()!");
        return Task.FromResult(new List<string>());
    }
    

    // delete
    public Task DeleteObject(string key) {
        Console.WriteLine($"StorageProvider<{GetType().Name}> does not implement DeleteObject(key)!");
        return Task.CompletedTask;
    }
}