namespace Framework.Application
{
    using Framework.ComponentJson;
    using Framework.Json;
    using Framework.Server;
    using Microsoft.AspNetCore.Http;
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    public class App
    {
        /// <summary>
        /// Gets or sets AppJson. This is the application root json component being transferred between server and client.
        /// </summary>
        public AppJson AppJson { get; internal set; }

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
        protected virtual void Init()
        {

        }

        protected internal void InitInternal()
        {
            Init();
            UtilServer.Session.SetString("Main", string.Format("App start: {0}", UtilFramework.DateTimeToString(DateTime.Now.ToUniversalTime())));
        }

        protected virtual void Process()
        {

        }

        protected internal void ProcessInternal()
        {
            Process();

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
    }

    public class AppSelector
    {
        public async Task<App> CreateApp(HttpContext context)
        {
            var result = CreateApp();
            string json = await UtilServer.StreamToString(context.Request.Body);
            if (json != null) // If post
            {
                result.AppJson = UtilJson.Deserialize<AppJson>(json, result.TypeComponentInNamespaceList());
            }
            else
            {
                result.AppJson = new AppJson();
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
                result.InitInternal();
            }

            // Process
            result.ProcessInternal();

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
