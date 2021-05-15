namespace Framework.Server
{
    using Framework.App;
    using Framework.Config;
    using Framework.Json;
    using Microsoft.AspNetCore.Http;
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Threading.Tasks;

    internal class Request
    {
        /// <summary>
        /// Every request goes through here.
        /// </summary>
        public async Task RunAsync(HttpContext context)
        {
            // await Task.Delay(500); // Simulate slow network.

            // Log
            var logTime = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mmm:ss.fff");
            var logStopwatch = new Stopwatch();
            logStopwatch.Start();
            var logSessionLength = 0;
            string logException = null;

            UtilStopwatch.RequestBind();
            try
            {
                UtilStopwatch.TimeStart(name: "Request");

                UtilServer.Cors();

                // Request path
                string path = context.Request.Path;

                // Get current website request from "ConfigServer.json"
                AppSelector appSelector = new AppSelector();
                var isRedirectHttps = appSelector.ConfigDomain.IsRedirectHttps && !context.Request.IsHttps;

                if (isRedirectHttps)
                {
                    // RedirectHttps on website level. Not on server middleware level.
                    string url = "https://" + context.Request.Host + context.Request.Path + context.Request.QueryString;
                    context.Response.Redirect(url);
                }
                else
                {
                    // POST app.json
                    if (!await Post(context, path, appSelector))
                    {
                        // GET index.html from "Application.Server/Framework/Application.Website/Website01/browser/" (With server side rendering or serve index.html directly)
                        if (!await WebsiteServerSideRenderingAsync(context, path, appSelector, null))
                        {
                            // GET file from "Application.Server/Framework/Application.Website/Website01/browser/"
                            if (!await WebsiteFileAsync(context, path, appSelector))
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

                logSessionLength = appSelector.JsonSessionLength;

                // Total time for one request.
                UtilStopwatch.TimeStop(name: "Request");
                // One log entry for one request.
                UtilStopwatch.TimeLog();
            }
            catch (Exception exception)
            {
                logException = UtilFramework.ExceptionToString(exception);
            }
            finally
            {
                UtilStopwatch.RequestRelease();
            }

            // Log
            {
                logStopwatch.Stop();
                string logEscape(string value)
                {
                    return value?.Replace("\r", "(new line)").Replace("\n", "(new line)").Replace(";", "(semicolon)").Replace(",", "(comma)").Replace("\"", "(double quote)").Replace("'", "(quote)");
                }
                var logIp = context.Connection.RemoteIpAddress.ToString();
                var logUserAgent = logEscape(context.Request.Headers["User-Agent"].ToString());
                var logTimeDelta = (logStopwatch.ElapsedMilliseconds / 1000.0f).ToString();
                var logMethod = context.Request.Method;
                var logHost = string.Format("{0}://{1}/", context.Request.Scheme, context.Request.Host.Value);
                var logNavigatePath = logEscape(context.Request.Path + context.Request.QueryString.ToString());
                logException = logEscape(logException);
                var logText = $"{ UtilFramework.Version },=\"{ logTime }\",{ logTimeDelta },{ logIp },{ logMethod },{ logHost },{ logHost }{ logNavigatePath.Substring(1) },{logSessionLength},{ logUserAgent },{logException}";
                File.AppendAllText(UtilFramework.FileNameLog, logText + Environment.NewLine);
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
        /// Divert request to "Application.Server/Framework/Application.Website/Website01/browser/"
        /// </summary>
        private static async Task<bool> WebsiteServerSideRenderingAsync(HttpContext context, string navigatePath, AppSelector appSelector, AppJson appJson)
        {
            bool result = false;

            // FolderNameServer
            string folderNameServer = appSelector.ConfigWebsite.FolderNameServerGet(appSelector, "Application.Server/");

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

                    // Google Analytics 4
                    if (UtilFramework.StringNull(appSelector.ConfigDomain.GoogleAnalyticsId) != null)
                    {
                        htmlIndex = htmlIndex.Replace("G-XXXXXXXXXX", appSelector.ConfigDomain.GoogleAnalyticsId);
                    }

                    // Google AdSense
                    if (UtilFramework.StringNull(appSelector.ConfigDomain.GoogleAdSenseId) != null)
                    {
                        htmlIndex = htmlIndex.Replace("ca-pub-XXXXXXXXXXXXXXXX", appSelector.ConfigDomain.GoogleAdSenseId);
                    }

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
            var navigateResult = await appJson.NavigateInternalAsync(navigatePath, appSelector);
            if (navigateResult.RedirectPath != null)
            {
                context.Response.Redirect(navigateResult.RedirectPath);
                result = true;
            }
            else
            {
                if (navigateResult.IsSession)
                {
                    var appJsonSession = await appSelector.CreateAppJsonSession(context); // With deserialize session.
                    var navigateSessionResult = await appJsonSession.NavigateSessionInternalAsync(navigatePath, isAddHistory: false, appSelector);
                    if (navigateSessionResult.IsPage)
                    {
                        if (navigateSessionResult.RedirectPath != null)
                        {
                            context.Response.Redirect(navigateSessionResult.RedirectPath);
                            result = true;
                        }
                        else
                        {
                            // Send page together with HTTP 404 not found code
                            if (navigateSessionResult.IsPageNotFound)
                            {
                                // Do not serialize custom error page and reset request, response count
                                context.Response.StatusCode = StatusCodes.Status404NotFound;
                                appJsonSession.IsPageNotFound = true;
                                appJsonSession.RequestCount -= 1;
                                appJsonSession.ResponseCount -= 1;
                                // Custom error page rendering
                                await WebsiteServerSideRenderingAsync(context, "/", appSelector, appJsonSession);
                                result = true;
                            }
                            else
                            {
                                result = await WebsiteServerSideRenderingAsync(context, "/", appSelector, appJsonSession);
                            }
                        }
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
                url += context.Request.Host.ToUriComponent() + "/Framework/Application.Website/" + UtilFramework.FolderNameParse(appSelector.ConfigWebsite.FolderNameAngularWebsite) + "server/main.js"; // Url of server side rendering when running on IIS Server
            }
            else
            {
                // Running in Visual Studio. See also method StartUniversalServerAngular();
                url = "http://localhost:" + (appSelector.ConfigWebsite.FolderNameAngularPort).ToString() + "/"; // Url of server side rendering when running in Visual Studio
            }

            // Process AppJson
            string jsonClient = await appSelector.ProcessAsync(context, appJson); // Process (For first server side rendering)

            bool isServerSideRendering = ConfigServer.Load().IsServerSideRendering;
            string indexHtml;
            if (isServerSideRendering)
            {
                // index.html server side rendering
                indexHtml = await UtilServer.WebPost(url, jsonClient); // Server side rendering POST. http://localhost:8080/Framework/Application.Website/Website01/server/main.js
            }
            else
            {
                // index.html serve directly
                string fileName = UtilServer.FolderNameContentRoot() + UtilFramework.FolderNameParse(appSelector.ConfigWebsite.FolderNameServerGet(appSelector, "Application.Server/"), "/index.html");
                indexHtml = UtilFramework.FileLoad(fileName);
            }

            // Set jsonBrowser in index.html.
            string scriptFind = "</app-root>"; //" <script>var jsonBrowser={}</script>"; // For example Html5Boilerplate build process renames var jsonBrowser to a.
            string scriptReplace = "</app-root><script>var jsonBrowser = " + jsonClient + "</script>";
            indexHtml = UtilFramework.Replace(indexHtml, scriptFind, scriptReplace); // Send jsonBrowser with index.html to client for both SSR and not SSR.

            return indexHtml;
        }

        /// <summary>
        /// Returns true, if file found in folder "Application.Server/Framework/Application.Website/Website01/browser/"
        /// </summary>
        private async Task<bool> WebsiteFileAsync(HttpContext context, string path, AppSelector appSelector)
        {
            bool result = false;
            if (UtilServer.NavigatePathIsFileName(path))
            {
                // Serve fileName
                string fileName = UtilServer.FolderNameContentRoot() + UtilFramework.FolderNameParse(appSelector.ConfigWebsite.FolderNameServerGet(appSelector, "Application.Server/"), path);
                if (File.Exists(fileName))
                {
                    context.Response.ContentType = UtilServer.ContentType(fileName);
                    await context.Response.SendFileAsync(fileName);
                    return true;
                }
            }
            return result;
        }
    }
}
