namespace Framework.Server
{
    using Framework.Component;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using System.Diagnostics;
    using System.IO;
    using System.Threading.Tasks;

    public static class Startup
    {
        public static void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (UtilServer.IsIssServer == false)
            {
                if (ConfigFramework.Load().IsServerSideRendering)
                {
                    UtilServer.StartUniversalServer();
                }
            }

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseDefaultFiles(); // Used for index.html
            app.UseStaticFiles(); // Enable access to files in folder wwwwroot.

            app.Run(new StartupRun(app, env).Run);
        }
    }

    internal class StartupRun
    {
        public StartupRun(IApplicationBuilder app, IHostingEnvironment env)
        {
            this.App = app;
            this.Env = env;
        }

        public readonly IApplicationBuilder App;

        public readonly IHostingEnvironment Env;

        public async Task Run(HttpContext context)
        {
            bool result = await ServeFrameworkFile(context);

            if (result == false)
            {
                context.Response.StatusCode = 404; // Not found
            }

            return;
        }

        private async Task ServerSideRendering(HttpContext context, string path)
        {
            context.Response.ContentType = UtilServer.ContentType(path);
            string url;
            if (UtilServer.IsIssServer)
            {
                // Running on IIS Server.
                url = "http://" + context.Request.Host.ToUriComponent() + "/Framework/dist/server.js";
            }
            else
            {
                // Running in Visual Studio
                url = "http://localhost:4000/"; // Call Universal server when running in Visual Studio.
            }
            string isCustomIndexHtml = ConfigFramework.Load().IsCustomIndexHtml ? "true" : "false";
            url += "?IsCustomIndexHtml=" + isCustomIndexHtml;
            App app = new App(null);
            string json = UtilJson.Serialize(app);
            string htmlServerSideRendering = await UtilServer.WebPost(url, json);

            // Set jsonBrowser in html.
            string scriptFind = "var jsonBrowser = {};";
            string scriptReplace = "var jsonBrowser = " + json + ";";
            htmlServerSideRendering = UtilFramework.Replace(htmlServerSideRendering, scriptFind, scriptReplace);

            context.Response.ContentType = UtilServer.ContentType(path);
            await context.Response.WriteAsync(htmlServerSideRendering);
        }

        /// <summary>
        /// Returns true, if file found in "framework/dist/browser" folder.
        /// </summary>
        private async Task<bool> ServeFrameworkFile(HttpContext context)
        {
            string path = context.Request.Path;

            // index.html
            if (UtilServer.PathIsFileName(path) == false)
            {
                path = "/index.html";
            }

            bool requestIsFileName = UtilServer.PathIsFileName(path);

            if (path == "/index.html" && ConfigFramework.Load().IsServerSideRendering)
            {
                await ServerSideRendering(context, path);
                return true;
            }
            else
            {
                if (requestIsFileName)
                {
                    if (path == "/index.html" && ConfigFramework.Load().IsCustomIndexHtml)
                    {
                        string fileNameIndexHtml = UtilServer.FolderNameContentRoot(Env) + "Framework" + path;
                        context.Response.ContentType = UtilServer.ContentType(fileNameIndexHtml);
                        await context.Response.SendFileAsync(fileNameIndexHtml);
                        return true;
                    }
                    else
                    {
                        // Serve fileName
                        string fileName = UtilServer.FolderNameContentRoot(Env) + "Framework/dist/browser" + path;
                        if (File.Exists(fileName))
                        {
                            context.Response.ContentType = UtilServer.ContentType(fileName);
                            await context.Response.SendFileAsync(fileName);
                            return true;
                        }
                    }
                }
            }

            return false;
        }
    }
}
