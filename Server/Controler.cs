namespace Server
{
    using Framework.Server.Application;
    using Microsoft.AspNetCore.Mvc;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;

    public class WebController : Controller
    {
        // const string path = "/web/"; // Run with root debug page.
        const string path = "/"; // Run direct.

        [Route(path + "{*uri}")]
        public async Task<IActionResult> Web()
        {
            // Html
            if (HttpContext.Request.Path == path)
            {
                JsonApplication jsonApplicationOut = new Application.ApplicationX().Process(null, HttpContext.Request.Path);
                string htmlUniversal = null;
                string html = IndexHtml(true);
                htmlUniversal = await HtmlUniversal(html, jsonApplicationOut, true); // Angular Universal server side rendering.
                return Content(htmlUniversal, "text/html");
            }
            // Json API
            if (HttpContext.Request.Path == path + "Application.json")
            {
                string jsonInText = Util.StreamToString(Request.Body);
                JsonApplication jsonApplicationIn = Framework.Server.Json.Util.Deserialize<JsonApplication>(jsonInText);
                JsonApplication jsonApplicationOut = new Application.ApplicationX().Process(jsonApplicationIn, HttpContext.Request.Path);
                jsonApplicationOut.IsJsonGet = false;
                string jsonOutText = Framework.Server.Json.Util.Serialize(jsonApplicationOut);
                if (Framework.Server.Config.Instance.IsDebugJson)
                {
                    jsonApplicationOut.IsJsonGet = true;
                    string jsonOutDebug = Framework.Server.Json.Util.Serialize(jsonApplicationOut);
                    Framework.Util.FileWrite(Framework.Util.FolderName + "Submodule/Client/Application.json", jsonOutDebug);
                }
                return Content(jsonOutText, "application/json");
            }
            // node_modules
            if (HttpContext.Request.Path.ToString().StartsWith("/node_modules/"))
            {
                return Util.FileGet(this, "", "../Client/", "Universal/");
            }
            // (*.css; *.js)
            if (HttpContext.Request.Path.ToString().EndsWith(".css") || HttpContext.Request.Path.ToString().EndsWith(".js"))
            {
                return Util.FileGet(this, "", "Universal/", "Universal/");
            }
            return NotFound();
        }

        /// <summary>
        /// Returns server side rendered index.html.
        /// </summary>
        private async Task<string> HtmlUniversal(string html, JsonApplication jsonApplication, bool isUniversal)
        {
            if (isUniversal == false)
            {
                return html;
            }
            else
            {
                string htmlUniversal = null;
                string url = "http://" + Request.Host.ToUriComponent() + "/Universal/index.js";
                jsonApplication.IsBrowser = false; // Server side rendering mode.
                string jsonText = Framework.Server.Json.Util.Serialize(jsonApplication);
                htmlUniversal = await Post(url, jsonText, false); // Call Angular Universal server side rendering service.
                if (htmlUniversal == null)
                {
                    url = "http://localhost:1337/"; // Application not running on IIS. Divert to UniversalExpress when running in Visual Studio.
                    htmlUniversal = await Post(url, jsonText, true);
                    Util.Assert(htmlUniversal != "<app></app>"); // Catch java script errors. See UniversalExpress console for errors!
                }
                //
                string result = null;
                // Replace <app> on index.html
                {
                    int indexBegin = htmlUniversal.IndexOf("<app>");
                    int indexEnd = htmlUniversal.IndexOf("</app>") + "</app>".Length;
                    string htmlUniversalClean = htmlUniversal.Substring(indexBegin, (indexEnd - indexBegin));
                    result = html.Replace("<app>Loading AppComponent content here ...</app>", htmlUniversalClean);
                }
                jsonApplication.IsBrowser = true; // Client side rendering mode.
                string jsonTextBrowser = Framework.Server.Json.Util.Serialize(jsonApplication);
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
                Util.Assert(resultAssert != result, "Adding browserJson failed!");
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
                return System.IO.File.ReadAllText("indexBundle.html"); // Original source: Client/index.html
            }
        }
    }
}
