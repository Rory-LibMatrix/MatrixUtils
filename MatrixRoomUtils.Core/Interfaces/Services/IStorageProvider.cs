namespace MatrixRoomUtils.Core.Interfaces.Services;

public interface IStorageProvider {
    // save all children of a type with reflection
    public Task SaveAllChildrenAsync<T>(string key, T value) {
        Console.WriteLine($"StorageProvider<{GetType().Name}> does not implement SaveAllChildren<T>(key, value)!");
        throw new NotImplementedException();
    }

    // load all children of a type with reflection
    public Task<T?> LoadAllChildrenAsync<T>(string key) {
        Console.WriteLine($"StorageProvider<{GetType().Name}> does not implement LoadAllChildren<T>(key)!");
        throw new NotImplementedException();
    }


    public Task SaveObjectAsync<T>(string key, T value) {
        Console.WriteLine($"StorageProvider<{GetType().Name}> does not implement SaveObject<T>(key, value)!");
        throw new NotImplementedException();
    }

    // load
    public Task<T?> LoadObjectAsync<T>(string key) {
        Console.WriteLine($"StorageProvider<{GetType().Name}> does not implement LoadObject<T>(key)!");
        throw new NotImplementedException();
    }

    // check if exists
    public Task<bool> ObjectExistsAsync(string key) {
        Console.WriteLine($"StorageProvider<{GetType().Name}> does not implement ObjectExists(key)!");
        throw new NotImplementedException();
    }

    // get all keys
    public Task<List<string>> GetAllKeysAsync() {
        Console.WriteLine($"StorageProvider<{GetType().Name}> does not implement GetAllKeys()!");
        throw new NotImplementedException();
    }


    // delete
    public Task DeleteObjectAsync(string key) {
        Console.WriteLine($"StorageProvider<{GetType().Name}> does not implement DeleteObject(key)!");
        throw new NotImplementedException();
    }

    // save stream
    public Task SaveStreamAsync(string key, Stream stream) {
        Console.WriteLine($"StorageProvider<{GetType().Name}> does not implement SaveStream(key, stream)!");
        throw new NotImplementedException();
    }

    // load stream
    public Task<Stream?> LoadStreamAsync(string key) {
        Console.WriteLine($"StorageProvider<{GetType().Name}> does not implement LoadStream(key)!");
        throw new NotImplementedException();
    }
}
