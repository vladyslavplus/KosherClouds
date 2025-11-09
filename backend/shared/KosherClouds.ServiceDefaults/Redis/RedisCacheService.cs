using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System.Text.Json;

namespace KosherClouds.ServiceDefaults.Redis
{
    public class RedisCacheService : IRedisCacheService
    {
        private readonly IConnectionMultiplexer _multiplexer;
        private readonly IDatabase _db;
        private readonly ILogger<RedisCacheService> _logger;
        private readonly JsonSerializerOptions _jsonOptions;
        private static readonly TimeSpan DefaultExpiration = TimeSpan.FromMinutes(30);

        public RedisCacheService(
            IConnectionMultiplexer multiplexer,
            ILogger<RedisCacheService> logger)
        {
            _multiplexer = multiplexer ?? throw new ArgumentNullException(nameof(multiplexer));
            _db = _multiplexer.GetDatabase();
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            };
        }

        public async Task<T?> GetDataAsync<T>(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                _logger.LogWarning("Redis key is null or empty.");
                return default;
            }

            var data = await _db.StringGetAsync(key);
            if (data.IsNullOrEmpty)
            {
                _logger.LogDebug("Cache miss for key: {Key}", key);
                return default;
            }

            _logger.LogDebug("Cache hit for key: {Key}", key);
            return JsonSerializer.Deserialize<T>(data!, _jsonOptions);
        }

        public async Task SetDataAsync<T>(string key, T data, TimeSpan? expiration = null)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                _logger.LogWarning("Redis key is null or empty.");
                return;
            }

            if (data == null)
            {
                _logger.LogWarning("Attempted to cache null data for key: {Key}", key);
                return;
            }

            var json = JsonSerializer.Serialize(data, _jsonOptions);
            await _db.StringSetAsync(key, json, expiration ?? DefaultExpiration);

            _logger.LogInformation("Data cached for key: {Key} (TTL: {TTL})",
                key, (expiration ?? DefaultExpiration).TotalMinutes);
        }

        public async Task RemoveDataAsync(string key)
        {
            if (string.IsNullOrWhiteSpace(key)) return;

            await _db.KeyDeleteAsync(key);
            _logger.LogInformation("Redis key removed: {Key}", key);
        }

        public async Task AddToSetAsync(string setKey, string value)
        {
            if (string.IsNullOrWhiteSpace(setKey) || string.IsNullOrWhiteSpace(value)) return;
            await _db.SetAddAsync(setKey, value);
        }

        public async Task<IEnumerable<string>> GetSetMembersAsync(string setKey)
        {
            if (string.IsNullOrWhiteSpace(setKey)) return Enumerable.Empty<string>();

            var members = await _db.SetMembersAsync(setKey);
            return members.Select(m => m.ToString());
        }

        public async Task ClearSetAsync(string setKey)
        {
            if (string.IsNullOrWhiteSpace(setKey)) return;
            await _db.KeyDeleteAsync(setKey);
        }

        public async Task RemoveByPatternAsync(string pattern)
        {
            if (string.IsNullOrWhiteSpace(pattern)) return;

            foreach (var endpoint in _multiplexer.GetEndPoints())
            {
                var server = _multiplexer.GetServer(endpoint);
                await foreach (var key in server.KeysAsync(pattern: pattern))
                {
                    await _db.KeyDeleteAsync(key);
                }
            }

            _logger.LogInformation("Removed keys matching pattern: {Pattern}", pattern);
        }
    }
}