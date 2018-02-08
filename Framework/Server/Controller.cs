namespace Framework.Server
{
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Caching.Memory;

    public abstract class WebControllerBase : Controller
    {
        public WebControllerBase()
        {
            this.MemoryCache = (IMemoryCache)new HttpContextAccessor().HttpContext.RequestServices.GetService(typeof(IMemoryCache));
        }

        public readonly IMemoryCache MemoryCache;
    }
}
