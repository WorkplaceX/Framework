namespace Framework.Server
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Caching.Memory;

    public abstract class WebControllerBase : Controller
    {
        public WebControllerBase(IMemoryCache memoryCache)
        {
            this.MemoryCache = memoryCache;
        }

        public readonly IMemoryCache MemoryCache;
    }
}
