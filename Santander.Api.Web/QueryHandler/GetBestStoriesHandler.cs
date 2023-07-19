using MediatR;
using Santander.Api.Web.Extensions;
using Santander.Api.Web.Models;
using Santander.Api.Web.Query;
using Santander.Backend.Core.Domain;
using Santander.Backend.Core.Interface;

namespace Santander.Api.Web.QueryHandler
{
    public class GetBestStoriesHandler : IRequestHandler<GetBestStoriesQuery, IEnumerable<Story>>
    {
        private readonly IDataCache _cache;
        private readonly IHackerNewsService _hackerNewsService;
        private readonly ILogger<GetBestStoriesHandler> _logger;

        // ctor
        public GetBestStoriesHandler(IDataCache cache, 
            IHackerNewsService hackerNewsService, ILogger<GetBestStoriesHandler> logger)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _hackerNewsService = hackerNewsService ?? throw new ArgumentNullException(nameof(hackerNewsService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IEnumerable<Story>> Handle(GetBestStoriesQuery request, CancellationToken cancellationToken)
        {
            try
            {
                if(request.StoryCount <= 0)
                    return Enumerable.Empty<Story>();

                if (cancellationToken.IsCancellationRequested)
                    return Enumerable.Empty<Story>();

                var bestStoryIds = await _hackerNewsService.GetBestStoryIds();

                if(!bestStoryIds.Any())
                    return Enumerable.Empty<Story>();

                var storyTasks = bestStoryIds.Select(async x => await GetStory(x, cancellationToken))
                                            .ToArray();
                                            
                Task.WaitAll(storyTasks);

                if(storyTasks.All(x => x.IsCompletedSuccessfully))
                {
                    var result = storyTasks.Select(x => x.Result)
                                        .Where(x => x != null)
                                        .OrderByDescending(x => x.Score)
                                        .Take(request.StoryCount)
                                        .Select(x => x.ToModel())
                                        .AsEnumerable();
                 
                    return result;
                }    
                else
                {
                    // Log trace for cancellations
                    if (storyTasks.Any(x => x.IsCanceled))
                        _logger.LogTrace($"Cancellation was requested during the execution of {nameof(GetBestStoriesHandler.Handle)}");

                    // Log error for exceptions
                    foreach(var failedTask in storyTasks.Where(x => x.IsFaulted))
                        _logger.LogError($"Exception or error was encountered in during the execution of {nameof(GetBestStoriesHandler.Handle)} on getting a story. {failedTask?.Exception?.Message}");

                    return Enumerable.Empty<Story>();
                }
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return Enumerable.Empty<Story>();
            }
        }

        private async Task<HackerNewsStory?> GetStory(int storyId, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
                return null;

            var story = await _cache.GetHackerNewsStory(storyId);

            if (story != null)
                return story;

            if (cancellationToken.IsCancellationRequested)
                return null;

            story = await _hackerNewsService.GetStory(storyId);
            
            if(story != null)
            {
                // save to cache but, don't wait on the async call to finish and move on  
                _ = _cache.SaveHackerNewsStory(story);
            }

            return story;
        }

    }
}
