﻿namespace Framework.Server
{
    using Framework.Application;
    using Framework.Config;
    using Microsoft.ApplicationInsights.Extensibility;
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
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>(); // Needed for IIS. Otherwise new HttpContextAccessor(); results in null reference exception.
            services.AddScoped<UtilServer.InstanceService, UtilServer.InstanceService>(); // Singleton per request.
            
            services.AddDistributedMemoryCache();
            services.AddSession(options =>
            {
                options.Cookie.Name = "FrameworkSession";
                options.IdleTimeout = TimeSpan.FromSeconds(60);
            });
            services.AddCors();

            TelemetryConfiguration.Active.DisableTelemetry = true; // Disable "Application Insights Telemetry" logging
        }

        public static void Configure(IApplicationBuilder applicationBuilder, AppSelector appSelector)
        {
            UtilServer.ApplicationBuilder = applicationBuilder;

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
                applicationBuilder.UseDeveloperExceptionPage();
            }

            applicationBuilder.UseDefaultFiles(); // Used for index.html
            applicationBuilder.UseStaticFiles(); // Enable access to files in folder wwwwroot.
            applicationBuilder.UseSession();
            applicationBuilder.UseCors(config => config.AllowAnyOrigin().AllowCredentials()); // Access-Control-Allow-Origin. Client POST uses withCredentials to pass cookies!

            applicationBuilder.Run(new Request(applicationBuilder, appSelector).RunAsync);
        }
    }
}
