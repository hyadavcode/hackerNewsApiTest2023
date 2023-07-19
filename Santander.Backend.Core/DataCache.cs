using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Santander.Backend.Core.Domain;
using Santander.Backend.Core.Interface;

namespace Santander.Backend.Core
{
    public partial class DataCache : IDataCache
    {
        // fields
        private IMemoryCache _memoryCache;
        private readonly ILogger _logger;
        private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1,1);

        // ctor
        public DataCache(IMemoryCache memoryCache, ILogger<DataCache> logger)
        {
            _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));   
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }  

        // private methods
        private string GetKey(string cacheKeyPrefix, object key)
        {
            return $"{cacheKeyPrefix}.{key}";
        }

        // interface methods
        public async Task<HackerNewsStory?> GetHackerNewsStory(int storyId)
        {
            var task = Task.Factory.StartNew(() =>
            {
                var key = GetKey(CacheKeys.HackerNewsStory, storyId);

                if (_memoryCache.TryGetValue(key, out var value))
                    return (HackerNewsStory?)value;
                else
                {
                    _logger.LogTrace($"Cache miss : {nameof(DataCache.GetHackerNewsStory)} : object not found in cache for story id = {storyId}");
                    return null;
                }
            });

            return await task;
        }

        public async Task SaveHackerNewsStory(HackerNewsStory dto)
        {
            var task = Task.Factory.StartNew(() =>
            {
                try
                {
                    if (dto?.Id == null)
                        return Task.FromException(new ArgumentNullException(nameof(dto)));

                    var exists = GetHackerNewsStory(dto.Id);
                    var key = GetKey(CacheKeys.HackerNewsStory, dto.Id);

                    _semaphore.Wait();
                    _memoryCache.Set(key, dto, TimeSpan.FromMinutes(10));
                    return Task.CompletedTask;
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Exception in {nameof(DataCache.SaveHackerNewsStory)}. {ex.Message}", ex);
                    return Task.FromException(ex);
                }
                finally
                {
                    _semaphore.Release();
                }
            });

            await task;           
        }
    }
}