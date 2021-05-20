namespace Framework.Server
{
    using Framework.Config;
    using Framework.DataAccessLayer.DatabaseMemory;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Rewrite;
    using Microsoft.Extensions.DependencyInjection;
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// ASP.NET Core configuration.
    /// </summary>
    public static class StartupFramework
    {
        internal const string CookieName = "FrameworkSession"; // Session cookie.

        public static void ConfigureServices(IServiceCollection services)
        {
            // Dependency Injection DI. See also https://docs.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection?view=aspnetcore-2.1
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>(); // Needed for IIS. Otherwise new HttpContextAccessor(); results in null reference exception.
            services.AddSingleton<DatabaseMemoryInternal>();

            services.AddDistributedMemoryCache();
            services.AddSession(options =>
            {
                options.Cookie.Name = CookieName;
                options.IdleTimeout = TimeSpan.FromSeconds(5 * 60); // Session expire 5 minutes
            });

            services.AddSingleton<BackgroundFrameworkService>();
            services.AddHostedService(provider => provider.GetService<BackgroundFrameworkService>());
        }

        public static void Configure(IApplicationBuilder applicationBuilder)
        {
            UtilServer.ApplicationBuilder = applicationBuilder;

            var configServer = ConfigServer.Load();

            if (UtilServer.IsIssServer == false)
            {
                // Running in Visual Studio environment.
                if (configServer.IsServerSideRendering)
                {
                    UtilServer.StartUniversalServerAngular();
                }
            }

            if (configServer.IsUseDeveloperExceptionPage)
            {
                applicationBuilder.UseDeveloperExceptionPage();
            }

            applicationBuilder.UseDefaultFiles(); // Used for index.html
            applicationBuilder.UseStaticFiles(); // Enable access to files in folder wwwwroot.
            applicationBuilder.UseSession();

            // IsRedirectWww
            if (configServer.IsRedirectWww)
            {
                // Rewrite for example workplacex.org to www.workplacex.org
                // Do not rewrite for example demo.workplacex.org
                // See also: https://docs.microsoft.com/en-us/aspnet/core/fundamentals/url-rewriting?view=aspnetcore-5.0
                var domainNameList = new List<string>();
                foreach (var website in configServer.WebsiteList)
                {
                    foreach (var domainName in website.DomainNameList)
                    {
                        if (domainName.DomainName.StartsWith("www."))
                        {
                            domainNameList.Add(domainName.DomainName.Substring("www.".Length));
                        }
                    }
                }
                var options = new RewriteOptions();
                if (domainNameList.Count > 0)
                {
                    options = options.AddRedirectToWww(domainNameList.ToArray());
                }
                // options = options.AddRedirect("(.*[^/])$", "$1/"); // Enforce trailing slash. For example /path becomes /path/ // Be aware! /abc.js becomes /abc.js/
                applicationBuilder.UseRewriter(options);
            }

            // IsRedirectHttps
            if (configServer.IsRedirectHttps)
            {
                // Enforce HTTPS in ASP.NET Core https://docs.microsoft.com/en-us/aspnet/core/security/enforcing-ssl?view=aspnetcore-5.0&tabs=visual-studio
                applicationBuilder.UseHsts();
                applicationBuilder.UseHttpsRedirection();
            }

            applicationBuilder.Run(new Request().RunAsync);
        }
    }
}
