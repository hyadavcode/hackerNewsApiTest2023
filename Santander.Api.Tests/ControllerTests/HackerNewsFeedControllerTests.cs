using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Santander.Api.Web.Controllers;
using Santander.Api.Web.Models;
using Santander.Backend.Core.Configuration;

namespace Santander.Api.Tests.ControllerTests
{
    public class HackerNewsFeedControllerTests
    {
        [Fact]
        public async Task GetBestStories_Returns_BadRequest_OnInvalidStoryCount_Arguments()
        {
            var restApiOptionTestData = new RestApiOptions
            {
                GetRequestDefaultTimeout = 120000
            };

            var sender = new Mock<MediatR.ISender>();
            var config = new Mock<IOptions<RestApiOptions>>();
            var logger = new Mock<ILogger<HackerNewsFeedController>>();

            sender.Setup(x => x.Send(It.IsAny<object>(), It.IsAny<CancellationToken>()));
            config.Setup(x => x.Value).Returns(() => restApiOptionTestData);

            var ctr = new HackerNewsFeedController(sender.Object, config.Object, logger.Object);

            var result = await ctr.GetBestStories(-10);
            Assert.NotNull(result?.Result);
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public async Task GetBestStories_Returns_NotFound_WhenNoDataAvailable()
        {
            var restApiOptionTestData = new RestApiOptions
            {
                GetRequestDefaultTimeout = 120000
            };

            var sender = new Mock<MediatR.ISender>();
            var config = new Mock<IOptions<RestApiOptions>>();
            var logger = new Mock<ILogger<HackerNewsFeedController>>();

            sender.Setup(x => x.Send(It.IsAny<IRequest<IEnumerable<Story>>>(), It.IsAny<CancellationToken>()))
                  .ReturnsAsync(Enumerable.Empty<Story>());

            config.Setup(x => x.Value).Returns(() => restApiOptionTestData);

            var ctr = new HackerNewsFeedController(sender.Object, config.Object, logger.Object);

            var result = await ctr.GetBestStories(10);
            Assert.NotNull(result?.Result);
            Assert.IsType<NotFoundResult>(result.Result);

            sender.Reset();
        }

        [Fact]
        public async Task GetBestStories_Returns_CorrectData_WhenAvailable_And_Calls_Mediator_Send()
        {
            var restApiOptionTestData = new RestApiOptions
            {
                GetRequestDefaultTimeout = 120000
            };

            var testStoryData = new[]
            {
                new Story{ Id  = 23, Title = "test2"},
                new Story{ Id  = 24, Title = "test3"},
                new Story{ Id  = 25, Title = "test4"}
            };

            var sender = new Mock<ISender>();
            var config = new Mock<IOptions<RestApiOptions>>();
            var logger = new Mock<ILogger<HackerNewsFeedController>>();

            sender.Setup(x => x.Send(It.IsAny<IRequest<IEnumerable<Story>>>(), It.IsAny<CancellationToken>()))                    
                  .ReturnsAsync(testStoryData);

            config.Setup(x => x.Value).Returns(() => restApiOptionTestData);

            var ctr = new HackerNewsFeedController(sender.Object, config.Object, logger.Object);

            var result = await ctr.GetBestStories(10);
            Assert.NotNull(result?.Result);
            Assert.IsType<OkObjectResult>(result.Result);

            var okResult = (OkObjectResult)result.Result;
            var stories = (IEnumerable<Story>)okResult.Value;
            Assert.True(stories.All(x => testStoryData.Any(y => y.Id == x.Id)));

            sender.Verify(x => x.Send(It.IsAny<IRequest<IEnumerable<Story>>>(), It.IsAny<CancellationToken>()), Times.Exactly(1));
        }
    }
}