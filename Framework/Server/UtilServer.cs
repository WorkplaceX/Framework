namespace Framework.Server
{
    using Framework.Application;
    using Framework.Json;
    using Framework.Session;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;

    internal class UtilServer
    {
        [ThreadStatic]
        public static IApplicationBuilder ApplicationBuilder;

        public static IHostingEnvironment HostingEnvironment
        {
            get
            {
                IHostingEnvironment result = null;
                HttpContext context = Context;
                if (context != null)
                {
                    result = (IHostingEnvironment)context.RequestServices.GetService(typeof(IHostingEnvironment));
                }
                else
                {
                    if (ApplicationBuilder != null)
                    {
                        result = (IHostingEnvironment)ApplicationBuilder.ApplicationServices.GetService(typeof(IHostingEnvironment));
                    }
                }
                return result;
            }
        }

        /// <summary>
        /// Gets Context of web request.
        /// </summary>
        public static HttpContext Context
        {
            get
            {
                return new HttpContextAccessor().HttpContext; // Not available during startup. See also: method ConfigureServices();
            }
        }

        public static ISession Session
        {
            get
            {
                return Context.Session;
            }
        }

        /// <summary>
        /// Returns location of ASP.NET server wwwroot folder.
        /// </summary>
        public static string FolderNameContentRoot()
        {
            return new Uri(HostingEnvironment.ContentRootPath).AbsolutePath + "/";
        }

        /// <summary>
        /// Returns request url.
        /// </summary>
        /// <param name="isServer">If true, it returns root url "http://localhost:49323/". If false, it returns "http://localhost:49323/config/data.txt".</param>
        public static string RequestUrl(bool isServer)
        {
            string result = null;
            HttpContext context = Context;
            if (isServer)
            {
                result = string.Format("{0}://{1}/", context.Request.Scheme, context.Request.Host.Value);
            }
            else
            {
                result = string.Format("{0}://{1}{2}", context.Request.Scheme, context.Request.Host.Value, context.Request.Path);
            }
            return result;
        }

        /// <summary>
        /// Returns html content type.
        /// </summary>
        public static string ContentType(string fileName)
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
                case ".png": result = "image/png"; break;
                case ".ico": result = "image/x-icon"; break;
                case ".jpg": result = "image/jpeg"; break;
                case ".pdf": result = "application/pdf"; break;
                case ".json": result = "application/json"; break;
                default:
                    result = "text/plain"; break; // Type not found!
            }
            return result;
        }

        /// <summary>
        /// Returns true, if application runs on IIS server.
        /// </summary>
        /// <returns></returns>
        public static bool IsIssServer
        {
            get
            {
                string folderName = UtilFramework.FolderNameGet() + "Application.Server/";
                bool result = Directory.Exists(folderName) == false;
                return result;
            }
        }

        /// <summary>
        /// Start Universal server. Detects Visual Studio environment.
        /// </summary>
        public static void StartUniversalServer()
        {
            string folderName = UtilFramework.FolderName + "Application.Server/Framework/dist/";
            string fileNameServer = folderName + "server.js";
            if (!File.Exists(fileNameServer))
            {
                throw new Exception(string.Format("File does not exis! Make sure cli build did run. ({0})", fileNameServer));
            }
            ProcessStartInfo info = new ProcessStartInfo();
            info.WorkingDirectory = folderName;
            info.FileName = "node.exe";
            info.Arguments = "server.js";
            info.UseShellExecute = true; // Open additional node window.
            // info.Environment.Add("PORT", "4000"); // Does not work in connection with "info.UseShellExecute = true;". For default port see also: Submodule/Client/src/server.ts
            info.WindowStyle = ProcessWindowStyle.Minimized; // Show node window minimized.
            Process.Start(info);
        }

        /// <summary>
        /// Used to get body of web post.
        /// </summary>
        public static async Task<string> StreamToString(Stream stream)
        {
            string result;
            using (var streamReader = new StreamReader(stream))
            {
                result = await streamReader.ReadToEndAsync();
            }
            if (result == "")
            {
                result = null;
            }
            return result;
        }

        /// <summary>
        /// Returns true, if request is a FileName. Otherwise request is a FolderName.
        /// </summary>
        public static bool PathIsFileName(string path)
        {
            return !string.IsNullOrEmpty(Path.GetFileName(path));
        }

        /// <summary>
        /// Post to json url.
        /// </summary>
        public static async Task<string> WebPost(string url, string json)
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
                response.EnsureSuccessStatusCode();
                string result = await response.Content.ReadAsStringAsync();
                return result;
            }
        }

        /// <summary>
        /// Gets currently processed app.
        /// </summary>
        public static AppInternal AppInternal
        {
            get
            {
                InstanceService instanceService = (InstanceService)UtilServer.Context.RequestServices.GetService(typeof(InstanceService));
                return instanceService.AppInternal;
            }
            set
            {
                InstanceService instanceService = (InstanceService)UtilServer.Context.RequestServices.GetService(typeof(InstanceService));
                UtilFramework.Assert(instanceService.AppInternal == null);
                instanceService.AppInternal = value;
            }
        }

        /// <summary>
        /// Gets AppSession. Server side session state of currently processed app.
        /// </summary>
        public static AppSession AppSession
        {
            get
            {
                return AppInternal.AppSession;
            }
        }

        /// <summary>
        /// Gets AppJson. Currently processed app.
        /// </summary>
        public static AppJson AppJson
        {
            get
            {
                return AppInternal.AppJson;
            }
        }

        public class InstanceService
        {
            public AppInternal AppInternal;
        }
    }
}
