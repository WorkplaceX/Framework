﻿namespace Framework.App
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
    using System.IO;

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
                if (!File.Exists(ConfigServer.FileName))
                {
                    throw new Exception("File ConfigServer.json not found! Make sure cli (wpx) did run at least once.");
                }

                // Run command cli env name=dev
                throw new Exception(string.Format("Website not found! See also file: ConfigServer.json (Domain={0}; Environment={1};)", requestDomainName, this.ConfigServer.EnvironmentName));
            }
            if (result.Count > 1)
            {
                throw new Exception(string.Format("More than one website found! See also file: ConfigServer.json ({0})", requestDomainName));
            }

            this.ConfigWebsite = result.Single();
            this.ConfigDomain = ConfigWebsite.DomainNameList.Where(item => item.DomainName == requestDomainName).Single();
            this.AppTypeName = ConfigDomain.AppTypeName;
        }

        /// <summary>
        /// Gets ConfigServer. Currently loaded config.
        /// </summary>
        public readonly ConfigServer ConfigServer;

        /// <summary>
        /// Gets ConfigWebsite. This is the currently requested Website.
        /// </summary>
        public readonly ConfigServerWebsite ConfigWebsite;

        /// <summary>
        /// Gets ConfigDomain. This is the currently requested domain.
        /// </summary>
        public readonly ConfigServerWebsiteDomain ConfigDomain;

        /// <summary>
        /// Gets AppTypeName. This is the currently requested App.
        /// </summary>
        public readonly string AppTypeName;

        /// <summary>
        /// Gets LogJsonSessionLength. This is the length of last serialized session data by method ProcessAsync();
        /// </summary>
        public int LogJsonSessionLength { get; private set; }

        /// <summary>
        /// Gets LogCommandEnum. This is the POST request body payload first CommandEnum.
        /// </summary>
        public string LogCommandEnum { get; private set; }

        /// <summary>
        /// Gets LogNavigatePathAddHistory. This is the new virtual NavigatePath.
        /// </summary>
        public string LogNavigatePathAddHistory { get; private set; }

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
                await appJson.ProcessInternalAsync(appJson, this);
            }
            catch (Exception exception)
            {
                new Alert(appJson, UtilFramework.ExceptionToString(exception), AlertEnum.Error);
                appJson.IsReload = true;
                context.Response.Cookies.Delete(StartupFramework.CookieName); // Delete session cookie to request new session.
            }

            // Version tag
            RenderVersion(appJson);

            // RequestCount
            appJson.RequestCount = appJson.RequestJson.RequestCount;

            // ResponseCount
            appJson.ResponseCount += 1;

            // SerializeSession, SerializeClient
            UtilSession.Serialize(appJson, out string jsonClientResponse, out int jsonSessionLength);
            
            // Log
            LogJsonSessionLength = jsonSessionLength;
            LogNavigatePathAddHistory = appJson.NavigatePathAddHistory;

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

            // Log
            LogCommandEnum = string.Format("{0}", requestJson.CommandList.FirstOrDefault()?.CommandEnum);
            
            AppJson appJson = null;
            if (UtilServer.RequestMethodIsGet() == false) // No session deserialize for GET. See also method CreateAppJson(); on how to preserve for example selected language.
            {
                // Deserialize AppJson (Session)
                appJson = UtilSession.Deserialize();
            }

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
                if (appJson == null || UtilServer.RequestMethodIsGet())
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
                if (UtilServer.RequestMethodIsPost() || browserNavigatePath == "/")
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

            // Preserve for example login user and selected language.
            if (UtilServer.RequestMethodIsGet())
            {
                var appJsonPrevious = UtilSession.Deserialize();
                if (appJsonPrevious != null)
                {
                    result.AppJsonPrevious = appJsonPrevious;
                }
            }

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
