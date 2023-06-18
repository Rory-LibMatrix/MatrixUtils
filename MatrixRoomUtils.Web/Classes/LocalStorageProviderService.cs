using Blazored.LocalStorage;
using MatrixRoomUtils.Core.Interfaces.Services;

namespace MatrixRoomUtils.Web.Classes; 

public class LocalStorageProviderService : IStorageProvider {
    public LocalStorageProviderService(ILocalStorageService localStorageService) {
        
    }
}