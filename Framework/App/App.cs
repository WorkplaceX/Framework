namespace Framework.Application
{
    using Framework.Json;
    using Framework.Session;
    using Framework.Server;
    using Microsoft.AspNetCore.Http;
    using System;
    using System.Threading.Tasks;
    using System.Linq;

    internal class AppInternal
    {
        /// <summary>
        /// Gets or sets AppJson. This is the application root json component being transferred between server and client.
        /// </summary>
        public AppJson AppJson { get; internal set; }

        /// <summary>
        /// Gets or sets AppSession. This is the application session state.
        /// </summary>
        internal AppSession AppSession { get; set; }

        public Type[] TypeComponentInNamespaceList;
    }

    public class AppSelector
    {
        internal async Task<AppInternal> CreateAppAndProcessAsync(HttpContext context)
        {
            var result = new AppInternal();
            UtilServer.AppInternal = result;
            result.TypeComponentInNamespaceList = TypeComponentInNamespaceList();
            string json = await UtilServer.StreamToString(context.Request.Body);
            if (json != null) // If post
            {
                result.AppJson = UtilJson.Deserialize<AppJson>(json, result.TypeComponentInNamespaceList);
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
                string embeddedUrl = result.AppJson.EmbeddedUrl;
                result.AppJson = CreateAppJson(); // Reset
                result.AppSession = new AppSession(); // Reset
                result.AppJson.RequestCount = requestCount;
                result.AppJson.ResponseCount = responseCount;
                result.AppSession.ResponseCount = responseCount;
                result.AppJson.BrowserUrl = browserUrl;
                result.AppJson.EmbeddedUrl = embeddedUrl;
                result.AppJson.IsInit = true;
                await result.AppJson.InitInternalAsync();
            }

            UtilFramework.Assert(result.AppJson.ResponseCount == result.AppSession.ResponseCount, "Request mismatch!");

            // Process
            await result.AppJson.ProcessInternalAsync();

            RenderVersion(result.AppJson); // Version tag

            UtilFramework.Assert(result.AppJson.RequestCount == requestCountAssert); // Incoming and outgoing RequestCount has to be identical!

            return result;
        }

        private void RenderVersion(AppJson appJson)
        {
            // Version
            appJson.Version = UtilFramework.Version;
            appJson.VersionBuild = UtilFramework.VersionBuild;

            // Session
            appJson.Session = UtilServer.Session.Id;
            if (string.IsNullOrEmpty(appJson.SessionApp))
            {
                appJson.SessionApp = UtilServer.Session.Id;
            }

            // IsReload
            if (UtilServer.Session.Id != appJson.SessionApp) // Session expired!
            {
                appJson.IsReload = true;
            }

            // RequestUrl
            appJson.RequestUrl = UtilServer.RequestUrl(false);
        }

        private AppJson CreateAppJson()
        {
            Type type = TypeAppJson();
            AppJson result = (AppJson)Activator.CreateInstance(type, null);
            return result;
        }

        /// <summary>
        /// Returns type of AppJson. Used for first html request.
        /// </summary>
        protected virtual Type TypeAppJson()
        {
            return typeof(AppJson);
        }

        /// <summary>
        /// Returns assembly and namespace to search for classes when deserializing json. (For example: "MyPage")
        /// </summary>
        virtual internal Type[] TypeComponentInNamespaceList()
        {
            return (new Type[] {
                GetType(), // Namespace of running application.
                typeof(AppJson), // For example button.
            }).Distinct().ToArray(); // Enable serialization of components in App and AppConfig namespace.
        }
    }
}
