namespace Framework.Server
{
    using Framework.Config;
    using Framework.DataAccessLayer.DatabaseMemory;
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
            services.AddSingleton<DatabaseMemoryInternal>();

            services.AddDistributedMemoryCache();
            services.AddSession(options =>
            {
                options.Cookie.Name = "FrameworkSession";
                options.IdleTimeout = TimeSpan.FromSeconds(60);
            });
        }

        public static void Configure(IApplicationBuilder applicationBuilder)
        {
            UtilServer.ApplicationBuilder = applicationBuilder;

            if (UtilServer.IsIssServer == false)
            {
                // Running in Visual Studio environment.
                if (ConfigServer.Load().IsServerSideRendering)
                {
                    UtilServer.StartUniversalServer();
                }
            }

            if (ConfigServer.Load().IsUseDeveloperExceptionPage)
            {
                applicationBuilder.UseDeveloperExceptionPage();
            }

            applicationBuilder.UseDefaultFiles(); // Used for index.html
            applicationBuilder.UseStaticFiles(); // Enable access to files in folder wwwwroot.
            applicationBuilder.UseSession();

            // Enforce HTTPS in ASP.NET Core https://docs.microsoft.com/en-us/aspnet/core/security/enforcing-ssl?view=aspnetcore-5.0&tabs=visual-studio
            applicationBuilder.UseHsts();
            applicationBuilder.UseHttpsRedirection();

            applicationBuilder.Run(new Request().RunAsync);
        }
    }
}
