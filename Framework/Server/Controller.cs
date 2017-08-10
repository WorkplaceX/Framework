namespace Framework.Server
{
    using Framework.Component;
    using Framework.Json;
    using Framework.Application;
    using Microsoft.AspNetCore.Mvc;
    using System;
    using System.IO;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;
    using Framework.DataAccessLayer;
    using System.Linq;
    using Database.dbo;

    public class WebController
    {
        public WebController(ControllerBase controller, string requestPathBase, App app)
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
        internal async Task<IActionResult> WebRequest()
        {
            string requestPath = Controller.HttpContext.Request.Path.ToString();
            if (!(App != null && requestPath.StartsWith(RequestPathBase)))
            {
                return Controller.NotFound(); // Not found (404) response.
            }
            // Html request
            if (requestPath.StartsWith(RequestPathBase) && (requestPath.EndsWith("/") || requestPath.EndsWith(".html")))
            {
                AppJson appJsonOut = App.Run(null, Controller.HttpContext);
                string htmlUniversal = null;
                string html = UtilServer.FileNameIndex();
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
                    appJsonOut = App.Run(appJsonIn, Controller.HttpContext);
                    appJsonOut.ErrorProcess = null;
                }
                catch (Exception exception)
                {
                    // Prevent Internal Error 500 on process exception.
                    appJsonOut = JsonConvert.Deserialize<AppJson>(jsonInText, new Type[] { App.TypeComponentInNamespace() }); // Send AppJsonIn back.
                    appJsonOut.ErrorProcess = UtilFramework.ExceptionToText(exception);
                }
                string jsonOutText = Json.JsonConvert.Serialize(appJsonOut, new Type[] { App.TypeComponentInNamespace() });
                if (Debugger.IsAttached)
                {
                    Controller.Response.Headers.Add("Access-Control-Allow-Origin", "http://localhost:4200"); // Avoid "C:\Program Files (x86)\Google\Chrome\Application\chrome.exe" --disable-web-security --user-data-dir
                }
                return Controller.Content(jsonOutText, "application/json");
            }
            // Framework/Server/wwwroot/*.* request
            if (Path.GetFileName(requestPath).Contains("."))
            {
                string fileName = Path.GetFileName(requestPath);
                fileName = UtilServer.FolderNameFrameworkServer() + "wwwroot/" + fileName;
                if (File.Exists(fileName))
                {
                    return UtilServer.FileNameToFileContentResult(Controller, fileName);
                }
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
                foreach (FrameworkFileStorage item in UtilDataAccessLayer.Select<FrameworkFileStorage>().Where(item => item.Name == fileName))
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
            if (isUniversal == false)
            {
                return html;
            }
            else
            {
                string htmlUniversal = null;
                string url = "http://" + Controller.Request.Host.ToUriComponent() + "/Universal/index.js";
                appJson.IsBrowser = false; // Server side rendering mode.
                string jsonText = Json.JsonConvert.Serialize(appJson, app.TypeComponentInNamespace());
                // Universal rendering
                {
                    if (UtilFramework.FolderNameIsIss)
                    {
                        // Running on IIS Server.
                        htmlUniversal = await Post(url, jsonText, true); // Call Angular Universal server side rendering service.
                    }
                    else
                    {
                        // Running in Visual Studio
                        url = "http://localhost:1337/Universal/index.js"; // Call UniversalExpress when running in Visual Studio.
                        htmlUniversal = await Post(url, jsonText, true);
                    }
                }
                //
                string result = htmlUniversal;
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

        /// <summary>
        /// Post to json url.
        /// </summary>
        private async Task<string> Post(string url, string json, bool isEnsureSuccessStatusCode)
        {
            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.PostAsync(url, new StringContent(json, Encoding.Unicode, "application/json")); // Make sure project UniversalExpress is running.
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
}
