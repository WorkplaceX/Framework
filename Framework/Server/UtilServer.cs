namespace Framework.Server
{
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using System;

    public class UtilServer
    {
        /// <summary>
        /// Returns location of ASP.NET server wwwroot folder.
        /// </summary>
        public static string FolderNameContentRoot(IHostingEnvironment hostingEnvironment)
        {
            return new Uri(hostingEnvironment.ContentRootPath).AbsolutePath + "/";
        }
    }
}
