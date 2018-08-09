namespace Framework.Server
{
    using Framework.Application;
    using Framework.Config;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.DependencyInjection;
    using System;

    public static class Startup
    {
        public static void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>(); // Needed for IIS. Otherwise new HttpContextAccessor(); results in null reference exception.

            services.AddDistributedMemoryCache();
            services.AddSession(options =>
            {
                options.Cookie.Name = "FrameworkSession";
                options.IdleTimeout = TimeSpan.FromSeconds(5);
            });
            services.AddCors();
        }

        public static void Configure(IApplicationBuilder app, AppSelector appSelector)
        {
            UtilServer.App = app;

            ConfigFramework.Init();

            if (UtilServer.IsIssServer == false)
            {
                if (ConfigFramework.Load().IsServerSideRendering)
                {
                    UtilServer.StartUniversalServer();
                }
            }

            if (ConfigFramework.Load().IsUseDeveloperExceptionPage)
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseDefaultFiles(); // Used for index.html
            app.UseStaticFiles(); // Enable access to files in folder wwwwroot.
            app.UseSession();
            app.UseCors(config => config.AllowAnyOrigin().AllowCredentials()); // Access-Control-Allow-Origin. Client POST uses withCredentials to pass cookies!

            app.Run(new Request(app, appSelector).Run);
        }
    }
}
