namespace Framework.Server
{
    using Framework.Application;
    using Framework.Config;
    using Microsoft.AspNetCore.Http;
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using System.Web;

    internal class Request
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

            // Get current website request from "ConfigWebServer.json"
            AppSelector appSelector = new AppSelector();

            // POST app.json
            if (await Post(context, path, appSelector))
            {
                return;
            }

            // GET index.html from "Application.Server/Framework/Website/" (With server side rendering)
            if (await WebsiteServerSideRenderingAsync(context, path, appSelector))
            {
                return;
            }

            // GET file from "Application.Server/Framework/Website/"
            if (await WebsiteFileAsync(context, path, appSelector))
            {
                return;
            }

            // GET Angular file from "Application.Server/Framework/Angular/browser"
            if (await AngularBrowserFileAsync(context, path))
            {
                return;
            }

            context.Response.StatusCode = 404; // Not found
        }

        /// <summary>
        /// Handle client web POST /app.json
        /// </summary>
        private static async Task<bool> Post(HttpContext context, string path, AppSelector appSelector)
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
        private static async Task<bool> WebsiteServerSideRenderingAsync(HttpContext context, string path, AppSelector appSelector)
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
                if (fileName.EndsWith(".html") && ConfigWebServer.Load().IsServerSideRendering && UtilFramework.StringNull(appSelector.Website.AppTypeName) != null)
                {
                    context.Response.ContentType = UtilServer.ContentType(fileName);
                    string htmlIndex = await WebsiteServerSideRenderingAsync(context, appSelector);
                    await context.Response.WriteAsync(htmlIndex);
                    result = true;
                }
            }
            return result;
        }

        /// <summary>
        /// Render first html GET request.
        /// </summary>
        private static async Task<string> WebsiteServerSideRenderingAsync(HttpContext context, AppSelector appSelector)
        {
            string url;
            if (UtilServer.IsIssServer)
            {
                // Running on IIS Server.
                url = "http://" + context.Request.Host.ToUriComponent() + "/Framework/Angular/server.js"; // Url of server side rendering when running on IIS Server
            }
            else
            {
                // Running in Visual Studio.
                url = "http://localhost:4000/"; // Url of server side rendering when running in Visual Studio
            }

            // Process AppJson
            string jsonClient = await appSelector.CreateAppAndProcessAsync(context);  // Process (For first server side rendering)

            // Server side rendering POST.
            string serverSideRenderView = "Website/" + UtilFramework.FolderNameParse(appSelector.Website.FolderNameServer, "/index.html");
            serverSideRenderView = HttpUtility.UrlEncode(serverSideRenderView);
            url += "?view=" + serverSideRenderView;
            string indexHtml = await UtilServer.WebPost(url, jsonClient); // Server side rendering POST

            // Set jsonBrowser in index.html.
            string scriptFind = "var jsonBrowser = {};";
            string scriptReplace = "var jsonBrowser = " + jsonClient + ";";
            indexHtml = UtilFramework.Replace(indexHtml, scriptFind, scriptReplace);

            return indexHtml;
        }


        /// <summary>
        /// Returns true, if file found in folder "Application.Server/Framework/Website/"
        /// </summary>
        private async Task<bool> WebsiteFileAsync(HttpContext context, string path, AppSelector appSelector)
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
