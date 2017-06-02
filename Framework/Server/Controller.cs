﻿namespace Framework.Server
{
    using Framework.Server.Application;
    using Framework.Server.Application.Json;
    using Microsoft.AspNetCore.Mvc;
    using System;
    using System.IO;
    using System.Net.Http;
    using System.Reflection;
    using System.Text;
    using System.Threading.Tasks;

    public class WebController
    {
        public WebController(ControllerBase controller, string routePath, ApplicationServerBase applicationServer)
        {
            this.Controller = controller;
            this.RoutePath = routePath;
            this.ApplicationServer = applicationServer;
        }

        public readonly ControllerBase Controller;

        public readonly string RoutePath;

        public readonly ApplicationServerBase ApplicationServer;

        public async Task<IActionResult> Web()
        {
            // Html
            if (Controller.HttpContext.Request.Path == RoutePath)
            {
                ApplicationJson applicationJsonOut = ApplicationServer.Process(null, Controller.HttpContext.Request.Path);
                string htmlUniversal = null;
                string html = IndexHtml(true);
                htmlUniversal = await HtmlUniversal(html, applicationJsonOut, true); // Angular Universal server side rendering.
                return Controller.Content(htmlUniversal, "text/html");
            }
            // Json API
            if (Controller.HttpContext.Request.Path == RoutePath + "Application.json")
            {
                string jsonInText = Util.StreamToString(Controller.Request.Body);
                ApplicationJson applicationJsonIn = Framework.Server.Json.Util.Deserialize<ApplicationJson>(jsonInText);
                ApplicationJson applicationJsonOut = ApplicationServer.Process(applicationJsonIn, Controller.HttpContext.Request.Path);
                applicationJsonOut.IsJsonGet = false;
                string jsonOutText = Framework.Server.Json.Util.Serialize(applicationJsonOut);
                if (Framework.Server.Config.Instance.IsDebugJson)
                {
                    applicationJsonOut.IsJsonGet = true;
                    string jsonOutDebug = Framework.Server.Json.Util.Serialize(applicationJsonOut);
                    Framework.Util.FileWrite(Framework.Util.FolderName + "Submodule/Client/Application.json", jsonOutDebug);
                }
                return Controller.Content(jsonOutText, "application/json");
            }
            // Framework/Server/wwwroot/
            {
                string fileName = Controller.HttpContext.Request.Path.ToString().Substring(RoutePath.Length);
                fileName = Server.Util.FileNameToWwwRoot(fileName);
                if (File.Exists(fileName))
                {
                    return Server.Util.FileNameToFileContentResult(Controller, fileName);
                }
            }
            // node_modules
            if (Controller.HttpContext.Request.Path.ToString().StartsWith("/node_modules/"))
            {
                return Util.FileGet(Controller, "", "../Client/", "Universal/");
            }
            // (*.css; *.js)
            if (Controller.HttpContext.Request.Path.ToString().EndsWith(".css") || Controller.HttpContext.Request.Path.ToString().EndsWith(".js"))
            {
                return Util.FileGet(Controller, RoutePath, "Universal/", "Universal/");
            }
            return Controller.NotFound();
        }

        /// <summary>
        /// Returns server side rendered index.html.
        /// </summary>
        private async Task<string> HtmlUniversal(string html, ApplicationJson applicationJson, bool isUniversal)
        {
            if (isUniversal == false)
            {
                return html;
            }
            else
            {
                string htmlUniversal = null;
                string url = "http://" + Controller.Request.Host.ToUriComponent() + "/Universal/index.js";
                applicationJson.IsBrowser = false; // Server side rendering mode.
                string jsonText = Framework.Server.Json.Util.Serialize(applicationJson);
                // Universal rendering
                {
                    if (Framework.Util.FolderNameIsIss)
                    {
                        // Running on IIS Server.
                        htmlUniversal = await Post(url, jsonText, false); // Call Angular Universal server side rendering service.
                    }
                    else
                    {
                        // Running in Visual Studio
                        url = "http://localhost:1337/"; // Call UniversalExpress when running in Visual Studio.
                        htmlUniversal = await Post(url, jsonText, true);
                    }
                }
                Framework.Util.Assert(htmlUniversal != "<app></app>"); // Catch java script errors. See UniversalExpress console for errors!
                //
                string result = null;
                // Replace <app> on index.html
                {
                    int indexBegin = htmlUniversal.IndexOf("<app>");
                    int indexEnd = htmlUniversal.IndexOf("</app>") + "</app>".Length;
                    string htmlUniversalClean = htmlUniversal.Substring(indexBegin, (indexEnd - indexBegin));
                    result = html.Replace("<app>Loading AppComponent content here ...</app>", htmlUniversalClean);
                }
                applicationJson.IsBrowser = true; // Client side rendering mode.
                string jsonTextBrowser = Framework.Server.Json.Util.Serialize(applicationJson);
                string resultAssert = result;
                // Add json to index.html (Client/index.html)
                {
                    string scriptFind = "System.import('app').catch(function(err){ console.error(err); });";
                    string scriptReplace = "var browserJson = " + jsonTextBrowser + "; " + scriptFind;
                    result = result.Replace(scriptFind, scriptReplace);
                }
                // Add json to index.html (Server/indexBundle.html)
                {
                    string scriptFind = "function downloadJSAtOnload() {";
                    string scriptReplace = "var browserJson = " + jsonTextBrowser + ";\r\n" + scriptFind;
                    result = result.Replace(scriptFind, scriptReplace);
                }
                Framework.Util.Assert(resultAssert != result, "Adding browserJson failed!");
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

        /// <summary>
        /// Returns index.html.
        /// </summary>
        /// <param name="isBundle">If true use Server/index.html else Client/index.html</param>
        private string IndexHtml(bool isBundle)
        {
            if (isBundle == false)
            {
                return System.IO.File.ReadAllText("Universal/index.html"); // Original source: Client/index.html
            }
            else
            {
                string fileName = Server.Util.FileNameToWwwRoot("indexBundle.html"); // Original source: Framework/Server/wwwroot/indexBundle.html
                return System.IO.File.ReadAllText(fileName);
            }
        }
    }
}
