namespace Framework.Server
{
    using Microsoft.AspNetCore.Hosting;
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;

    public class UtilServer
    {
        /// <summary>
        /// Returns location of ASP.NET server wwwroot folder.
        /// </summary>
        internal static string FolderNameContentRoot(IHostingEnvironment hostingEnvironment)
        {
            return new Uri(hostingEnvironment.ContentRootPath).AbsolutePath + "/";
        }

        /// <summary>
        /// Returns html content type.
        /// </summary>
        internal static string ContentType(string fileName)
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
                default:
                    result = "text/plain"; break; // Type not found!
            }
            return result;
        }

        /// <summary>
        /// Returns true, if application runs on IIS server.
        /// </summary>
        /// <returns></returns>
        internal static bool IsIssServer
        {
            get
            {
                string folderName = UtilFramework.FolderName + "Application.Server/";
                bool result = Directory.Exists(folderName) == false;
                return result;
            }
        }

        /// <summary>
        /// Start Universal server. Detects Visual Studio environment.
        /// </summary>
        internal static void StartUniversalServer()
        {
            string folderName = UtilFramework.FolderName + "Application.Server/Framework/dist/";
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
        /// Returns true, if request is a FileName.
        /// </summary>
        internal static bool PathIsFileName(string path)
        {
            return !string.IsNullOrEmpty(Path.GetFileName(path));
        }

        /// <summary>
        /// Post to json url.
        /// </summary>
        internal static async Task<string> WebPost(string url, string json)
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
    }
}
