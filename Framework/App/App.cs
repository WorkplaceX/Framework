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
            // DeserializeSession
            var appInternal = UtilSession.Deserialize(); // Deserialize session or init.
            UtilServer.AppInternal = appInternal;

            // RequestJson
            RequestJson requestJson;
            string requestJsonText = await UtilServer.StreamToString(context.Request.Body);
            if (requestJsonText != null)
            {
                requestJson = JsonSerializer.Deserialize<RequestJson>(requestJsonText);
            }
            else
            {
                requestJson = new RequestJson { Command = RequestCommand.None, RequestCount = 1 };
            }

            bool isSessionExpired = appInternal.AppJson == null && requestJson.RequestCount > 1;
            bool isBrowserRefresh = requestJson.RequestCount != appInternal.AppSession.RequestCount + 1; // Or BrowserTabSwitch.
            bool isBrowserTabSwitch = requestJson.ResponseCount != appInternal.AppSession.ResponseCount;

            // New Session
            if (appInternal.AppJson == null || isBrowserRefresh || isBrowserTabSwitch)
            {
                appInternal.AppJson = CreateAppJson();
                requestJson = new RequestJson { Command = RequestCommand.None, RequestCount = requestJson.RequestCount };
                appInternal.AppJson.RequestJson = requestJson;
                appInternal.AppJson.RequestUrl = UtilServer.RequestUrl();
                if (isSessionExpired)
                {
                    appInternal.AppJson.IsSessionExpired = true;
                }
                await appInternal.AppJson.InitInternalAsync();
            }
            else
            {
                appInternal.AppJson.IsSessionExpired = false;
            }

            // Set RequestJson
            appInternal.AppJson.RequestJson = requestJson;

            // Process
            await appInternal.AppJson.ProcessInternalAsync();

            // Version tag
            RenderVersion(appInternal.AppJson);

            // RequestCount
            appInternal.AppJson.RequestCount = requestJson.RequestCount;
            appInternal.AppSession.RequestCount = requestJson.RequestCount;

            // ResponseCount
            appInternal.AppSession.ResponseCount += 1;
            appInternal.AppJson.ResponseCount = appInternal.AppSession.ResponseCount;

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
