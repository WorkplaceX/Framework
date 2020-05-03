﻿namespace Framework.Server
{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Net.Http;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading.Tasks;

    internal class UtilServer
    {
        public static void Cors()
        {
            string origin = Context.Request.Headers["Origin"];
            if (origin != null)
            {
                Context.Response.Headers.Add("Access-Control-Allow-Origin", origin); // Prevent browser error: blocked by CORS policy.
                Context.Response.Headers.Add("Access-Control-Allow-Credentials", "true");
            }
        }

        [ThreadStatic]
        public static IApplicationBuilder ApplicationBuilder;

        public static IWebHostEnvironment HostingEnvironment
        {
            get
            {
                IWebHostEnvironment result = null;
                HttpContext context = Context;
                if (context != null)
                {
                    result = (IWebHostEnvironment)context.RequestServices.GetService(typeof(IWebHostEnvironment));
                }
                else
                {
                    if (ApplicationBuilder != null)
                    {
                        result = (IWebHostEnvironment)ApplicationBuilder.ApplicationServices.GetService(typeof(IWebHostEnvironment));
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
        /// Returns client request url. For example: "http://localhost:49323/".
        /// </summary>
        public static string RequestUrl()
        {
            HttpContext context = Context;
            string result = string.Format("{0}://{1}/", context.Request.Scheme, context.Request.Host.Value);
            // result = string.Format("{0}://{1}{2}", context.Request.Scheme, context.Request.Host.Value, context.Request.Path); // Returns also path. For example: "http://localhost:49323/config/data.txt"
            return result;
        }

        /// <summary>
        /// Returns client request domain name. For example "localhost". Does not include http, https or port.
        /// </summary>
        public static string RequestDomainName()
        {
            return Context.Request.Host.Host;
        }

        /// <summary>
        /// Returns html content type.
        /// </summary>
        public static string ContentType(string fileName)
        {
            // ContentType
            string fileNameExtension = UtilFramework.FileNameExtension(fileName);
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
                case ".xlsx": result = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"; break;
                default:
                    result = "text/plain"; break; // Type not found!
            }
            return result;
        }

        /// <summary>
        /// Returns true, if application runs on IIS server. Otherwise it runs from Visual Studio.
        /// </summary>
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
            string folderName = UtilFramework.FolderName + "Application.Server/Framework/";
            string fileNameServer = folderName + "Framework.Angular/server/main.js";
            if (!File.Exists(fileNameServer))
            {
                throw new Exception(string.Format("File does not exis! Make sure cli build did run. ({0})", fileNameServer));
            }
            ProcessStartInfo info = new ProcessStartInfo
            {
                WorkingDirectory = folderName,
                FileName = "node.exe"
            };
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                info.FileName = "node";
            }
            info.Arguments = "Framework.Angular/server/main.js";
            info.UseShellExecute = true; // Open additional node window.
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
        /// Returns true, if request or response path is a FileName. Otherwise path is a FolderName.
        /// </summary>
        public static bool PathIsFileName(string path)
        {
            return !string.IsNullOrEmpty(Path.GetFileName(path));
        }

        /// <summary>
        /// Returns for example: "/cms/install/"
        /// </summary>
        /// <param name="pathBase">For example: "/cms/".</param>
        /// <param name="path">For example: "/install/".</param>
        public static string Path(string pathBase, string path)
        {
            if (!pathBase.EndsWith("/"))
            {
                pathBase += "/";
            }
            if (path.StartsWith("/"))
            {
                path = path.Substring("/".Length);
            }
            return pathBase + path;
        }

        /// <summary>
        /// Post to json url.
        /// </summary>
        public static async Task<string> WebPost(string url, string json)
        {
            using HttpClient client = new HttpClient();
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
}
