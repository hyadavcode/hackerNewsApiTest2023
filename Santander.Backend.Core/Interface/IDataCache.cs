using Santander.Backend.Core.Domain;

namespace Santander.Backend.Core.Interface
{
    public interface IDataCache
    {
        Task<HackerNewsStory?> GetHackerNewsStory(int storyId);
        Task SaveHackerNewsStory(HackerNewsStory dto);
    }
}