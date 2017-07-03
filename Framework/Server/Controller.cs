namespace Framework.Server
{
    using Framework.Component;
    using Framework.Json;
    using Framework.Application;
    using Microsoft.AspNetCore.Mvc;
    using System;
    using System.IO;
    using System.Net.Http;
    using System.Reflection;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;
    using Framework.DataAccessLayer;

    public class WebController
    {
        public WebController(ControllerBase controller, string routePath, App app)
        {
            this.Controller = controller;
            this.RoutePath = routePath;
            this.App = app;
        }

        public readonly ControllerBase Controller;

        public readonly string RoutePath;

        public readonly App App;

        /// <summary>
        /// Web request.
        /// </summary>
        /// <param name="typeComponentInNamespace">Additional namespace in which to search for json class when deserializing.</param>
        /// <returns></returns>
        internal async Task<IActionResult> WebRequest()
        {
            // Html request
            if (Controller.HttpContext.Request.Path == RoutePath)
            {
                AppJson appJsonOut = App.Run(null, Controller.HttpContext);
                string htmlUniversal = null;
                string html = UtilServer.FileNameIndex();
                htmlUniversal = await HtmlUniversal(html, appJsonOut, true, App); // Angular Universal server side rendering.
                return Controller.Content(htmlUniversal, "text/html");
            }
            // Json API request
            if (Controller.HttpContext.Request.Path == RoutePath + "Application.json")
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
                    appJsonOut.ErrorProcess = Framework.UtilFramework.ExceptionToText(exception);
                }
                string jsonOutText = Json.JsonConvert.Serialize(appJsonOut, new Type[] { App.TypeComponentInNamespace() });
                if (Debugger.IsAttached)
                {
                    Controller.Response.Headers.Add("Access-Control-Allow-Origin", "http://localhost:4200"); // Avoid "C:\Program Files (x86)\Google\Chrome\Application\chrome.exe" --disable-web-security --user-data-dir
                }
                return Controller.Content(jsonOutText, "application/json");
            }
            // Framework/Server/wwwroot/*.* request
            {
                string fileName = Controller.HttpContext.Request.Path.ToString().Substring(RoutePath.Length);
                fileName = UtilServer.FolderNameFrameworkServer() + "wwwroot/" + fileName;
                if (File.Exists(fileName))
                {
                    return UtilServer.FileNameToFileContentResult(Controller, fileName);
                }
            }
            // Server/Universal/*.js request
            if (Controller.HttpContext.Request.Path.ToString().EndsWith(".js"))
            {
                string fileName = Controller.HttpContext.Request.Path.ToString().Substring(RoutePath.Length);
                fileName = UtilServer.FolderNameServer() + "Universal/" + fileName;
                if (File.Exists(fileName))
                {
                    return UtilServer.FileNameToFileContentResult(Controller, fileName);
                }
            }
            // FileStorage request
            {
                string fileName = Controller.HttpContext.Request.Path.ToString().Substring(RoutePath.Length);
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
                    if (Framework.UtilFramework.FolderNameIsIss)
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
                Framework.UtilFramework.Assert(resultAssert != result, "Adding browserJson failed!");
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
                HttpResponseMessage response = await client.PostAsync(url, new StringContent(json, Encoding.Unicode, "application/json"));
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
