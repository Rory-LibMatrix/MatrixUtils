using Blazored.LocalStorage;
using LibMatrix.Interfaces.Services;

namespace MatrixUtils.Web.Classes;

public class LocalStorageProviderService(ILocalStorageService localStorageService) : IStorageProvider {
    Task IStorageProvider.SaveAllChildrenAsync<T>(string key, T value) {
        throw new NotImplementedException();
    }

    Task<T?> IStorageProvider.LoadAllChildrenAsync<T>(string key) where T : default => throw new NotImplementedException();

    async Task IStorageProvider.SaveObjectAsync<T>(string key, T value) => await localStorageService.SetItemAsync(key, value);

    async Task<T?> IStorageProvider.LoadObjectAsync<T>(string key) where T : default => await localStorageService.GetItemAsync<T>(key);

    async Task<bool> IStorageProvider.ObjectExistsAsync(string key) => await localStorageService.ContainKeyAsync(key);

    async Task<IEnumerable<string>> IStorageProvider.GetAllKeysAsync() => (await localStorageService.KeysAsync()).ToList();

    async Task IStorageProvider.DeleteObjectAsync(string key) => await localStorageService.RemoveItemAsync(key);
}
