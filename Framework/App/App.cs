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
    using System.Text.Json;

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
            string requestJsonText = await UtilServer.StreamToString(context.Request.Body);
            bool isEmbeddedUrl = (string)context.Request.Query["isEmbeddedUrl"] == ""; // Flag set by Angular client on first app.json POST if running embedded on other website.
            RequestJson requestJson = null;
            if (requestJsonText != null && !isEmbeddedUrl) // If client POST
            {
                requestJson = JsonSerializer.Deserialize<RequestJson>(requestJsonText);
                string jsonClient = UtilServer.Session.GetString("JsonClient");
                if (jsonClient == null)
                {
                    appInternal.AppSession = new AppSession();
                    appInternal.AppJson = CreateAppJson(); // Session expired.
                }
                else
                {
                    appInternal.AppJson = (AppJson)UtilJson.Deserialize(jsonClient);
                }
                appInternal.AppJson.IsSessionExpired = false;
                appInternal.AppJson.RequestCount = requestJson.RequestCount;
                appInternal.AppJson.RequestJson = requestJson;
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
                appInternal.AppJson = CreateAppJson(); // Reset
                appInternal.AppSession = new AppSession(); // Reset
                appInternal.AppJson.RequestCount = requestCount;
                appInternal.AppJson.ResponseCount = responseCount;
                appInternal.AppSession.ResponseCount = responseCount;
                appInternal.AppJson.BrowserUrl = browserUrl;
                appInternal.AppJson.RequestUrl = UtilServer.RequestUrl();
                appInternal.AppJson.IsInit = true;
                appInternal.AppJson.IsSessionExpired = isSessionExpired;
                await appInternal.AppJson.InitInternalAsync();
            }

            UtilFramework.Assert(appInternal.AppJson.ResponseCount == appInternal.AppSession.ResponseCount, "Request mismatch!");

            if (appInternal.AppJson.RequestJson == null)
            {
                appInternal.AppJson.RequestJson = new RequestJson(); // As long as client null request are comming
            }

            // Process
            await appInternal.AppJson.ProcessInternalAsync();

            RenderVersion(appInternal.AppJson); // Version tag

            UtilFramework.Assert(appInternal.AppJson.RequestCount == requestCountAssert); // Incoming and outgoing RequestCount has to be identical!

            // SerializeSession, SerializeClient
            UtilSession.Serialize(appInternal, out string jsonClientResponse);

            return jsonClientResponse;
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
            result.RequestJson = new RequestJson();
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
    }
}
