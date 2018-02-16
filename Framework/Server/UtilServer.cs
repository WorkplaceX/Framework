namespace Framework.Server
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Framework.Application;
    using Framework.Component;
    using Framework.Json;
    using System.Diagnostics;
    using Database.dbo;
    using Framework.DataAccessLayer;
    using System.Linq;
    using System.Net.Http;
    using System.Text;
    using Microsoft.AspNetCore.Http;

    public static class UtilServer
    {
        /// <summary>
        /// If application runs embedded in another web page, urlRelative is returned as absolute url.
        /// </summary>
        public static string EmbeddedUrl(App app, string urlRelative, out bool isEmbedded)
        {
            string result = urlRelative;
            isEmbedded = false;
            if (app.AppJson.BrowserUrl != null)
            {
                isEmbedded = new Uri(app.AppJson.BrowserUrl).Authority != new Uri(app.AppJson.RequestUrl).Authority;
                if (isEmbedded)
                {
                    result = app.AppJson.RequestUrl;
                    if (result.EndsWith("/"))
                    {
                        result = result.Substring(0, result.Length - 1);
                    }
                    result += urlRelative;
                }
            }
            return result;
        }

        /// <summary>
        /// If application runs embedded in another web page, urlRelative is returned as absolute url.
        /// </summary>
        public static string EmbeddedUrl(App app, string urlRelative)
        {
            return EmbeddedUrl(app, urlRelative, out bool isEmbedded);
        }

        /// <summary>
        /// If application runs embedded in another web page, urlRelative is returned as absolute url. Refers to "url('')" tag in style sheet.
        /// </summary>
        /// <param name="fileName">Style sheet (*.css) file name.</param>
        public static FileContentResult EmbeddedCss(ControllerBase controller, string fileName)
        {
            if (controller.HttpContext.Request.Query["embedded"].FirstOrDefault() != null)
            {
                string contentType = FileNameToFileContentType(fileName);
                // Read file
                string text = UtilFramework.FileRead(fileName);
                text = text.Replace("url('/", "url('" + UtilServer.RequestUrl());
                var byteList = Encoding.UTF8.GetBytes(text);
                var result = controller.File(byteList, contentType);
                return result;
            }
            else
            {
                return FileNameToFileContentResult(controller, fileName);
            }
        }

        /// <summary>
        /// Returns request url
        /// </summary>
        /// <param name="isRoot">If true, it returns root url "http://localhost:49323/" Used for filed like (*.css). If false it returns "http://localhost:49323/config/". Used for Application.json</param>
        /// <returns></returns>
        internal static string RequestUrl(bool isRoot = true)
        {
            string result = null;
            HttpContext httpContext = new HttpContextAccessor().HttpContext;
            if (httpContext != null) // Otherwise running in unit test mode.
            {
                if (isRoot)
                {
                    result = string.Format("{0}://{1}/", httpContext.Request.Scheme, httpContext.Request.Host.Value);
                }
                else
                {
                    result = string.Format("{0}://{1}{2}", httpContext.Request.Scheme, httpContext.Request.Host.Value, httpContext.Request.Path);
                }
            }
            return result;
        }

        /// <summary>
        /// Run one application on this ASP.NET Core and database instance. Every WebRequest goes through here. 
        /// </summary>
        /// <param name="typeAppDefault">Type of application (See also: class App)</param>
        public static async Task<IActionResult> ControllerWebRequest(WebControllerBase webController, string controllerPath, Type typeAppDefault)
        {
            string requestPathBase;
            App app = new AppSelector(typeAppDefault).Create(webController, controllerPath, out requestPathBase);
            string url = webController.Request.Path; // For debug.
            var result = await new UtilWebController(webController, controllerPath, app).WebRequest();
            return result;
        }

        /// <summary>
        /// Run multiple applications on this ASP.NET Core and database instance. Every WebRequest goes through here. 
        /// </summary>
        public static async Task<IActionResult> ControllerWebRequest(WebControllerBase webController, string controllerPath, AppSelector appSelector)
        {
            string requestPathBase;
            App app = appSelector.Create(webController, controllerPath, out requestPathBase);
            string url = webController.Request.Path; // For debug.
            var result = await new UtilWebController(webController, requestPathBase, app).WebRequest();
            return result;
        }

        /// <summary>
        /// Start Universal server. Used for debug and BuildTool.
        /// </summary>
        internal static void StartUniversalServer()
        {
            string folderName = UtilFramework.FolderName + "Server/Universal/";
            if (Directory.Exists(folderName))
            {
                // Start Universal server, if running in Visual Studio environment.
                ProcessStartInfo info = new ProcessStartInfo();
                info.WorkingDirectory = folderName;
                info.FileName = "node.exe";
                info.Arguments = "index.js";
                info.UseShellExecute = true;
                // info.Environment.Add("PORT", "4000"); // Does not work in connection with "info.UseShellExecute = true;". For default port see also: Submodule/Client/src/server.ts
                info.WindowStyle = ProcessWindowStyle.Minimized; // Show node window minimized.
                System.Diagnostics.Process.Start(info);
            }
        }

        internal static string StreamToString(Stream stream)
        {
            using (var streamReader = new StreamReader(stream))
            {
                return streamReader.ReadToEnd();
            }
        }

        /// <summary>
        /// Returns html content type.
        /// </summary>
        private static string FileNameToFileContentType(string fileName)
        {
            // ContentType
            string fileNameExtension = Path.GetExtension(fileName);
            string result; // https://www.sitepoint.com/web-foundations/mime-types-complete-list/
            switch (fileNameExtension)
            {
                case ".html": result = "text/html"; break;
                case ".css": result = "text/css"; break;
                case ".js": result = "text/javascript"; break;
                case ".map": result = "text/plain"; break;
                case ".scss": result = "text/plain"; break; // Used only if internet explorer is in debug mode!
                case ".png": result = "image/png"; break;
                case ".ico": result = "image/x-icon"; break;
                case ".jpg": result = "image/jpeg"; break;
                case ".pdf": result = "application/pdf"; break;
                default:
                    result = "text/plain"; break; // Type not found!
            }
            return result;
        }

        /// <summary>
        /// Returns FileContentResult.
        /// </summary>
        internal static FileContentResult FileNameToFileContentResult(ControllerBase controller, string fileName)
        {
            string contentType = FileNameToFileContentType(fileName);
            // Read file
            var byteList = File.ReadAllBytes(fileName);
            var result = controller.File(byteList, contentType);
            return result;
        }

        /// <summary>
        /// Returns FileContentResult.
        /// </summary>
        internal static FileContentResult FileNameToFileContentResult(ControllerBase controller, string fileName, byte[] data)
        {
            string contentType = FileNameToFileContentType(fileName);
            return controller.File(data, contentType);
        }

        /// <summary>
        /// Returns FolderName Framework/Server/. Different folder if running on IIS.
        /// </summary>
        internal static string FolderNameFrameworkServer()
        {
            if (UtilFramework.FolderNameIsIss)
            {
                return UtilFramework.FolderName + "Server/";
            }
            else
            {
                return UtilFramework.FolderName + "Submodule/Framework/Server/";
            }
        }

        /// <summary>
        /// Returns FolderName Server/.
        /// </summary>
        internal static string FolderNameServer()
        {
            if (UtilFramework.FolderNameIsIss)
            {
                return UtilFramework.FolderName;
            }
            else
            {
                return UtilFramework.FolderName + "Server/";
            }
        }

        internal static string FileNameIndex()
        {
            string result = FolderNameFrameworkServer() + "wwwroot/" + "index.html";
            string fileNameOverride = FolderNameServer() + "wwwroot/" + "index.html";
            if (File.Exists(fileNameOverride))
            {
                return fileNameOverride;
            }
            return result;
        }

        internal static string FileNameIndexUniversal()
        {
            string result = FolderNameFrameworkServer() + "wwwroot/" + "indexUniversal.html";
            string fileNameOverride = FolderNameServer() + "wwwroot/" + "indexUniversal.html";
            if (File.Exists(fileNameOverride))
            {
                return fileNameOverride;
            }
            return result;
        }

        /// <summary>
        /// Post to json url.
        /// </summary>
        internal static async Task<string> WebPost(string url, string json, bool isEnsureSuccessStatusCode)
        {
            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response;
                try
                {
                    response = await client.PostAsync(url, new StringContent(json, Encoding.Unicode, "application/json")); // Make sure Universal server is running.
                }
                catch (HttpRequestException exception)
                {
                    throw new Exception(string.Format("Http request failed! ({0})", url), exception);
                }
                if (isEnsureSuccessStatusCode)
                {
                    response.EnsureSuccessStatusCode();
                    string result = await response.Content.ReadAsStringAsync();
                    return result;
                }
                else
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        string result = await response.Content.ReadAsStringAsync();
                        return result;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
        }
    }

    internal class UtilWebController
    {
        public UtilWebController(ControllerBase controller, string requestPathBase, App app)
        {
            this.Controller = controller;
            this.RequestPathBase = requestPathBase;
            this.App = app;
        }

        public readonly ControllerBase Controller;

        /// <summary>
        /// Gets RequestPathBase. For example "/web/framework/".
        /// </summary>
        public readonly string RequestPathBase;

        public readonly App App;

        /// <summary>
        /// Web request.
        /// </summary>
        /// <param name="typeComponentInNamespace">Additional namespace in which to search for json class when deserializing.</param>
        /// <returns></returns>
        public async Task<IActionResult> WebRequest()
        {
            string requestPath = Controller.HttpContext.Request.Path.ToString();
            UtilFramework.LogDebug(string.Format("Request ({0})", requestPath));
            // Framework/Server/wwwroot/*.* content request. For example styles.css
            if (Path.GetFileName(requestPath).Contains("."))
            {
                string fileName = Path.GetFileName(requestPath);
                fileName = UtilServer.FolderNameFrameworkServer() + "wwwroot/" + fileName;
                if (File.Exists(fileName))
                {
                    FileContentResult fileContentResult;
                    if (fileName.EndsWith("styles.css"))
                    {
                        fileContentResult = UtilServer.EmbeddedCss(Controller, fileName);
                    }
                    else
                    {
                        fileContentResult = UtilServer.FileNameToFileContentResult(Controller, fileName);
                    }
                    return fileContentResult;
                }
            }
            // App specific request
            if (!(App != null && requestPath.StartsWith(RequestPathBase)))
            {
                throw new Exception("No App defined for this path!");
                // return Controller.NotFound(); // Not found (404) response.
            }
            // Html request
            if (requestPath.StartsWith(RequestPathBase) && (requestPath.EndsWith("/") || requestPath.EndsWith(".html")))
            {
                AppJson appJsonOut = App.Run(null);
                string htmlUniversal = null;
                string html = UtilFramework.FileRead(UtilServer.FileNameIndex());
                htmlUniversal = await HtmlUniversal(html, appJsonOut, true, App); // Angular Universal server side rendering.
                return Controller.Content(htmlUniversal, "text/html");
            }
            // Json API request
            if (requestPath.StartsWith(RequestPathBase) && requestPath.EndsWith("Application.json"))
            {
                string jsonInText = UtilServer.StreamToString(Controller.Request.Body);
                AppJson appJsonIn = JsonConvert.Deserialize<AppJson>(jsonInText, new Type[] { App.TypeComponentInNamespace() });
                AppJson appJsonOut;
                try
                {
                    appJsonOut = App.Run(appJsonIn);
                    appJsonOut.ErrorProcess = null;
                }
                catch (Exception exception)
                {
                    // Prevent Internal Error 500 on process exception.
                    appJsonOut = JsonConvert.Deserialize<AppJson>(jsonInText, new Type[] { App.TypeComponentInNamespace() }); // Send AppJsonIn back.
                    appJsonOut.ErrorProcess = UtilFramework.ExceptionToText(exception);
                }
                string jsonOutText = Json.JsonConvert.Serialize(appJsonOut, new Type[] { App.TypeComponentInNamespace() });
                UtilServer.EmbeddedUrl(App, "", out bool isEmbedded);
                if (new Uri(appJsonIn.BrowserUrl).Authority != new Uri(appJsonIn.RequestUrl).Authority)
                {
                    string url = appJsonIn.BrowserUrl;
                    if (url.EndsWith("/"))
                    {
                        url = url.Substring(0, url.Length - 1);
                    }
                    // Script is embeded in a different web page.
                    Controller.Response.Headers.Add("Access-Control-Allow-Origin", url); // Avoid "C:\Program Files (x86)\Google\Chrome\Application\chrome.exe" --disable-web-security --user-data-dir
                }
                return Controller.Content(jsonOutText, "application/json");
            }
            // Server/Universal/*.js request
            if (requestPath.EndsWith(".js"))
            {
                string fileName = Path.GetFileName(requestPath);
                fileName = UtilServer.FolderNameServer() + "Universal/" + fileName;
                if (File.Exists(fileName))
                {
                    return UtilServer.FileNameToFileContentResult(Controller, fileName);
                }
            }
            // FileStorage request
            {
                string fileName = requestPath.Substring(RequestPathBase.Length);
                bool isFound = false;
                byte[] data = null;
                foreach (FrameworkFileStorage item in UtilDataAccessLayer.Query<FrameworkFileStorage>().Where(item => item.Name == fileName))
                {
                    UtilFramework.Assert(isFound == false, string.Format("Found more than one file! ({0})", fileName));
                    data = item.Data;
                    isFound = true;
                }
                if (isFound && data != null)
                {
                    return UtilServer.FileNameToFileContentResult(Controller, fileName, data);
                }
            }
            return Controller.NotFound(); // Not found (404) response.
        }

        /// <summary>
        /// Returns server side rendered index.html.
        /// </summary>
        private async Task<string> HtmlUniversal(string html, AppJson appJson, bool isUniversal, App app)
        {
            string result = html;
            if (isUniversal)
            {
                string url = "http://" + Controller.Request.Host.ToUriComponent() + "/Universal/index.js";
                appJson.IsBrowser = false; // Server side rendering mode.
                string jsonText = Json.JsonConvert.Serialize(appJson, app.TypeComponentInNamespace());
                string htmlUniversal;
                // Universal rendering
                {
                    if (UtilFramework.FolderNameIsIss)
                    {
                        // Running on IIS Server.
                        htmlUniversal = await UtilServer.WebPost(url, jsonText, true); // Call Angular Universal server side rendering service.
                    }
                    else
                    {
                        // Running in Visual Studio
                        url = "http://localhost:4000/Universal/index.js"; // Call Universal server when running in Visual Studio.
                        htmlUniversal = await UtilServer.WebPost(url, jsonText, true);
                    }
                }
                //
                string htmlBegin = "<html><head></head><body>";
                string htmlEnd = "</body></html>";
                UtilFramework.Assert(htmlUniversal.StartsWith(htmlBegin));
                UtilFramework.Assert(htmlUniversal.EndsWith(htmlEnd));
                result = htmlUniversal.Substring(htmlBegin.Length, htmlUniversal.Length - (htmlBegin.Length + htmlEnd.Length));
                //
                UtilFramework.Assert((int)result[0] == 65279); // Special character
                result = result.Substring(1); // Remove specail character
                //
                string htmlFind = "<div data-app=\"\" ng-version=\"4.2.4\">";
                UtilFramework.Assert(result.StartsWith(htmlFind));
                result = result.Replace(htmlFind, "<div data-app=\"\" data-ng-version=\"4.2.4\">"); // Prefix data for html5.
                //
                htmlFind = "<div data-app></div>";
                UtilFramework.Assert(html.Contains(htmlFind));
                result = html.Replace(htmlFind, result);
                //
                htmlFind = "innerHTML=\"";
                result = result.Replace(htmlFind, "data-innerHTML=\""); // Prefix data for html5.
            }
            appJson.IsBrowser = true; // Client side rendering mode.
            string jsonTextBrowser = Json.JsonConvert.Serialize(appJson, app.TypeComponentInNamespace());
            string resultAssert = result;
            // Add json to index.html (Client/index.html)
            {
                string scriptFind = "var browserJson = '{ }';";
                string scriptReplace = "var browserJson = " + jsonTextBrowser + "; ";
                result = result.Replace(scriptFind, scriptReplace);
            }
            UtilFramework.Assert(resultAssert != result, "Adding browserJson failed!");
            return result;
        }
    }
}
