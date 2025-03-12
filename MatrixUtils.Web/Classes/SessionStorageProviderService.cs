using Blazored.SessionStorage;
using LibMatrix.Interfaces.Services;

namespace MatrixUtils.Web.Classes;

public class SessionStorageProviderService : IStorageProvider {
    private readonly ISessionStorageService _sessionStorageService;

    public SessionStorageProviderService(ISessionStorageService sessionStorage) {
        _sessionStorageService = sessionStorage;
    }

    Task IStorageProvider.SaveAllChildrenAsync<T>(string key, T value) {
        throw new NotImplementedException();
    }

    Task<T?> IStorageProvider.LoadAllChildrenAsync<T>(string key) where T : default => throw new NotImplementedException();

    async Task IStorageProvider.SaveObjectAsync<T>(string key, T value) => await _sessionStorageService.SetItemAsync(key, value);

    async Task<T?> IStorageProvider.LoadObjectAsync<T>(string key) where T : default => await _sessionStorageService.GetItemAsync<T>(key);

    async Task<bool> IStorageProvider.ObjectExistsAsync(string key) => await _sessionStorageService.ContainKeyAsync(key);

    async Task<IEnumerable<string>> IStorageProvider.GetAllKeysAsync() => (await _sessionStorageService.KeysAsync()).ToList();

    async Task IStorageProvider.DeleteObjectAsync(string key) => await _sessionStorageService.RemoveItemAsync(key);
}
