namespace Santander.Backend.Core.Configuration
{
    public class HackerNewsExternalApiOptions
    {
        public const string Key = "HackerNewsExternalApi";

        public string BaseUrl { get; set; } = string.Empty;
        public string BestStoriesIdsUrl { get; set; } = string.Empty;
        public string StoryUrl { get; set; } = string.Empty;
    }
}
