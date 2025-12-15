using System;
using System.Text.Json;
using System.Threading.Tasks;
using Cinephila.Domain.Redis;
using StackExchange.Redis;

namespace Cinephila.DataAccess.Redis
{
    public class RedisRepository : IRedisRepository
    {
        private readonly IDatabase _redis;

        public RedisRepository(ConnectionMultiplexer redis)
        {
            _redis = redis.GetDatabase();
        }

        public async Task<T> GetObjectAsync<T>(string key)
        {
            var content = await _redis.StringGetAsync(key);

            if (string.IsNullOrWhiteSpace(content))
                return default;

            return JsonSerializer.Deserialize<T>(content.ToString());
        }

        public async Task<bool> SetObjectAsync<T>(string key, T data, DateTime expiry)
        {
            var expiryTimeSpan = expiry.Subtract(DateTime.UtcNow);

            return await _redis.StringSetAsync(key, JsonSerializer.Serialize(data), expiryTimeSpan);
        }
    }
}
