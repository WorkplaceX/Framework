namespace Framework.Server
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    //
    using Framework.Server.Application;

    public static class Util
    {
        public static async Task<IActionResult> ControllerWebRequest(ControllerBase controller, string routePath, ApplicationBase application)
        {
            return await new WebController(controller, routePath, application).Web();
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
        /// Copy file from source to dest and serve it.
        /// </summary>
        /// <param name="controller">WebApi controller</param>
        /// <param name="requestFolderName">For example: MyApp/</param>
        /// <param name="folderNameSourceRelative">For example ../Angular/</param>
        /// <param name="folderNameDestRelative">For example Application/Nodejs/Client/</param>
        public static FileContentResult FileGet(ControllerBase controller, string requestFolderName, string folderNameSourceRelative, string folderNameDestRelative)
        {
            FileContentResult result = null;
            string requestFileName = controller.Request.Path.Value;
            string requestFolderNameMatch = requestFileName;
            if (!requestFolderNameMatch.EndsWith("/"))
            {
                requestFolderNameMatch += "/";
            }
            if (requestFolderName.StartsWith("/"))
            {
                requestFolderName = requestFolderName.Substring(1);
            }
            if (requestFolderNameMatch.StartsWith("/" + requestFolderName))
            {
                requestFileName = requestFileName.Substring(requestFolderName.Length + 1);
                Uri folderName = new Uri(Directory.GetCurrentDirectory() + "/");
                Uri folderNameSource = new Uri(folderName, folderNameSourceRelative);
                Uri folderNameDest = new Uri(folderName, folderNameDestRelative);
                Uri fileNameSource = new Uri(folderNameSource, requestFileName);
                Uri fileNameDest = new Uri(folderNameDest, requestFileName);
                // ContentType
                string fileNameExtension = Path.GetExtension(fileNameSource.LocalPath);
                string contentType; // https://wiki.selfhtml.org/wiki/Referenz:MIME-Typen
                switch (fileNameExtension)
                {
                    case ".html": contentType = "text/html"; break;
                    case ".css": contentType = "text/css"; break;
                    case ".js": contentType = "text/javascript"; break;
                    case ".map": contentType = "text/plain"; break;
                    case ".scss": contentType = "text/plain"; break; // Used only if internet explorer is in debug mode!
                    default:
                        throw new Exception("Unknown!");
                }
                // Copye from source to dest
                if (File.Exists(fileNameSource.LocalPath) && !File.Exists(fileNameDest.LocalPath))
                {
                    string folderNameCopy = Directory.GetParent(fileNameDest.LocalPath).ToString();
                    if (!Directory.Exists(folderNameCopy))
                    {
                        Directory.CreateDirectory(folderNameCopy);
                    }
                    File.Copy(fileNameSource.LocalPath, fileNameDest.LocalPath);
                }
                // Serve dest
                var byteList = File.ReadAllBytes(fileNameDest.LocalPath);
                result = controller.File(byteList, contentType);
            }
            return result;
        }

        /// <summary>
        /// Returns FileContentResult.
        /// </summary>
        public static FileContentResult FileNameToFileContentResult(ControllerBase controller, string fileName)
        {
            // ContentType
            string fileNameExtension = Path.GetExtension(fileName);
            string contentType; // https://wiki.selfhtml.org/wiki/Referenz:MIME-Typen
            switch (fileNameExtension)
            {
                case ".html": contentType = "text/html"; break;
                case ".css": contentType = "text/css"; break;
                case ".js": contentType = "text/javascript"; break;
                case ".map": contentType = "text/plain"; break;
                case ".scss": contentType = "text/plain"; break; // Used only if internet explorer is in debug mode!
                case ".png": contentType = "image/png"; break;
                default:
                    throw new Exception("Unknown!");
            }
            // Read file
            var byteList = File.ReadAllBytes(fileName);
            var result = controller.File(byteList, contentType);
            return result;
        }

        /// <summary>
        /// Returns FileName in Framework/Server/wwwroot/ folder.
        /// </summary>
        /// <param name="fileName">For example: index.html</param>
        public static string FileNameToWwwRoot(string fileName)
        {
            if (Framework.Util.FolderNameIsIss == false)
            {
                fileName = Framework.Util.FolderName + "Submodule/Framework/Server/wwwroot/" + fileName;
            }
            else
            {
                fileName = Framework.Util.FolderName + "Server/wwwroot/" + fileName;
            }
            return fileName;
        }
    }
}
