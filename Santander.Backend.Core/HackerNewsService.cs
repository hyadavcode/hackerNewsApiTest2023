using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Santander.Backend.Core.Configuration;
using Santander.Backend.Core.Domain;
using Santander.Backend.Core.Interface;
using System.Net.Http;
using System.Text.Json;

namespace Santander.Backend.Core
{
    public class HackerNewsService : IHackerNewsService
    {
        private readonly ILogger<HackerNewsService> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly HackerNewsExternalApiOptions _hackerNewsApiConfig;
        private HttpClient? _httpClient;

        private HttpClient HackerNewsApiClient
        {
            get
            {
                if(_httpClient == null)
                {
                    _httpClient = _httpClientFactory.CreateClient();
                    _httpClient.BaseAddress = new Uri(_hackerNewsApiConfig.BaseUrl);
                }

                return _httpClient;
            }
        }

        // ctor
        public HackerNewsService(IHttpClientFactory httpClientFactory,
            IOptions<HackerNewsExternalApiOptions> hackerNewsApiConfig,
            ILogger<HackerNewsService> logger)
        {
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _hackerNewsApiConfig = hackerNewsApiConfig?.Value ?? throw new ArgumentNullException(nameof(hackerNewsApiConfig));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IEnumerable<int>> GetBestStoryIds()
        {
            try
            {
                var client = HackerNewsApiClient;
                client.DefaultRequestHeaders.Add("Accept", "application/json");
  
                var response = await client.GetStreamAsync($"/{_hackerNewsApiConfig.BestStoriesIdsUrl}");
                var result = await JsonSerializer.DeserializeAsync<IEnumerable<int>>(response);

                if(result == null)
                    throw new InvalidDataException("HackerNews external API returned no data");

                return result;

            }
            catch(Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return Enumerable.Empty<int>();
            }
        }

        public async Task<HackerNewsStory?> GetStory(int storyId)
        {
            try
            {
                var client = HackerNewsApiClient;
                client.DefaultRequestHeaders.Add("Accept", "application/json");

                var response = await client.GetStreamAsync($"/{_hackerNewsApiConfig.StoryUrl}/{storyId}.json");

                var jsonOptions = new JsonSerializerOptions
                {
                    AllowTrailingCommas = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                var result = await JsonSerializer.DeserializeAsync<HackerNewsStory?>(response, jsonOptions);

                if (result == null)
                    throw new InvalidDataException($"HackerNews external API returned no data for story id = {storyId}");

                return result;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return null;
            }
        }
    }
}