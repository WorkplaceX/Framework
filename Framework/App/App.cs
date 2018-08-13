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
    using Framework.App;

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

        internal async Task ProcessInternalAsync()
        {
            AppJson.SessionState = UtilServer.Session.GetString("Main");
            await UtilServer.App.AppSession.ProcessAsync(); // Grid
            await UtilApp.ProcessAsync(); // Button
            await ProcessAsync(); // Custom
        }

        protected virtual internal IQueryable GridLoadQuery(Grid grid)
        {
            return null;
        }
        
        protected virtual internal async Task GridRowSelectChangeAsync(Grid grid)
        {
            await Task.Run(() => { });
        }

        protected virtual internal async Task ButtonClickAsync(Button button)
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
            int requestCountAssert = result.AppJson.RequestCount;

            UtilSession.Deserialize(result); // Deserialize session or init.

            // User hit reload button in browser.
            bool isBrowserRefresh = (result.AppJson.ResponseCount == 0 && result.AppSession.ResponseCount > 0);

            // User has app open in two browser tabs.
            bool isBrowserTabSwitch = (result.AppJson.ResponseCount != result.AppSession.ResponseCount);

            // Init
            if (result.AppJson.IsInit == false || isBrowserRefresh || isBrowserTabSwitch)
            {
                int requestCount = result.AppJson.RequestCount;
                int responseCount = result.AppSession.ResponseCount;
                string browserUrl = result.AppJson.BrowserUrl;
                result.AppJson = new AppJson(); // Reset
                result.AppJson.RequestCount = requestCount;
                result.AppJson.ResponseCount = responseCount;
                result.AppJson.BrowserUrl = browserUrl;
                result.AppJson.IsInit = true;
                await result.InitInternalAsync();
            }

            UtilFramework.Assert(result.AppJson.ResponseCount == result.AppSession.ResponseCount, "Request mismatch!");

            // Process
            await result.ProcessInternalAsync();

            CreateApp(result); // Version tag

            UtilFramework.Assert(result.AppJson.RequestCount == requestCountAssert); // Incoming and outgoing RequestCount has to be identical!

            return result;
        }

        private void CreateApp(App app)
        {
            // Version
            app.AppJson.Version = UtilFramework.Version;
            app.AppJson.VersionBuild = UtilFramework.VersionBuild;

            // Session
            app.AppJson.Session = UtilServer.Session.Id;
            if (string.IsNullOrEmpty(app.AppJson.SessionApp))
            {
                app.AppJson.SessionApp = UtilServer.Session.Id;
            }

            // IsReload
            if (UtilServer.Session.Id != app.AppJson.SessionApp) // Session expired!
            {
                app.AppJson.IsReload = true;
            }

            // RequestUrl
            app.AppJson.RequestUrl = UtilServer.RequestUrl(false);
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
