using MediatR;
using Santander.Api.Web.Models;

namespace Santander.Api.Web.Query
{
    public class GetBestStoriesQuery : IRequest<IEnumerable<Story>> 
    {
        public int StoryCount { get; set; } = 0;
    }
}
