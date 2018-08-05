namespace Framework.Server
{
    using Framework.Component;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    public static class Startup
    {
        public static void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            ConfigFramework.Init(env);

            if (UtilServer.IsIssServer == false)
            {
                if (ConfigFramework.Load().IsServerSideRendering)
                {
                    UtilServer.StartUniversalServer();
                }
            }

            // if (env.IsDevelopment()) // TODO ConfigFramework
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseDefaultFiles(); // Used for index.html
            app.UseStaticFiles(); // Enable access to files in folder wwwwroot.

            app.Run(new StartupRun(app, env).Run);
        }
    }

    internal class StartupRun
    {
        public StartupRun(IApplicationBuilder app, IHostingEnvironment env)
        {
            this.App = app;
            this.Env = env;
        }

        public readonly IApplicationBuilder App;

        public readonly IHostingEnvironment Env;

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
        /// <param name="isIndexEmpty">Use file indexEmpty.html</param>
        private async Task ServerSideRendering(HttpContext context, string path, bool isIndexEmpty, IHostingEnvironment env = null)
        {
            context.Response.ContentType = UtilServer.ContentType(path);
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
            string isIndexEmptyHtml = isIndexEmpty ? "true" : "false";
            url += "?IsIndexEmptyHtml=" + isIndexEmptyHtml;
            App app = new App(null);
            string json = UtilJson.Serialize(app);
            string htmlServerSideRendering = await UtilServer.WebPost(url, json);

            if (isIndexEmpty)
            {
                htmlServerSideRendering = UtilFramework.Replace(htmlServerSideRendering, "<html><head><title>Empty</title><style ng-transition=\"Application\"></style></head><body>", "");
                htmlServerSideRendering = UtilFramework.Replace(htmlServerSideRendering, "</body></html>", "");
            }

            // Set jsonBrowser in html.
            string scriptFind = "var jsonBrowser = {};";
            string scriptReplace = "var jsonBrowser = " + json + ";";
            htmlServerSideRendering = UtilFramework.Replace(htmlServerSideRendering, scriptFind, scriptReplace);

            context.Response.ContentType = UtilServer.ContentType(path);
            await context.Response.WriteAsync(htmlServerSideRendering);
        }

        private static async Task<bool> ServeFrameworkFileWebsite(HttpContext context, string path, IHostingEnvironment env)
        {
            bool result = false;
            var configFramework = ConfigFramework.Load(env);
            var website = configFramework.WebsiteList.FirstOrDefault();
            if (website != null)
            {
                string fileName = UtilServer.FolderNameContentRoot(env) + "Framework/Website/" + website.DomainName + path;
                if (File.Exists(fileName))
                {
                    context.Response.ContentType = UtilServer.ContentType(fileName);
                    await context.Response.SendFileAsync(fileName);
                    result = true;
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
            bool result = await ServeFrameworkFileWebsite(context, path, Env);
            if (result)
            {
                return true;
            }

            bool requestIsFileName = UtilServer.PathIsFileName(path);

            if (path.EndsWith("/index.html") && ConfigFramework.Load(Env).IsServerSideRendering)
            {
                await ServerSideRendering(context, path, false, Env);
                return true;
            }
            else
            {
                if (requestIsFileName)
                {
                    // Serve fileName
                    string fileName = UtilServer.FolderNameContentRoot(Env) + "Framework/dist/browser" + path;
                    if (File.Exists(fileName))
                    {
                        context.Response.ContentType = UtilServer.ContentType(fileName);
                        await context.Response.SendFileAsync(fileName);
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
