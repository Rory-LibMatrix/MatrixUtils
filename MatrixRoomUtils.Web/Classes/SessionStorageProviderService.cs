using Blazored.SessionStorage;
using MatrixRoomUtils.Core.Interfaces.Services;

namespace MatrixRoomUtils.Web.Classes; 

public class SessionStorageProviderService : IStorageProvider {
    private readonly ISessionStorageService _sessionStorage;

    public SessionStorageProviderService(ISessionStorageService sessionStorage) {
        _sessionStorage = sessionStorage;
    }
}