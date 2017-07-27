namespace Framework.Server
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    //
    using Framework.Application;

    public static class UtilServer
    {
        public static async Task<IActionResult> ControllerWebRequest(ControllerBase controller, string routePath, App app)
        {
            return await new WebController(controller, routePath, app).WebRequest();
        }

        public static async Task<IActionResult> ControllerWebRequest(ControllerBase controller, string controllerPath, AppSelector appSelector)
        {
            string requestPathBase;
            App app = appSelector.Create(controller, controllerPath, out requestPathBase);
            return await new WebController(controller, requestPathBase, app).WebRequest();
        }

        public static string StreamToString(Stream stream)
        {
            using (var streamReader = new StreamReader(stream))
            {
                return streamReader.ReadToEnd();
            }
        }

        /// <summary>
        /// Uri for Windows and Linux.
        /// </summary>
        public class Uri
        {
            public Uri(string uriString)
            {
                if (uriString.StartsWith("/"))
                {
                    this.isLinux = true;
                    uriString = "Linux:" + uriString;
                }
                this.uriSystem = new System.Uri(uriString);
            }

            public Uri(Uri baseUri, string relativeUri)
            {
                this.uriSystem = new System.Uri(baseUri.uriSystem, relativeUri);
            }

            private readonly bool isLinux;

            private readonly System.Uri uriSystem;

            public string LocalPath
            {
                get
                {
                    string result = uriSystem.LocalPath;
                    if (isLinux)
                    {
                        result = result.Substring("Linux:".Length);
                    }
                    return result;
                }
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
        public static FileContentResult FileNameToFileContentResult(ControllerBase controller, string fileName)
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
        public static FileContentResult FileNameToFileContentResult(ControllerBase controller, string fileName, byte[] data)
        {
            string contentType = FileNameToFileContentType(fileName);
            return controller.File(data, contentType);
        }

        /// <summary>
        /// Returns FolderName Framework/Server/. Different folder if running on IIS.
        /// </summary>
        public static string FolderNameFrameworkServer()
        {
            if (Framework.UtilFramework.FolderNameIsIss)
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
        public static string FolderNameServer()
        {
            if (Framework.UtilFramework.FolderNameIsIss)
            {
                return UtilFramework.FolderName;
            }
            else
            {
                return UtilFramework.FolderName + "Server/";
            }
        }

        public static string FileNameIndex()
        {
            string result = FolderNameFrameworkServer() + "wwwroot/" + "index.html";
            string fileNameOverwrite = FolderNameServer() + "wwwroot/" + "index.html";
            if (File.Exists(fileNameOverwrite))
            {
                return fileNameOverwrite;
            }
            return result;
        }
    }
}
