namespace Framework.Server
{
    using Framework.Application;
    using Framework.Config;
    using Framework.Dal.Memory;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.DependencyInjection;
    using System;

    /// <summary>
    /// ASP.NET Core configuration.
    /// </summary>
    public static class StartupFramework
    {
        public static void ConfigureServices(IServiceCollection services)
        {
            // Dependency Injection DI. See also https://docs.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection?view=aspnetcore-2.1
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>(); // Needed for IIS. Otherwise new HttpContextAccessor(); results in null reference exception.
            services.AddScoped<UtilServer.InstanceService, UtilServer.InstanceService>(); // Singleton per request.
            services.AddSingleton<MemoryInternal>();

            services.AddDistributedMemoryCache();
            services.AddSession(options =>
            {
                options.Cookie.Name = "FrameworkSession";
                options.IdleTimeout = TimeSpan.FromSeconds(60);
            });
        }

        public static void Configure(IApplicationBuilder applicationBuilder, AppSelector appSelector)
        {
            UtilServer.ApplicationBuilder = applicationBuilder;

            if (UtilServer.IsIssServer == false)
            {
                if (ConfigFramework.Load().IsServerSideRendering)
                {
                    UtilServer.StartUniversalServer();
                }
            }

            if (ConfigFramework.Load().IsUseDeveloperExceptionPage)
            {
                applicationBuilder.UseDeveloperExceptionPage();
            }

            applicationBuilder.UseDefaultFiles(); // Used for index.html
            applicationBuilder.UseStaticFiles(); // Enable access to files in folder wwwwroot.
            applicationBuilder.UseSession();

            applicationBuilder.Run(new Request(applicationBuilder, appSelector).RunAsync);
        }
    }
}
