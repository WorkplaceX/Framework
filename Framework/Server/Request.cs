﻿namespace Framework.Server
{
    using Framework.App;
    using Framework.Config;
    using Framework.Json;
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

            UtilStopwatch.RequestBind();
            try
            {
                UtilStopwatch.TimeStart(name: "Request");

                UtilServer.Cors();

                // Request path
                string path = context.Request.Path;

                // Get current website request from "ConfigServer.json"
                AppSelector appSelector = new AppSelector();

                // POST app.json
                if (!await Post(context, path, appSelector))
                {
                    // GET index.html from "Application.Server/Framework/Application.Website/" (With server side rendering or serve index.html directly)
                    if (!await WebsiteServerSideRenderingAsync(context, path, appSelector, null))
                    {
                        // GET file from "Application.Server/Framework/Application.Website/"
                        if (!await WebsiteFileAsync(context, path, appSelector))
                        {
                            // GET Angular file from "Application.Server/Framework/Framework.Angular/browser"
                            if (!await AngularBrowserFileAsync(context, path))
                            {
                                // GET file from database or navigate to subpage.
                                if (!await FileDownloadAsync(context, path, appSelector))
                                {
                                    context.Response.StatusCode = StatusCodes.Status404NotFound;
                                }
                            }
                        }
                    }
                }

                // Total time for one request.
                UtilStopwatch.TimeStop(name: "Request");
                // One log entry for one request.
                UtilStopwatch.TimeLog(); 
            }
            finally
            {
                UtilStopwatch.RequestRelease();
            }
        }

        /// <summary>
        /// Handle client web POST /app.json
        /// </summary>
        private static async Task<bool> Post(HttpContext context, string path, AppSelector appSelector)
        {
            bool result = false;
            if (path == "/app.json")
            {
                string jsonClient = await appSelector.ProcessAsync(context, null); // Process (Client http post)
                context.Response.ContentType = UtilServer.ContentType(path);
                
                await context.Response.WriteAsync(jsonClient);
                result = true;
            }

            return result;
        }

        /// <summary>
        /// Divert request to "Application.Server/Framework/Application.Website/"
        /// </summary>
        private static async Task<bool> WebsiteServerSideRenderingAsync(HttpContext context, string navigatePath, AppSelector appSelector, AppJson appJson)
        {
            bool result = false;

            // FolderNameServer
            string folderNameServer = appSelector.Website.FolderNameServerGet(appSelector.ConfigServer, "Application.Server/");

            // FolderName
            string folderName = UtilServer.FolderNameContentRoot() + folderNameServer;
            if (!Directory.Exists(folderName))
            {
                throw new Exception(string.Format("Folder does not exis! Make sure cli build command did run. ({0})", folderName));
            }

            // Index.html
            string pathIndexHtml = navigatePath;
            if (!UtilServer.NavigatePathIsFileName(navigatePath))
            {
                pathIndexHtml += "index.html";
            }

            // FileName
            string fileName = folderName + UtilFramework.FolderNameParse(null, pathIndexHtml);
            if (File.Exists(fileName))
            {
                if (fileName.EndsWith(".html") && UtilFramework.StringNull(appSelector.AppTypeName) != null)
                {
                    context.Response.ContentType = UtilServer.ContentType(fileName);

                    // Do not cache (*.html) page with included jsonBrowser. For example if user navigates to sub page (POST) and then opens an image 
                    // and then navigates back, it forces browser to reload page and not to show an old cached page.
                    // See also: http://cristian.sulea.net/blog/disable-browser-caching-with-meta-html-tags/
                    context.Response.Headers.Add("Cache-Control", "no-cache, no-store, must-revalidate");
                    context.Response.Headers.Add("Pragma", "no-cache");
                    context.Response.Headers.Add("Expires", "0");

                    // Create page (*.html). Also if SSR is disabled.
                    string htmlIndex = await WebsiteServerSideRenderingAsync(context, appSelector, appJson);

                    await context.Response.WriteAsync(htmlIndex);
                    result = true;
                }
            }
            return result;
        }

        /// <summary>
        /// Browser GET request to download file.
        /// </summary>
        private static async Task<bool> FileDownloadAsync(HttpContext context, string path, byte[] data)
        {
            bool result = false;
            if (data != null)
            {
                UtilFramework.Assert(data != null);
                string fileName = UtilFramework.FolderNameParse(null, path);
                context.Response.ContentType = UtilServer.ContentType(fileName);
                await context.Response.Body.WriteAsync(data, 0, data.Length);
                result = true;
            }
            return result;
        }

        /// <summary>
        /// Browser request to download file or navigate to subpage.
        /// </summary>
        private static async Task<bool> FileDownloadAsync(HttpContext context, string navigatePath, AppSelector appSelector)
        {
            bool result;
            var appJson = appSelector.CreateAppJson(); // Without deserialize session.
            var navigateResult = await appJson.NavigateInternalAsync(navigatePath);
            if (navigateResult.IsSession)
            {
                var appJsonSession = await appSelector.CreateAppJsonSession(context); // With deserialize session.
                var navigateSessionResult = await appJsonSession.NavigateSessionInternalAsync(navigatePath, isAddHistory: false);
                if (navigateSessionResult.IsPage)
                {
                    // Send page together with HTTP 404 not found code
                    if (navigateSessionResult.IsPageNotFound)
                    {
                        context.Response.StatusCode = StatusCodes.Status404NotFound;
                    }

                    result = await WebsiteServerSideRenderingAsync(context, "/", appSelector, appJsonSession);
                }
                else
                {
                    // File download with session
                    result = await FileDownloadAsync(context, navigatePath, navigateSessionResult.Data);
                }
            }
            else
            {
                // File download without session
                result = await FileDownloadAsync(context, navigatePath, navigateResult.Data);
            }
            return result;
        }

        /// <summary>
        /// Render first html GET request. Also if SSR is disabled. Include always jsonBrowser into (*.html) response file.
        /// </summary>
        private static async Task<string> WebsiteServerSideRenderingAsync(HttpContext context, AppSelector appSelector, AppJson appJson)
        {
            string url;
            if (UtilServer.IsIssServer)
            {
                // Running on IIS Server.
                url = context.Request.IsHttps ? "https://" : "http://";
                url += context.Request.Host.ToUriComponent() + "/Framework/Framework.Angular/server/main.js"; // Url of server side rendering when running on IIS Server
            }
            else
            {
                // Running in Visual Studio.
                url = "http://localhost:4000/"; // Url of server side rendering when running in Visual Studio
            }

            // Process AppJson
            string jsonClient = await appSelector.ProcessAsync(context, appJson); // Process (For first server side rendering)

            // Server side rendering POST.
            string folderNameServer = appSelector.Website.FolderNameServerGet(appSelector.ConfigServer, "Application.Server/Framework/");

            string serverSideRenderView = UtilFramework.FolderNameParse(folderNameServer, "/index.html");
            serverSideRenderView = HttpUtility.UrlEncode(serverSideRenderView);
            url += "?view=" + serverSideRenderView;

            bool isServerSideRendering = ConfigServer.Load().IsServerSideRendering;
            string indexHtml;
            if (isServerSideRendering)
            {
                // index.html server side rendering
                indexHtml = await UtilServer.WebPost(url, jsonClient); // Server side rendering POST. http://localhost:5000/Framework/Framework.Angular/server.js?view=Application.Website%2fDefault%2findex.html
            }
            else
            {
                // index.html serve directly
                string fileName = UtilServer.FolderNameContentRoot() + UtilFramework.FolderNameParse(appSelector.Website.FolderNameServerGet(appSelector.ConfigServer, "Application.Server/"), "/index.html");
                indexHtml = UtilFramework.FileLoad(fileName);
            }

            // Set jsonBrowser in index.html.
            string scriptFind = "</app-root>"; //" <script>var jsonBrowser={}</script>"; // For example Html5Boilerplate build process renames var jsonBrowser to a.
            string scriptReplace = "</app-root><script>var jsonBrowser = " + jsonClient + "</script>";
            indexHtml = UtilFramework.Replace(indexHtml, scriptFind, scriptReplace); // Send jsonBrowser with index.html to client for both SSR and not SSR.

            // Add Angular scripts
            scriptFind = "</body></html>";
            scriptReplace = "<script src=\"runtime.js\" defer></script><script src=\"polyfills.js\" defer></script><script src=\"main.js\" defer></script>" +
                "</body></html>";
            indexHtml = UtilFramework.Replace(indexHtml, scriptFind, scriptReplace);

            return indexHtml;
        }

        /// <summary>
        /// Returns true, if file found in folder "Application.Server/Framework/Application.Website/"
        /// </summary>
        private async Task<bool> WebsiteFileAsync(HttpContext context, string path, AppSelector appSelector)
        {
            bool result = false;
            if (UtilServer.NavigatePathIsFileName(path))
            {
                // Serve fileName
                string fileName = UtilServer.FolderNameContentRoot() + UtilFramework.FolderNameParse(appSelector.Website.FolderNameServerGet(appSelector.ConfigServer, "Application.Server/"), path);
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
        /// Returns true, if file found in folder "Application.Server/Framework/Framework.Angular/browser".
        /// </summary>
        private async Task<bool> AngularBrowserFileAsync(HttpContext context, string path)
        {
            // Fallback Application.Server/Framework/Framework.Angular/browser
            if (UtilServer.NavigatePathIsFileName(path))
            {
                // Serve fileName
                string fileName = UtilServer.FolderNameContentRoot() + "Framework/Framework.Angular/browser" + path;

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
