namespace Framework.Application
{
    using Framework.Json;
    using Framework.Session;
    using Framework.Server;
    using Microsoft.AspNetCore.Http;
    using System;
    using System.Threading.Tasks;
    using System.Linq;
    using Framework.Config;
    using System.Collections.Generic;

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

    /// <summary>
    /// Create AppJson or deserialize from server session. Process request. Serialize AppJson to server session and angular client.
    /// </summary>
    internal class AppSelector
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public AppSelector()
        {
            List<ConfigWebServerWebsite> result = new List<ConfigWebServerWebsite>();
            string requestDomainName = UtilServer.RequestDomainName();
            var config = ConfigWebServer.Load();
            foreach (var website in config.WebsiteList)
            {
                foreach (string domainName in website.DomainNameList)
                {
                    if (domainName == requestDomainName)
                    {
                        result.Add(website);
                    }
                }
            }

            // Make sure Website has been found
            if (result.Count == 0)
            {
                throw new Exception(string.Format("Website not found! See also: ConfigWebServer.json ({0})", requestDomainName));
            }
            if (result.Count > 1)
            {
                throw new Exception(string.Format("More than one website found! See also: ConfigWebServer.json ({0})", requestDomainName));
            }

            this.Website = result.Single();
        }

        /// <summary>
        /// Gets Website. This is the currently requested Website.
        /// </summary>
        public readonly ConfigWebServerWebsite Website;

        /// <summary>
        /// Returns JsonClient. Create AppJson and process request.
        /// </summary>
        internal async Task<string> CreateAppAndProcessAsync(HttpContext context)
        {
            var appInternal = new AppInternal();
            UtilServer.AppInternal = appInternal;
            appInternal.TypeComponentInNamespaceList = TypeComponentInNamespaceList();
            string json = await UtilServer.StreamToString(context.Request.Body);
            if (json != null && !json.Contains("EmbeddedUrl")) // If post
            {
                appInternal.AppJson = UtilJson.Deserialize<AppJson>(json, appInternal.TypeComponentInNamespaceList);
                appInternal.AppJson.IsSessionExpired = false;
            }
            else
            {
                appInternal.AppJson = CreateAppJson();
            }
            int requestCountAssert = appInternal.AppJson.RequestCount;

            UtilSession.Deserialize(appInternal); // Deserialize session or init.

            // User hit reload button in browser.
            bool isBrowserRefresh = (appInternal.AppJson.ResponseCount == 0 && appInternal.AppSession.ResponseCount > 0);

            // User has app open in two browser tabs.
            bool isBrowserTabSwitch = (appInternal.AppJson.ResponseCount != appInternal.AppSession.ResponseCount);

            // Session expired
            bool isSessionExpired = (appInternal.AppSession.ResponseCount == 0 && appInternal.AppJson.ResponseCount > 0);

            // Init
            if (appInternal.AppJson.IsInit == false || isBrowserRefresh || isBrowserTabSwitch || isSessionExpired)
            {
                int requestCount = appInternal.AppJson.RequestCount;
                int responseCount = appInternal.AppSession.ResponseCount;
                string browserUrl = appInternal.AppJson.BrowserUrl;
                string embeddedUrl = appInternal.AppJson.EmbeddedUrl;
                appInternal.AppJson = CreateAppJson(); // Reset
                appInternal.AppSession = new AppSession(); // Reset
                appInternal.AppJson.RequestCount = requestCount;
                appInternal.AppJson.ResponseCount = responseCount;
                appInternal.AppSession.ResponseCount = responseCount;
                appInternal.AppJson.BrowserUrl = browserUrl;
                appInternal.AppJson.EmbeddedUrl = embeddedUrl;
                appInternal.AppJson.RequestUrl = UtilServer.RequestUrl();
                appInternal.AppJson.IsInit = true;
                appInternal.AppJson.IsSessionExpired = isSessionExpired;
                await appInternal.AppJson.InitInternalAsync();
            }

            UtilFramework.Assert(appInternal.AppJson.ResponseCount == appInternal.AppSession.ResponseCount, "Request mismatch!");

            // Process
            await appInternal.AppJson.ProcessInternalAsync();

            RenderVersion(appInternal.AppJson); // Version tag

            UtilFramework.Assert(appInternal.AppJson.RequestCount == requestCountAssert); // Incoming and outgoing RequestCount has to be identical!

            // SerializeClient
            string jsonClient = UtilJson.Serialize(appInternal.AppJson, appInternal.TypeComponentInNamespaceList);

            // SerializeSession
            UtilSession.Serialize(appInternal);

            return jsonClient;
        }

        /// <summary>
        /// Create new AppJson component for this session.
        /// </summary>
        private AppJson CreateAppJson()
        {
            Type type = UtilFramework.TypeFromName(Website.AppTypeName);
            if (type == null)
            {
                throw new Exception(string.Format("AppTypeName does not exist! See also file: ConfigWebServer.json ({0})", Website.AppTypeName));
            }

            AppJson result = (AppJson)Activator.CreateInstance(type);
            result.Constructor(null);
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
        }

        /// <summary>
        /// Returns assembly and namespace to search for classes when deserializing json. (For example: "MyPage")
        /// </summary>
        virtual internal Type[] TypeComponentInNamespaceList()
        {
            var typeAppJson = UtilFramework.TypeFromName(Website.AppTypeName);
            return (new Type[] {
                typeAppJson, // Namespace of running application.
                typeof(AppJson), // For example button.
            }).Distinct().ToArray(); // Enable serialization of components in App and AppConfig namespace.
        }
    }
}
