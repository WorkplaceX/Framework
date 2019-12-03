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
    using System.Text.Json;
    using System.Collections.Generic;

    internal class AppInternal // TODO Json2 remove
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

    internal class AppSelector // TODO Json2 remove
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
                result.AppJson.IsSessionExpired = false;
            }
            else
            {
                result.AppJson = CreateAppJson();
            }
            int requestCountAssert = result.AppJson.RequestCount;

            UtilSession.Deserialize(result); // Deserialize session or init.

            // User hit reload button in browser.
            bool isBrowserRefresh = (result.AppJson.ResponseCount == 0 && result.AppSession.ResponseCount > 0);

            // User has app open in two browser tabs.
            bool isBrowserTabSwitch = (result.AppJson.ResponseCount != result.AppSession.ResponseCount);

            // Session expired
            bool isSessionExpired = (result.AppSession.ResponseCount == 0 && result.AppJson.ResponseCount > 0);

            // Init
            if (result.AppJson.IsInit == false || isBrowserRefresh || isBrowserTabSwitch || isSessionExpired)
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
                result.AppJson.RequestUrl = UtilServer.RequestUrl();
                result.AppJson.IsInit = true;
                result.AppJson.IsSessionExpired = isSessionExpired;
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
            if (UtilFramework.IsJson2 == false) // TODO Json2
            {
                return Type.GetType("Application.AppMain, Application");
            }
            string requestDomainName = UtilServer.RequestDomainName();
            var config = ConfigWebServer.Load();
            string appTypeName = null; 
            foreach (var website in config.WebsiteList)
            {
                foreach (string domainName in website.DomainNameList)
                {
                    if (domainName == requestDomainName)
                    {
                        appTypeName = website.AppTypeName;
                        break;
                    }
                }
                if (appTypeName != null)
                {
                    break;
                }
            }
            if (appTypeName == null)
            {
                appTypeName = config.WebsiteList.Where(item => item.DomainNameList.Count == 0).Select(item => item.AppTypeName).SingleOrDefault();
            }
            if (appTypeName == null)
            {
                throw new Exception("AppTypeName not defined! See also file: ConfigWebServer.json");
            }
            Type result = Type.GetType(appTypeName);
            if (result == null)
            {
                throw new Exception(string.Format("Type not found! See also file: ConfigWebServer.json ({0})", appTypeName));
            }
            return result;
        }

        private Type typeAppJson;

        /// <summary>
        /// Returns assembly and namespace to search for classes when deserializing json. (For example: "MyPage")
        /// </summary>
        virtual internal Type[] TypeComponentInNamespaceList()
        {
            if (typeAppJson == null)
            {
                typeAppJson = TypeAppJson();
            }
            return (new Type[] {
                typeAppJson, // Namespace of running application.
                typeof(AppJson), // For example button.
            }).Distinct().ToArray(); // Enable serialization of components in App and AppConfig namespace.
        }
    }

    /// <summary>
    /// Create AppJson or deserialize from server session. Process request. Serialize AppJson to server session and angular client.
    /// </summary>
    internal class AppSelector2
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public AppSelector2()
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
            AppJson2 appJson = null; // AppJson to process
            AppJson2Request appJsonRequest = new AppJson2Request(); // Default

            // Deserialize AppJson from server session
            appJson = UtilSession2.Deserialize();
            if (appJson == null)
            {
                appJson = CreateAppJson(); // Create new session
                await appJson.InitAsync();
            }

            // Deserialize AppJson2Request
            string jsonPostBody = await UtilServer.StreamToString(context.Request.Body); // Get request POST body
            if (jsonPostBody != null) // If post
            {
                appJsonRequest = JsonSerializer.Deserialize<AppJson2Request>(jsonPostBody);
            }

            appJson.AppJson2Request = appJsonRequest;
            appJson.RequestUrl = UtilServer.RequestUrl();
            appJson.RequestCount = (appJsonRequest?.RequestCount).GetValueOrDefault();
            appJson.IsJson2 = true;

            // Process request
            await appJson.ProcessInternalAsync(appJsonRequest);

            // Serialize AppJson to server session and angular client
            UtilSession2.Serialize(appJson, out string jsonClient);

            return jsonClient;
        }

        /// <summary>
        /// Create new AppJson component for this session.
        /// </summary>
        private AppJson2 CreateAppJson()
        {
            Type type = UtilJson2.TypeFromName(Website.AppTypeName);
            if (type == null)
            {
                throw new Exception(string.Format("AppTypeName does not exist! See also file: ConfigWebServer.json ({0})", Website.AppTypeName));
            }
            
            AppJson2 result = (AppJson2)Activator.CreateInstance(type);
            return result;
        }
    }
}
