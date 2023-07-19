using Microsoft.Extensions.Logging;
using Moq;
using Santander.Api.Web.Query;
using Santander.Api.Web.QueryHandler;
using Santander.Backend.Core.Domain;
using Santander.Backend.Core.Interface;

namespace Santander.Api.Tests.HandlerTests
{
    public class GetBestStoriesQueryHandlerTests
    {
        [Fact]
        public async void Test_Handle_Returns_Empty_Collection_On_Invalid_Request()
        {
            var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.CancelAfter(120000);
            var token = cancellationTokenSource.Token;
            var query = new GetBestStoriesQuery { StoryCount = 0 };

            var cache = new Mock<IDataCache>();
            var svc = new Mock<IHackerNewsService>();
            var logger = new Mock<ILogger<GetBestStoriesHandler>>();

            var handler = new GetBestStoriesHandler(cache.Object, svc.Object, logger.Object);

            var res = await handler.Handle(query, token);
            Assert.NotNull(res);
            Assert.Empty(res);
        }


        [Fact]
        public async void Test_Handle_Returns_Empty_Collection_On_Request_Cancellation_And_Does_Not_Call_Cache_Or_HackerService()
        {
            var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.CancelAfter(1);
            var token = cancellationTokenSource.Token;
            var query = new GetBestStoriesQuery { StoryCount = 0 };

            var cache = new Mock<IDataCache>();
            var svc = new Mock<IHackerNewsService>();
            var logger = new Mock<ILogger<GetBestStoriesHandler>>();

            var handler = new GetBestStoriesHandler(cache.Object, svc.Object, logger.Object);

            var res = await handler.Handle(query, token);
            Assert.NotNull(res);
            Assert.Empty(res);

            svc.Verify(x => x.GetBestStoryIds(), Times.Never);
            svc.Verify(x => x.GetStory(It.IsAny<int>()), Times.Never);
            cache.Verify(x => x.GetHackerNewsStory(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async void Test_Handle_AlwaysCalls_HackerService_GetStoriesIds_Once_OnValidRequest()
        {
            var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.CancelAfter(120000);
            var token = cancellationTokenSource.Token;
            var query = new GetBestStoriesQuery { StoryCount = 100 };

            var cache = new Mock<IDataCache>();
            var svc = new Mock<IHackerNewsService>();
            var logger = new Mock<ILogger<GetBestStoriesHandler>>();

            var handler = new GetBestStoriesHandler(cache.Object, svc.Object, logger.Object);

            var res = await handler.Handle(query, token);
            svc.Verify(x => x.GetBestStoryIds(), Times.Exactly(1));
        }

        [Fact]
        public async void Test_Handle_DoesNotCall_GetStory_OnCache_Or_HackerService_If_NoStoryIds_Are_Returned_By_HackerService()
        {
            var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.CancelAfter(120000);
            var token = cancellationTokenSource.Token;
            var query = new GetBestStoriesQuery { StoryCount = 100 };

            var cache = new Mock<IDataCache>();
            var svc = new Mock<IHackerNewsService>();
            var logger = new Mock<ILogger<GetBestStoriesHandler>>();

            svc.Setup(x => x.GetBestStoryIds()).ReturnsAsync(Enumerable.Empty<int>());

            var handler = new GetBestStoriesHandler(cache.Object, svc.Object, logger.Object);

            var res = await handler.Handle(query, token);
            svc.Verify(x => x.GetBestStoryIds(), Times.Exactly(1));

            Assert.Empty(res);
            svc.Verify(x => x.GetStory(It.IsAny<int>()), Times.Never);
            cache.Verify(x => x.GetHackerNewsStory(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async void Test_Handle_DoesNotCall_GetStory_On_HackerService_If_Story_Is_In_The_Cache()
        {
            var testStoryIdData =  new[] { 1, 2, 3 };

            var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.CancelAfter(120000);
            var token = cancellationTokenSource.Token;
            var query = new GetBestStoriesQuery { StoryCount = 100 };

            var cache = new Mock<IDataCache>();
            var svc = new Mock<IHackerNewsService>();
            var logger = new Mock<ILogger<GetBestStoriesHandler>>();

            svc.Setup(x => x.GetBestStoryIds()).ReturnsAsync(testStoryIdData);
            cache.Setup(x => x.GetHackerNewsStory(It.IsAny<int>()))
                  .ReturnsAsync((int x) => new HackerNewsStory 
                  { 
                      Id = x,
                      Time = DateTimeOffset.Now.ToUnixTimeSeconds(),
                      By = "test",
                      Descendants = 100,
                      Kids = new List<int> { 110, 120 },
                      Score = 900,
                      Title = "test",
                      Type = "test",
                      Url = "https://test/test/22"        
                  });

            var handler = new GetBestStoriesHandler(cache.Object, svc.Object, logger.Object);

            var res = await handler.Handle(query, token);
            
            svc.Verify(x => x.GetBestStoryIds(), Times.Exactly(1));
            Assert.Equal(testStoryIdData.Length, res.Count());           
            cache.Verify(x => x.GetHackerNewsStory(It.IsAny<int>()), Times.Exactly(testStoryIdData.Length));
            svc.Verify(x => x.GetStory(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async void Test_Handle_AlwaysCalls_GetStory_On_HackerService_If_Story_Is_Not_In_The_Cache()
        {
            var testStoryIdData = new[] { 10, 20 };

            var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.CancelAfter(120000);
            var token = cancellationTokenSource.Token;
            var query = new GetBestStoriesQuery { StoryCount = 10 };

            var cache = new Mock<IDataCache>();
            var svc = new Mock<IHackerNewsService>();
            var logger = new Mock<ILogger<GetBestStoriesHandler>>();

            svc.Setup(x => x.GetBestStoryIds()).ReturnsAsync(testStoryIdData);
            cache.Setup(x => x.GetHackerNewsStory(It.IsAny<int>()))
                  .ReturnsAsync(default(HackerNewsStory?));

            svc.Setup(x => x.GetStory(It.IsAny<int>()))
                  .ReturnsAsync((int x) => new HackerNewsStory
                  {
                      Id = x,
                      Time = DateTimeOffset.Now.ToUnixTimeSeconds(),
                      By = "test",
                      Descendants = 8762,
                      Kids = new List<int> { 278, 867 },
                      Score = 1899,
                      Title = "test",
                      Type = "test",
                      Url = "https://test/testsvc/2323"
                  });

            var handler = new GetBestStoriesHandler(cache.Object, svc.Object, logger.Object);

            var res = await handler.Handle(query, token);

            svc.Verify(x => x.GetBestStoryIds(), Times.Exactly(1));
            Assert.Equal(testStoryIdData.Length, res.Count());
            cache.Verify(x => x.GetHackerNewsStory(It.IsAny<int>()), Times.Exactly(testStoryIdData.Length));
            svc.Verify(x => x.GetStory(It.IsAny<int>()), Times.Exactly(testStoryIdData.Length));
        }
    }
}