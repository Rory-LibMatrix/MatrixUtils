using Blazored.LocalStorage;
using LibMatrix.Interfaces.Services;

namespace MatrixUtils.Web.Classes;

public class LocalStorageProviderService : IStorageProvider {
    private readonly ILocalStorageService _localStorageService;

    public LocalStorageProviderService(ILocalStorageService localStorageService) {
        _localStorageService = localStorageService;
    }

    Task IStorageProvider.SaveAllChildrenAsync<T>(string key, T value) {
        throw new NotImplementedException();
    }

    Task<T?> IStorageProvider.LoadAllChildrenAsync<T>(string key) where T : default => throw new NotImplementedException();

    async Task IStorageProvider.SaveObjectAsync<T>(string key, T value) => await _localStorageService.SetItemAsync(key, value);

    async Task<T?> IStorageProvider.LoadObjectAsync<T>(string key) where T : default => await _localStorageService.GetItemAsync<T>(key);

    async Task<bool> IStorageProvider.ObjectExistsAsync(string key) => await _localStorageService.ContainKeyAsync(key);

    async Task<List<string>> IStorageProvider.GetAllKeysAsync() => (await _localStorageService.KeysAsync()).ToList();

    async Task IStorageProvider.DeleteObjectAsync(string key) => await _localStorageService.RemoveItemAsync(key);
}
