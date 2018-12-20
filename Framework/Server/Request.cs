namespace Framework.Server
{
    using Framework.Application;
    using Framework.Config;
    using Framework.Json;
    using Framework.Session;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Http;
    using System;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    internal class Request
    {
        public Request(IApplicationBuilder applicationBuilder, AppSelector appSelector)
        {
            this.ApplicationBuilder = applicationBuilder;
            this.AppSelector = appSelector;
        }

        public readonly IApplicationBuilder ApplicationBuilder;

        public readonly AppSelector AppSelector;

        /// <summary>
        /// Every client request goes through here.
        /// </summary>
        public async Task RunAsync(HttpContext context)
        {
            // await Task.Delay(500); // Simulate slow network.

            UtilServer.Cors();

            // Path init
            string path = context.Request.Path;
            if (UtilServer.PathIsFileName(path) == false)
            {
                path += "index.html";
            }

            // GET Website
            bool result = await WebsiteAsync(context, path, AppSelector); // With server side rendering.
            if (result)
            {
                return;
            }

            // POST app.json
            result = await Post(context, path, AppSelector);
            if (result)
            {
                return;
            }

            // GET dist/browser/
            result = await DistBrowser(context, path);
            if (result)
            {
                return;
            }

            context.Response.StatusCode = 404; // Not found
        }

        /// <summary>
        /// Render first html GET request.
        /// </summary>
        private static async Task<string> ServerSideRenderingAsync(HttpContext context, string indexHtml, AppSelector appSelector)
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
                var app = await appSelector.CreateAppAndProcessAsync(context);  // Process (Server side rendering)
                
                // Serialize
                string json = UtilJson.Serialize(app.AppJson, app.TypeComponentInNamespaceList);
                UtilSession.Serialize(app);

                // Server side render post.
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

        /// <summary>
        /// Divert request to "Framework/Website/"
        /// </summary>
        private static async Task<bool> WebsiteAsync(HttpContext context, string path, AppSelector appSelector)
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
                        string htmlIndex = UtilFramework.FileLoad(fileName);
                        htmlIndex = await ServerSideRenderingAsync(context, htmlIndex, appSelector);
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
        /// Handle client web POST /app.json
        /// </summary>
        private static async Task<bool> Post(HttpContext context, string path, AppSelector appSelector)
        {
            bool result = false;
            if (path == "/app.json")
            {
                var app = await appSelector.CreateAppAndProcessAsync(context); // Process (Client http post)
                context.Response.ContentType = UtilServer.ContentType(path);
                
                // Serialize
                string json = UtilJson.Serialize(app.AppJson, app.TypeComponentInNamespaceList);
                UtilSession.Serialize(app);

                await context.Response.WriteAsync(json);
                result = true;
            }

            return result;
        }

        /// <summary>
        /// Returns true, if file found in "framework/dist/browser" folder.
        /// </summary>
        private async Task<bool> DistBrowser(HttpContext context, string path)
        {
            // Fallback dist/browser/
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
