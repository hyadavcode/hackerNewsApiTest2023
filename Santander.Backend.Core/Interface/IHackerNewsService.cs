using Santander.Backend.Core.Domain;

namespace Santander.Backend.Core.Interface
{
    public interface IHackerNewsService
    {
        Task<HackerNewsStory?> GetStory(int storyId);
        Task<IEnumerable<int>> GetBestStoryIds();
    }
}