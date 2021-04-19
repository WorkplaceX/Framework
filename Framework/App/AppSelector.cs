namespace Framework.App
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
            List<ConfigServerWebsite> result = new List<ConfigServerWebsite>();
            string requestDomainName = UtilServer.RequestDomainName();
            this.ConfigServer = ConfigServer.Load();
            foreach (var website in ConfigServer.WebsiteList)
            {
                foreach (var item in website.DomainNameList)
                {
                    if (item.DomainName == requestDomainName)
                    {
                        result.Add(website);
                    }
                }
            }

            // Make sure Website has been found
            if (result.Count == 0)
            {
                // Run command cli env name=dev
                throw new Exception(string.Format("Website not found! See also: ConfigServer.json (Domain={0}; Environment={1};)", requestDomainName, this.ConfigServer.EnvironmentName));
            }
            if (result.Count > 1)
            {
                throw new Exception(string.Format("More than one website found! See also: ConfigServer.json ({0})", requestDomainName));
            }

            this.Website = result.Single();
            this.AppTypeName = Website.DomainNameList.Where(item => item.DomainName == requestDomainName).Single().AppTypeName;
        }

        /// <summary>
        /// Gets ConfigServer. Currently loaded config.
        /// </summary>
        public readonly ConfigServer ConfigServer;

        /// <summary>
        /// Gets Website. This is the currently requested Website.
        /// </summary>
        public readonly ConfigServerWebsite Website;

        /// <summary>
        /// Gets AppTypeName. This is the currently requested App.
        /// </summary>
        public readonly string AppTypeName;

        /// <summary>
        /// Returns JsonClient. Create AppJson and process request.
        /// </summary>
        internal async Task<string> ProcessAsync(HttpContext context, AppJson appJson)
        {
            if (appJson == null)
            {
                // Create AppJson with session data.
                appJson = await CreateAppJsonSession(context);
            }

            // Process
            try
            {
                await appJson.ProcessInternalAsync(appJson);
            }
            catch (Exception exception)
            {
                new Alert(appJson, UtilFramework.ExceptionToString(exception), AlertEnum.Error);
                appJson.IsReload = true;
            }

            // Version tag
            RenderVersion(appJson);

            // RequestCount
            appJson.RequestCount = appJson.RequestJson.RequestCount;

            // ResponseCount
            appJson.ResponseCount += 1;

            // SerializeSession, SerializeClient
            UtilSession.Serialize(appJson, out string jsonClientResponse);

            return jsonClientResponse;
        }

        /// <summary>
        /// Create AppJson with session data.
        /// </summary>
        internal async Task<AppJson> CreateAppJsonSession(HttpContext context)
        {
            // Deserialize RequestJson
            RequestJson requestJson;
            string requestJsonText = await UtilServer.StreamToString(context.Request.Body);
            if (requestJsonText != null)
            {
                requestJson = JsonSerializer.Deserialize<RequestJson>(requestJsonText);
                requestJson.Origin = RequestOrigin.Browser;
                foreach (var command in requestJson.CommandList)
                {
                    command.GridCellText = UtilFramework.StringNull(command.GridCellText); // Sanitize incomming request.
                }
            }
            else
            {
                requestJson = new RequestJson(null) { RequestCount = 1 };
            }

            // Deserialize AppJson (Session) or init
            var appJson = UtilSession.Deserialize();

            // IsExpired
            bool isSessionExpired = appJson == null && requestJson.RequestCount > 1;
            bool isBrowserRefresh = appJson != null && requestJson.RequestCount == 1 && requestJson.RequestCount != appJson.RequestCount + 1;
            bool isBrowserTabSwitch = !isBrowserRefresh && (appJson != null && requestJson.ResponseCount != appJson.ResponseCount);
            bool isException = appJson?.IsReload == true; // After exception has been thrown recycle session.

            // New session
            if (appJson == null || isBrowserTabSwitch || isException)
            {
                // New AppJson (Session)
                bool isInit = false;
                if (appJson == null || UtilServer.Context.Request.Method == "GET")
                {
                    appJson = CreateAppJson();
                    isInit = true;
                }
                appJson.RequestUrlHost = UtilServer.RequestUrlHost();
                appJson.IsSessionExpired = isSessionExpired;

                // New RequestJson
                string browserNavigatePath = requestJson.BrowserNavigatePath;
                requestJson = new RequestJson(null) { RequestCount = requestJson.RequestCount, BrowserNavigatePathPost = requestJson.BrowserNavigatePathPost }; // Reset RequestJson.
                appJson.RequestJson = requestJson;

                // Add navigate command to queue
                if (UtilServer.Context.Request.Method == "POST" || browserNavigatePath == "/")
                {
                    appJson.Navigate(browserNavigatePath, isAddHistory: false); // User clicked backward or forward button in browser.
                }

                // New session init
                if (isInit)
                {
                    await appJson.InitInternalAsync();
                }
            }
            else
            {
                appJson.IsSessionExpired = false;
            }

            // Set RequestJson
            appJson.RequestJson = requestJson;

            return appJson;
        }

        /// <summary>
        /// Create AppJson without session data.
        /// </summary>
        public AppJson CreateAppJson()
        {
            Type type;
            try
            {
                type = UtilFramework.TypeFromName(AppTypeName);
            }
            catch
            {
                throw new Exception(string.Format("AppTypeName does not exist! See also file: ConfigServer.json ({0})", AppTypeName));
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
        }
    }
}
