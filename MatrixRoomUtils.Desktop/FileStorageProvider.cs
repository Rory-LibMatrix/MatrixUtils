using System.Text.Json;
using MatrixRoomUtils.Core.Extensions;
using MatrixRoomUtils.Core.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace MatrixRoomUtils.Desktop; 

public class FileStorageProvider : IStorageProvider {
    private readonly ILogger<FileStorageProvider> _logger;

    public string TargetPath { get; }

    /// <summary>
    /// Creates a new instance of <see cref="FileStorageProvider" />.
    /// </summary>
    /// <param name="targetPath"></param>
    public FileStorageProvider(string targetPath) {
        new Logger<FileStorageProvider>(new LoggerFactory()).LogInformation("test");
        Console.WriteLine($"Initialised FileStorageProvider with path {targetPath}");
        TargetPath = targetPath;
        if(!Directory.Exists(targetPath)) {
            Directory.CreateDirectory(targetPath);
        }
    }

    public async Task SaveObjectAsync<T>(string key, T value) => await File.WriteAllTextAsync(Path.Join(TargetPath, key), ObjectExtensions.ToJson(value));

    public async Task<T?> LoadObjectAsync<T>(string key) => JsonSerializer.Deserialize<T>(await File.ReadAllTextAsync(Path.Join(TargetPath, key)));

    public async Task<bool> ObjectExistsAsync(string key) => File.Exists(Path.Join(TargetPath, key));

    public async Task<List<string>> GetAllKeysAsync() => Directory.GetFiles(TargetPath).Select(Path.GetFileName).ToList();

    public async Task DeleteObjectAsync(string key) => File.Delete(Path.Join(TargetPath, key));
}