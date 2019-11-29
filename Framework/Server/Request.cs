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
            if (result.Contains("<app-root></app-root>")) // Needs server sie rendering
            {
                string url;
                if (UtilServer.IsIssServer)
                {
                    // Running on IIS Server.
                    url = "http://" + context.Request.Host.ToUriComponent() + "/Framework/Angular/server.js";
                }
                else
                {
                    // Running in Visual Studio.
                    url = "http://localhost:4000/"; // Call Universal server when running in Visual Studio.
                }
                var app = await appSelector.CreateAppAndProcessAsync(context);  // Process (Server side rendering)

                // Serialize
                app.AppJson.ServerSideRenderView = "Angular/browser/indexUniversal.html";
                string json = UtilJson.Serialize(app.AppJson, app.TypeComponentInNamespaceList);
                UtilSession.Serialize(app);

                // Server side render post.
                string htmlServerSideRendering = await UtilServer.WebPost(url, json);

                htmlServerSideRendering = UtilFramework.Replace(htmlServerSideRendering, "<html><head><style ng-transition=\"serverApp\"></style></head><body>", "");
                htmlServerSideRendering = UtilFramework.Replace(htmlServerSideRendering, "</body></html>", "");

                result = UtilFramework.Replace(result, "<app-root></app-root>", htmlServerSideRendering);

                // Set jsonBrowser in html.
                string scriptFind = "var jsonBrowser = {};";
                string scriptReplace = "var jsonBrowser = " + json + ";";
                result = UtilFramework.Replace(result, scriptFind, scriptReplace);
            }
            return result;
        }

        /// <summary>
        /// Divert request to "Application.Server/Framework/Website/"
        /// </summary>
        private static async Task<bool> WebsiteAsync(HttpContext context, string path, AppSelector appSelector)
        {
            bool result = false;
            var configWebServer = ConfigWebServer.Load();
            var website = configWebServer.WebsiteList.FirstOrDefault();
            if (website != null)
            {
                string folderName = UtilServer.FolderNameContentRoot() + "Framework/Website/" + website.FolderNameServer;
                if (!Directory.Exists(folderName))
                {
                    throw new Exception(string.Format("Folder does not exis! Make sure cli build did run. ({0})", folderName));
                }
                string fileName = UtilServer.FolderNameContentRoot() + "Framework/Website/" + UtilFramework.FolderNameParse(website.FolderNameServer, path);
                if (File.Exists(fileName))
                {
                    context.Response.ContentType = UtilServer.ContentType(fileName);
                    if (fileName.EndsWith(".html") && ConfigWebServer.Load().IsServerSideRendering)
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
        /// Returns true, if file found in "Application.Server/Framework/Angular/browser" folder.
        /// </summary>
        private async Task<bool> DistBrowser(HttpContext context, string path)
        {
            // Fallback Angular/browser/
            if (UtilServer.PathIsFileName(path))
            {
                // Serve fileName
                string fileName = UtilServer.FolderNameContentRoot() + "Framework/Angular/browser" + path;
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

    internal class Request2
    {
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

            AppSelector2 appSelector = new AppSelector2();

            // POST app.json
            if (await Post(context, path, appSelector))
            {
                return;
            }

            // GET Website
            if (await WebsiteAsync(context, path, appSelector)) // With server side rendering.
            {
                return;
            }

            // GET "Application.Server/Framework/Website/"
            if (await WebsiteFileAsync(context, path, appSelector))
            {
                return;
            }

            // GET "Application.Server/Framework/Angular/browser"
            if (await AngularBrowserFileAsync(context, path))
            {
                return;
            }
            
            context.Response.StatusCode = 404; // Not found
        }

        /// <summary>
        /// Handle client web POST /app.json
        /// </summary>
        private static async Task<bool> Post(HttpContext context, string path, AppSelector2 appSelector)
        {
            bool result = false;
            if (path == "/app.json")
            {
                string jsonClient = await appSelector.CreateAppAndProcessAsync(context); // Process (Client http post)
                context.Response.ContentType = UtilServer.ContentType(path);

                await context.Response.WriteAsync(jsonClient);
                result = true;
            }

            return result;
        }

        /// <summary>
        /// Divert request to "Application.Server/Framework/Website/"
        /// </summary>
        private static async Task<bool> WebsiteAsync(HttpContext context, string path, AppSelector2 appSelector)
        {
            bool result = false;
            string folderName = UtilServer.FolderNameContentRoot() + "Framework/Website/" + appSelector.Website.FolderNameServer;
            if (!Directory.Exists(folderName))
            {
                throw new Exception(string.Format("Folder does not exis! Make sure cli build did run. ({0})", folderName));
            }
            string fileName = UtilServer.FolderNameContentRoot() + "Framework/Website/" + UtilFramework.FolderNameParse(appSelector.Website.FolderNameServer, path);
            if (File.Exists(fileName))
            {
                if (fileName.EndsWith(".html") && ConfigWebServer.Load().IsServerSideRendering)
                {
                    context.Response.ContentType = UtilServer.ContentType(fileName);
                    string htmlIndex = UtilFramework.FileLoad(fileName);
                    htmlIndex = await ServerSideRenderingAsync(context, htmlIndex, appSelector);
                    await context.Response.WriteAsync(htmlIndex);
                    result = true;
                }
            }
            return result;
        }

        /// <summary>
        /// Render first html GET request.
        /// </summary>
        private static async Task<string> ServerSideRenderingAsync(HttpContext context, string indexHtml, AppSelector2 appSelector)
        {
            string result = indexHtml;
            if (result.Contains("<app-root></app-root>")) // Needs server sie rendering
            {
                string url;
                if (UtilServer.IsIssServer)
                {
                    // Running on IIS Server.
                    url = "http://" + context.Request.Host.ToUriComponent() + "/Framework/Angular/server.js";
                }
                else
                {
                    // Running in Visual Studio.
                    url = "http://localhost:4000/"; // Call Universal server when running in Visual Studio.
                }
                string jsonClient = await appSelector.CreateAppAndProcessAsync(context);  // Process (Server side rendering)

                // Server side render post.
                result = await UtilServer.WebPost(url, jsonClient);

                // Set jsonBrowser in html.
                string scriptFind = "var jsonBrowser = {};";
                string scriptReplace = "var jsonBrowser = " + jsonClient + ";";
                result = UtilFramework.Replace(result, scriptFind, scriptReplace);
            }
            return result;
        }


        /// <summary>
        /// Returns true, if file found in folder "Application.Server/Framework/Website/"
        /// </summary>
        private async Task<bool> WebsiteFileAsync(HttpContext context, string path, AppSelector2 appSelector)
        {
            bool result = false;
            if (UtilServer.PathIsFileName(path))
            {
                // Serve fileName
                string fileName = UtilServer.FolderNameContentRoot() + "Framework/Website/" + UtilFramework.FolderNameParse(appSelector.Website.FolderNameServer, path);
                if (File.Exists(fileName))
                {
                    context.Response.ContentType = UtilServer.ContentType(fileName);
                    await context.Response.SendFileAsync(fileName);
                    return true;
                }
            }
            return result;
        }

        /// <summary>
        /// Returns true, if file found in folder "Application.Server/Framework/Angular/browser".
        /// </summary>
        private async Task<bool> AngularBrowserFileAsync(HttpContext context, string path)
        {
            // Fallback Application.Server/Framework/Angular/browser
            if (UtilServer.PathIsFileName(path))
            {
                // Serve fileName
                string fileName = UtilServer.FolderNameContentRoot() + "Framework/Angular/browser" + path;
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