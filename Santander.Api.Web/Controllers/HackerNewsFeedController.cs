using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Santander.Api.Web.Models;
using Santander.Api.Web.Query;
using Santander.Backend.Core.Configuration;
using System;

namespace Santander.Api.Web.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class HackerNewsFeedController : ControllerBase
    {
        private readonly ISender _sender;
        private readonly ILogger<HackerNewsFeedController> _logger;        
        private readonly RestApiOptions? _apiConfig;
        private readonly CancellationTokenSource _tokenSource = new();

        public HackerNewsFeedController(ISender sender, 
            IOptions<RestApiOptions> apiConfig, ILogger<HackerNewsFeedController> logger)
        {
            _sender = sender ?? throw new ArgumentNullException(nameof(sender));
            _apiConfig = apiConfig?.Value ?? throw new ArgumentNullException(nameof(apiConfig));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet("bestStories/{storyCount}")]        
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<Story>>> GetBestStories(int storyCount)
        {
            try
            {
                _tokenSource.CancelAfter(TimeSpan.FromMilliseconds(_apiConfig.GetRequestDefaultTimeout));
                var cancellationToken = _tokenSource.Token;
                
                if(storyCount <= 0)
                    return BadRequest(storyCount);

                var request = new GetBestStoriesQuery { StoryCount = storyCount };
                var result = await _sender.Send(request, cancellationToken);

                if (result != null && result.Any())
                    return Ok(result.ToArray());
                else
                    return NotFound();
            }
            catch(Exception ex)
            {
                _logger.LogError($"Error in {nameof(HackerNewsFeedController.GetBestStories)}. {ex.Message}");
                return StatusCode(500);
            }
            finally
            {
                _tokenSource.TryReset();
            }
        }
    }
}