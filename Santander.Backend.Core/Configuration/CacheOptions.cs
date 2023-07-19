using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Santander.Backend.Core.Configuration
{
    public class CacheOptions
    {
        public const string Key = "Cache";

        public int HackerNewsStoryCacheDuration { get; set; } = 0;
        public int RestApiGetRequestOutputCacheDuration { get; set; } = 0;
    }
}
