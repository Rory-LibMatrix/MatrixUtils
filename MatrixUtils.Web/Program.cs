using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using Blazored.LocalStorage;
using Blazored.SessionStorage;
using LibMatrix.Services;
using MatrixUtils.Web;
using MatrixUtils.Web.Classes;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using SpawnDev.BlazorJS;
using SpawnDev.BlazorJS.WebWorkers;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// builder.Logging.SetMinimumLevel(LogLevel.Trace);

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddBlazorJSRuntime();
builder.Services.AddWebWorkerService(webWorkerService =>
{
    // Optionally configure the WebWorkerService service before it is used
    // Default WebWorkerService.TaskPool settings: PoolSize = 0, MaxPoolSize = 1, AutoGrow = true
    // Below sets TaskPool max size to 2. By default the TaskPool size will grow as needed up to the max pool size.
    // Setting max pool size to -1 will set it to the value of navigator.hardwareConcurrency
    webWorkerService.TaskPool.MaxPoolSize = 2;
    // Below is telling the WebWorkerService TaskPool to set the initial size to 2 if running in a Window scope and 0 otherwise
    // This starts up 2 WebWorkers to handle TaskPool tasks as needed
    webWorkerService.TaskPool.PoolSize = webWorkerService.GlobalScope == GlobalScope.Window ? 0 : 0;
});

try {
    builder.Configuration.AddJsonStream(await new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) }.GetStreamAsync("/appsettings.json"));
#if DEBUG
    builder.Configuration.AddJsonStream(await new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) }.GetStreamAsync("/appsettings.Development.json"));
#endif
}
catch (HttpRequestException e) {
    if (e.StatusCode == HttpStatusCode.NotFound)
        Console.WriteLine("Could not load appsettings, server returned 404.");
    else
        Console.WriteLine("Could not load appsettings: " + e);
}
catch (Exception e) {
    Console.WriteLine("Could not load appsettings: " + e);
}

builder.Logging.AddConfiguration(
    builder.Configuration.GetSection("Logging"));

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
builder.Services.AddScoped<RmuSessionStore>();
// await builder.Build().RunAsync();
await builder.Build().BlazorJSRunAsync();