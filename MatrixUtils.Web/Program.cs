using System.Text.Json;
using System.Text.Json.Serialization;
using Blazored.LocalStorage;
using Blazored.SessionStorage;
using LibMatrix.Services;
using MatrixUtils.Web;
using MatrixUtils.Web.Classes;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// builder.Logging.SetMinimumLevel(LogLevel.Trace);

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

try {
    builder.Configuration.AddJsonStream(await new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) }.GetStreamAsync("/appsettings.json"));
#if DEBUG
    builder.Configuration.AddJsonStream(await new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) }.GetStreamAsync("/appsettings.Development.json"));
#endif
}
catch (Exception e) {
    Console.WriteLine("Could not load appsettings: " + e);
}

builder.Services.AddBlazoredLocalStorage(config => {
    config.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
    config.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    config.JsonSerializerOptions.IgnoreReadOnlyProperties = true;
    config.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    config.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    config.JsonSerializerOptions.ReadCommentHandling = JsonCommentHandling.Skip;
    config.JsonSerializerOptions.WriteIndented = false;
});
builder.Services.AddBlazoredSessionStorage(config => {
    config.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
    config.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    config.JsonSerializerOptions.IgnoreReadOnlyProperties = true;
    config.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    config.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    config.JsonSerializerOptions.ReadCommentHandling = JsonCommentHandling.Skip;
    config.JsonSerializerOptions.WriteIndented = false;
});

builder.Services.AddScoped<TieredStorageService>(x =>
    new TieredStorageService(
        cacheStorageProvider: new SessionStorageProviderService(x.GetRequiredService<ISessionStorageService>()),
        dataStorageProvider: new LocalStorageProviderService(x.GetRequiredService<ILocalStorageService>())
    )
);

builder.Services.AddRoryLibMatrixServices();
builder.Services.AddScoped<RMUStorageWrapper>();
await builder.Build().RunAsync();
