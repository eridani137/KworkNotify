using System.Text.Json;
using StackExchange.Redis;

namespace KworkNotify.Core.Service;

public interface IRedisService
{
    Task<string?> GetAsync(string key);
    Task SetAsync(string key, string value, TimeSpan? expiry = null);
    Task<T?> GetAsync<T>(string key);
    Task<bool> SetAsync<T>(string key, T value, TimeSpan? expiry = null);
    Task<bool> ReplaceIfExistsAsync<T>(string key, T value, TimeSpan? expiry = null, bool keepTtl = false);
    Task<bool> SetKeyAsync(string key, TimeSpan? expiry = null);
    Task<bool> KeyExistsAsync(string key);
}

public class RedisService(IConnectionMultiplexer connection) : IRedisService
{
    public async Task<string?> GetAsync(string key)
    {
        var db = connection.GetDatabase();
        return await db.StringGetAsync(key);
    }

    public async Task SetAsync(string key, string value, TimeSpan? expiry = null)
    {
        var db = connection.GetDatabase();
        await db.StringSetAsync(key, value, expiry);
    }
    
    public async Task<T?> GetAsync<T>(string key)
    {
        ArgumentNullException.ThrowIfNull(key);
        var db = connection.GetDatabase();
        var value = await db.StringGetAsync(key);
        return value.IsNullOrEmpty ? default : JsonSerializer.Deserialize<T>(value!);
    }

    public async Task<bool> SetAsync<T>(string key, T value, TimeSpan? expiry = null)
    {
        var db = connection.GetDatabase();
        var jsonValue = JsonSerializer.Serialize(value);
        return await db.StringSetAsync(key, jsonValue, expiry);
    }
    
    public async Task<bool> ReplaceIfExistsAsync<T>(string key, T value, TimeSpan? expiry = null, bool keepTtl = false)
    {
        var db = connection.GetDatabase();
        var jsonValue = JsonSerializer.Serialize(value);
        return await db.StringSetAsync(key, jsonValue, expiry, keepTtl: keepTtl, when: When.Exists);
    }
    
    public async Task<bool> SetKeyAsync(string key, TimeSpan? expiry = null)
    {
        var db = connection.GetDatabase();
        return await db.StringSetAsync(key, "1", expiry);
    }

    public async Task<bool> KeyExistsAsync(string key)
    {
        var db = connection.GetDatabase();
        return await db.KeyExistsAsync(key);
    }
}