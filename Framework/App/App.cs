namespace Framework.Application
{
    using Framework.Json;
    using Framework.Session;
    using Framework.Server;
    using Microsoft.AspNetCore.Http;
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Framework.Dal;

    public class App
    {
        /// <summary>
        /// Gets or sets AppJson. This is the application root json component being transferred between server and client.
        /// </summary>
        public AppJson AppJson { get; internal set; }

        /// <summary>
        /// Gets or sets AppSession. This is the application session state.
        /// </summary>
        internal AppSession AppSession { get; set; }

        /// <summary>
        /// Returns assembly and namespace to search for classes when deserializing json. (For example: "MyPage")
        /// </summary>
        virtual internal Type[] TypeComponentInNamespaceList()
        {
            return (new Type[] {
                GetType(), // Namespace of running application.
                typeof(App), // Used for example for class Navigation.
            }).Distinct().ToArray(); // Enable serialization of components in App and AppConfig namespace.
        }

        /// <summary>
        /// Called on first request.
        /// </summary>
        protected virtual async Task InitAsync()
        {
            await Task.Run(() => { });
        }

        protected internal async Task InitInternalAsync()
        {
            await InitAsync();
            UtilServer.Session.SetString("Main", string.Format("App start: {0}", UtilFramework.DateTimeToString(DateTime.Now.ToUniversalTime())));
        }

        protected virtual async Task ProcessAsync()
        {
            await Task.Run(() => { });
        }

        protected internal async Task ProcessInternalAsync()
        {
            await ProcessAsync();

            await UtilServer.App.AppSession.ProcessAsync();

            AppJson.Version = UtilFramework.Version;
            AppJson.VersionBuild = UtilFramework.VersionBuild;
            AppJson.Session = UtilServer.Session.Id;
            if (string.IsNullOrEmpty(AppJson.SessionApp))
            {
                AppJson.SessionApp = UtilServer.Session.Id;
            }
            AppJson.SessionState = UtilServer.Session.GetString("Main");

            if (UtilServer.Session.Id != AppJson.SessionApp) // Session expired!
            {
                AppJson.IsReload = true;
            }
        }

        protected virtual internal IQueryable GridLoadQuery(Grid grid)
        {
            return null;
        }
        
        protected virtual internal async Task GridRowSelectChangeAsync(Grid grid)
        {
            await Task.Run(() => { });
        }
    }

    public class AppSelector
    {
        public async Task<App> CreateAppAsync(HttpContext context)
        {
            var result = CreateApp();
            UtilServer.App = result;
            string json = await UtilServer.StreamToString(context.Request.Body);
            if (json != null) // If post
            {
                result.AppJson = UtilJson.Deserialize<AppJson>(json, result.TypeComponentInNamespaceList());
            }
            else
            {
                result.AppJson = new AppJson();
            }

            UtilSession.Deserialize(result); // Deserialize session or init.

            // User hit reload button in browser.
            if (result.AppJson.ResponseCount == 0 && result.AppSession.ResponseCount > 0)
            {
                result.AppJson = new AppJson();
                result.AppSession = new AppSession();
            }

            // Init
            if (result.AppJson.IsInit == false)
            {
                int requestCount = result.AppJson.RequestCount;
                string browserUrl = result.AppJson.BrowserUrl;
                result.AppJson = new AppJson(); // Reset
                result.AppJson.RequestCount = requestCount;
                result.AppJson.BrowserUrl = browserUrl;
                result.AppJson.IsInit = true;
                await result.InitInternalAsync();
            }

            // Process
            await result.ProcessInternalAsync();

            // RequestUrl
            result.AppJson.RequestUrl = UtilServer.RequestUrl(false);

            return result;
        }

        /// <summary>
        /// Override this method define App.
        /// </summary>
        protected virtual App CreateApp()
        {
            return new App(); // DefaultApp
        }
    }
}
