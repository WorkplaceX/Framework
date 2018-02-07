namespace Server
{
    using Framework;
    using Framework.Server;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using System.Diagnostics;

    /// <summary>
    /// Derived class Startup has to be declared in Server assembly.
    /// </summary>
    public abstract class StartupBase
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit http://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
            services.AddMemoryCache();
            services.AddSingleton(new UnitTestService());
            //
            if (Debugger.IsAttached)
            {
                UtilServer.StartUniversalServer();
            }
        }

        private static bool debugIsException = true; // Enable exception page. // If running on IIS make sure web.config contains: arguments="Server.dll" if you get HTTP Error 502.5 - Process Failure

        public const string ControllerPath = "/"; // "/web/"; // Enable debug mode. Path when WebController kicks in.

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole();

            if (debugIsException)
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStaticFiles(); // Enable access to files in folder wwwwroot.

            app.UseMvc(); // Enable WebController.

            app.Run(async (context) => // Fallback if no URL matches.
            {
                await context.Response.WriteAsync("<html><head><title></title></head><body><h1>Debug</h1><a href='web/config/'>Config</a><br /><a href='web/demo/'>Demo</a></body></html>"); 
            });
        }
    }
}
