namespace KworkNotify.Core.Service.Cache;

public interface IAppCache
{
    Task<string?> GetAsync(string key);
    Task SetAsync(string key, string value, TimeSpan? expiry = null);
    Task<T?> GetAsync<T>(string key);
    Task<bool> SetAsync<T>(string key, T value, TimeSpan? expiry = null);
    Task<bool> ReplaceIfExistsAsync<T>(string key, T value, TimeSpan? expiry = null, bool keepTtl = false);
    Task<bool> SetKeyAsync(string key, TimeSpan? expiry = null);
    Task<bool> KeyExistsAsync(string key);
}