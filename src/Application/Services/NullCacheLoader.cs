using Application.Configurations;
using Core.Configuration;
using Core.Contracts;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.IO.Compression;
using System.Text.Json;

namespace Application.Services
{
    /// <summary>
    /// Used for testing.
    /// </summary>
    public class NullCacheLoader : ICacheLoader
    {
        private static readonly string _loggingPrefix = $"{DateTime.UtcNow.Month}_{DateTime.UtcNow.Day}_{DateTime.UtcNow.Year}";
        private readonly ILogger _logger;
        private readonly ConcurrentDictionary<string, object> _inMemoryCache = [];
        private readonly string _localCacheLocation;
        private readonly string _bloblCacheKey;

        /// <summary>
        /// Used to develop
        /// </summary>
        /// <param name="memOptions">MemoryCacheSettings options.</param>
        /// <param name="ezOptions">EzLeadSettings options.</param>
        /// <param name="logger">Logger</param>
        public NullCacheLoader(
            IOptions<MemoryCacheSettings> memOptions,
            IOptions<EzLeadSettings> ezOptions,
            ILogger<NullCacheLoader> logger)
        {
            _bloblCacheKey = string.IsNullOrEmpty(memOptions.Value.CacheKey) ? $"{_loggingPrefix.TrimEnd('/')}/FileName.{ezOptions.Value.LoggingBlobName}".TrimStart('/') : memOptions.Value.CacheKey;
            _localCacheLocation = Path.GetTempFileName();
            _logger = logger;

            var localCache = ezOptions.Value.LoggingLocalCache;
            if (File.Exists(localCache) && new FileInfo(localCache).Length > 0)
            {
                try
                {
                    var bytes = File.ReadAllBytes(localCache);
                    using var ms = new MemoryStream(bytes);
                    using var gzip = new GZipStream(ms, CompressionMode.Decompress);
                    using var reader = new StreamReader(gzip);
                    var json = reader.ReadToEnd();
                    var cache = JsonSerializer.Deserialize<IDictionary<string, object>>(json);
                    if (cache is not null)
                    {
                        foreach (var item in cache.ToDictionary())
                        {
                            PutAsync(item.Key, item.Value);
                        }
                    }
                    SaveCacheAsync(cache);
                    _logger.LogInformation("Loading from file system {LocalCacheLocation}", localCache);
                }
                catch (JsonException ex)
                {
                    _logger.LogError(ex, "Failed to deserialize cache file {LocalCacheLocation}", localCache);
                }
            }
            _logger.LogInformation("CacheKey{_bloblCacheKey}", _bloblCacheKey);
        }

        /// <summary>
        /// Asynchronously loads the cache data from a remote source never storage waiting to be saved.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains a dictionary of cache data,
        /// where the keys are strings and the values are objects. If no data is available, an empty dictionary is
        /// returned.</returns>
        public Task<IDictionary<string, object>> LoadCacheAsync()
        {
            return Task.FromResult(result: (IDictionary<string, object>)_inMemoryCache.ToDictionary());
        }

        /// <summary>
        /// Asynchronously stores a value in the cache with the specified key and optional expiration time.
        /// </summary>
        /// <param name="key">The unique key used to identify the cached value. Cannot be null or empty.</param>
        /// <param name="value">The value to store in the cache. Cannot be null.</param>
        /// <param name="absoluteExpiration">An optional expiration time for the cached value. If specified, the value will expire after the given
        /// duration. If null, the value will not have an expiration time.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public Task PutAsync<T>(string key, T value, TimeSpan? absoluteExpiration = null)
        {
            if (!string.IsNullOrEmpty(key) && value is not null)
            {
                _inMemoryCache[key] = value;
            }
            return Task.CompletedTask;
        }

        /// <summary>
        /// Saves the current cache data to a remote storage location.
        /// </summary>
        /// <remarks>The cache data is serialized to JSON format before being saved. Ensure that the
        /// provided dictionary contains  serializable objects to avoid serialization errors.</remarks>
        /// <param name="cache">An optional dictionary containing the cache data to save. If <see langword="null"/>, an empty cache will be
        /// saved.</param>
        /// <returns>A task that represents the asynchronous save operation.</returns>
        public Task SaveCacheAsync(IDictionary<string, object>? cache)
        {
            cache ??= _inMemoryCache.ToDictionary();
            var json = JsonSerializer.Serialize(cache);
            File.WriteAllText(_localCacheLocation, json);

            return Task.CompletedTask;
        }
    }
}
