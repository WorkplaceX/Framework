namespace Framework.Server
{
    using Framework.Component;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.DependencyInjection;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    public static class Startup
    {
        public static void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>(); // Needed for IIS. Otherwise new HttpContextAccessor(); results in null reference exception.
        }

        public static void Configure(IApplicationBuilder app)
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

            app.Run(new StartupRun(app).Run);
        }
    }

    internal class StartupRun
    {
        public StartupRun(IApplicationBuilder app)
        {
            this.App = app;
        }

        public readonly IApplicationBuilder App;

        public async Task Run(HttpContext context)
        {
            bool result = await ServeFrameworkFile(context);

            if (result == false)
            {
                context.Response.StatusCode = 404; // Not found
            }
            return;
        }

        /// <summary>
        /// Render first html request on server.
        /// </summary>
        private static async Task<string> ServerSideRendering(HttpContext context, string indexHtml)
        {
            string result = indexHtml;
            if (result.Contains("<data-app></data-app>")) // Needs server sie rendering
            {
                string url;
                if (UtilServer.IsIssServer)
                {
                    // Running on IIS Server.
                    url = "http://" + context.Request.Host.ToUriComponent() + "/Framework/dist/server.js";
                }
                else
                {
                    // Running in Visual Studio
                    url = "http://localhost:4000/"; // Call Universal server when running in Visual Studio.
                }
                App app = new App(null);
                string json = UtilJson.Serialize(app);
                string htmlServerSideRendering = await UtilServer.WebPost(url, json);

                htmlServerSideRendering = UtilFramework.Replace(htmlServerSideRendering, "<html><head><style ng-transition=\"Application\"></style></head><body>", "");
                htmlServerSideRendering = UtilFramework.Replace(htmlServerSideRendering, "</body></html>", "");

                result = UtilFramework.Replace(result, "<data-app></data-app>", htmlServerSideRendering);

                // Set jsonBrowser in html.
                string scriptFind = "var jsonBrowser = {};";
                string scriptReplace = "var jsonBrowser = " + json + ";";
                result = UtilFramework.Replace(result, scriptFind, scriptReplace);
            }
            return result;
        }

        private static async Task<bool> ServeFrameworkFileWebsite(HttpContext context, string path)
        {
            bool result = false;
            var configFramework = ConfigFramework.Load();
            var website = configFramework.WebsiteList.FirstOrDefault();
            if (website != null)
            {
                string fileName = UtilServer.FolderNameContentRoot() + "Framework/Website/" + website.DomainName + path;
                if (File.Exists(fileName))
                {
                    context.Response.ContentType = UtilServer.ContentType(fileName);
                    if (fileName.EndsWith(".html") && ConfigFramework.Load().IsServerSideRendering)
                    {
                        string htmlIndex = File.ReadAllText(fileName);
                        htmlIndex = await ServerSideRendering(context, htmlIndex);
                        await context.Response.WriteAsync(htmlIndex);
                        result = true;
                    }
                    else
                    {
                        await context.Response.SendFileAsync(fileName);
                        result = true;
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Returns true, if file found in "framework/dist/browser" folder.
        /// </summary>
        private async Task<bool> ServeFrameworkFile(HttpContext context)
        {
            string path = context.Request.Path;

            // index.html
            if (UtilServer.PathIsFileName(path) == false)
            {
                path += "index.html";
            }

            // Website
            bool result = await ServeFrameworkFileWebsite(context, path);
            if (result)
            {
                return true;
            }

            if (UtilServer.PathIsFileName(path))
            {
                // Serve fileName
                string fileName = UtilServer.FolderNameContentRoot() + "Framework/dist/browser" + path;
                if (File.Exists(fileName))
                {
                    context.Response.ContentType = UtilServer.ContentType(fileName);
                    await context.Response.SendFileAsync(fileName);
                    return true;
                }
            }

            return false;
        }
    }
}
