using Santander.Api.Web.Models;
using Santander.Backend.Core.Domain;

namespace Santander.Api.Web.Extensions
{
    public static class MappingExtensions
    {
        public static Story? ToModel(this HackerNewsStory dto)
        {
            if (dto == null)
                return null;

            var result = new Story
            {
                By = dto.By,
                Descendants = dto.Descendants,
                Id = dto.Id,
                Kids = new List<int>(dto.Kids),
                Score = dto.Score,
                Time = DateTimeOffset.FromUnixTimeSeconds(dto.Time).LocalDateTime,
                Title = dto.Title,
                Type = dto.Type,
                Url = dto.Url,
            };

            return result;
        }
    }
}
