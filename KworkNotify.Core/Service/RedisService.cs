using StackExchange.Redis;

namespace KworkNotify.Core.Service;

public interface IRedisService
{
    Task<string?> GetAsync(string key);
    Task SetAsync(string key, string value, TimeSpan? expiry = null);
    Task<bool> SetKeyAsync(string key, TimeSpan? expiry = null);
    Task<bool> KeyExistsAsync(string key);
}

public class RedisService(ConnectionMultiplexer connectionMultiplexer) : IRedisService
{
    public async Task<string?> GetAsync(string key)
    {
        var db = connectionMultiplexer.GetDatabase();
        return await db.StringGetAsync(key);
    }

    public async Task SetAsync(string key, string value, TimeSpan? expiry = null)
    {
        var db = connectionMultiplexer.GetDatabase();
        await db.StringSetAsync(key, value, expiry);
    }
    
    public async Task<bool> SetKeyAsync(string key, TimeSpan? expiry = null)
    {
        var db = connectionMultiplexer.GetDatabase();
        return await db.StringSetAsync(key, "1", expiry);
    }

    public async Task<bool> KeyExistsAsync(string key)
    {
        var db = connectionMultiplexer.GetDatabase();
        return await db.KeyExistsAsync(key);
    }
}