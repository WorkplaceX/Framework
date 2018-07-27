namespace Framework.Server
{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using System.IO;
    using System.Threading.Tasks;

    public static class Startup
    {
        public static void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
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

        private async Task<bool> ServeFrameworkFile(HttpContext context)
        {
            bool result = false;
            string fileName = UtilServer.FolderNameWwwroot(Env) + "framework" + context.Request.Path;
            if (File.Exists(fileName))
            {
                await context.Response.SendFileAsync(fileName);
                result = true;
            }
            return result;
        }
    }
}
