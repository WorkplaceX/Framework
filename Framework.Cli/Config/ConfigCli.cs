namespace Framework.Cli.Config
{
    using Framework.Config;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.Json.Serialization;

    /// <summary>
    /// Configuration used by all command cli. Values are stored in file ConfigCli.json. See also method ConfigCli.CopyConfigCliToConfigServer();
    /// For default configuration see also method AppCli.InitConfigCli();
    /// </summary>
    public class ConfigCli
    {
        /// <summary>
        /// Gets or sets EnvironmentName. This is the currently selected environment.
        /// </summary>
        public string EnvironmentName { get; set; }

        /// <summary>
        /// Returns EnvironmentName. This is the currently selected environment. Default is DEV.
        /// </summary>
        public string EnvironmentNameGet()
        {
            string result = EnvironmentName?.ToUpper();
            if (UtilFramework.StringNull(result) == null)
            {
                result = "DEV";
            }
            return result;
        }

        public List<ConfigCliEnvironment> EnvironmentList { get; set; }

        /// <summary>
        /// Returns currently selected environment. Default is DEV. Adds new environment if none exists.
        /// </summary>
        public ConfigCliEnvironment EnvironmentGet()
        {
            if (EnvironmentList == null)
            {
                EnvironmentList = new List<ConfigCliEnvironment>();
            }
            var environmentName = EnvironmentNameGet();

            var result = EnvironmentGet(environmentName);
            if (result == null)
            {
                result = new ConfigCliEnvironment { EnvironmentName = environmentName, IsUseDeveloperExceptionPage = environmentName == "DEV" };
                EnvironmentList.Add(result);
                EnvironmentName = environmentName;
            }
            return result;
        }

        /// <summary>
        /// Returns specific environment. Returns null if not found in config.
        /// </summary>
        internal ConfigCliEnvironment EnvironmentGet(string environmentName)
        {
            return EnvironmentList.SingleOrDefault(item => item.EnvironmentName?.ToUpper() == environmentName);
        }

        /// <summary>
        /// Gets or sets WebsiteList. Multiple domain names can be served by one ASP.NET Core instance.
        /// </summary>
        public List<ConfigCliWebsite> WebsiteList { get; set; }

        /// <summary>
        /// Gets or sets ExternalGitList.
        /// </summary>
        public List<ConfigCliExternal> ExternalGitList { get; set; }

        /// <summary>
        /// Gets ConfigCli.json. Used by CommandBuild. Created with default values if file does not exist.
        /// </summary>
        private static string FileName
        {
            get
            {
                return UtilFramework.FolderName + "ConfigCli.json";
            }
        }

        /// <summary>
        /// Create default ConfigCli.json file.
        /// </summary>
        public static void Init(AppCli appCli)
        {
            if (!File.Exists(FileName))
            {
                ConfigCli configCli = new ConfigCli();
                configCli.EnvironmentName = configCli.EnvironmentNameGet();
                configCli.EnvironmentList = new List<ConfigCliEnvironment>();
                configCli.WebsiteList = new List<ConfigCliWebsite>();
                configCli.ExternalGitList = new List<ConfigCliExternal>();
                appCli.InitConfigCli(configCli);

                // EnvironmentName defined in WebsiteList
                List<string> environmentNameList = new List<string>();
                foreach (var website in configCli.WebsiteList)
                {
                    foreach (var domainName in website.DomainNameList)
                    {
                        environmentNameList.Add(domainName.EnvironmentName);
                    }
                }
                environmentNameList = environmentNameList.Distinct().ToList();

                // Add missing environments
                foreach (var environmentName in environmentNameList)
                {
                    if (configCli.EnvironmentList.Where(item => item.EnvironmentName == environmentName).FirstOrDefault() == null)
                    {
                        configCli.EnvironmentList.Add(new ConfigCliEnvironment { EnvironmentName = environmentName, IsUseDeveloperExceptionPage = environmentName == "DEV" });
                    }
                }
                UtilFramework.ConfigSave(configCli, FileName);
            }
        }

        internal static ConfigCli Load()
        {
            var result = UtilFramework.ConfigLoad<ConfigCli>(FileName);

            result.EnvironmentName = result.EnvironmentNameGet(); // Init DEV if necessary
            if (result.WebsiteList == null)
            {
                result.WebsiteList = new List<ConfigCliWebsite>();
            }
            if (result.ExternalGitList == null)
            {
                result.ExternalGitList = new List<ConfigCliExternal>();
            }
            
            var folderNameAngularList = new Dictionary<string, int>();
            int folderNameAngularIndex = 0;
            foreach (var website in result.WebsiteList)
            {
                // Init DomainNameList
                if (website.DomainNameList == null)
                {
                    website.DomainNameList = new List<ConfigCliWebsiteDomain>();
                }

                // Init FolderNameAngularIndex
                var folderNameAngular = UtilFramework.FolderNameParse(website.FolderNameAngular);
                if (folderNameAngular != null)
                {
                    if (folderNameAngularList.ContainsKey(folderNameAngular) == false)
                    {
                        website.FolderNameAngularIndex = folderNameAngularIndex;
                        folderNameAngularIndex += 1;
                    }
                    else
                    {
                        website.FolderNameAngularIsDuplicate = true;
                        website.FolderNameAngularIndex = folderNameAngularList[folderNameAngular];
                    }
                }
            }
            return result;
        }

        internal static void Save(ConfigCli configCli)
        {
            UtilFramework.ConfigSave(configCli, FileName);
        }

        /// <summary>
        /// Copy from file ConfigCli.json to ConfigServer.json. Selects values depending on current EnvironmentName.
        /// </summary>
        internal static void CopyConfigCliToConfigServer()
        {
            // Console.WriteLine("Copy runtime specific values from ConfigCli to ConfigServer"); // There is also other values not needed for runtime like DeployAzureGitUrl.
            var configCli = ConfigCli.Load();
            var configServer = ConfigServer.Load();

            // Environment
            configServer.EnvironmentName = configCli.EnvironmentGet().EnvironmentName;
            configServer.IsUseDeveloperExceptionPage = configCli.EnvironmentGet().IsUseDeveloperExceptionPage;
            configServer.IsRedirectHttps = configCli.EnvironmentGet().IsRedirectHttps;
            configServer.IsRedirectWww = configCli.EnvironmentGet().IsRedirectWww;

            // ConnectionString
            configServer.ConnectionStringFramework = configCli.EnvironmentGet().ConnectionStringFramework;
            configServer.ConnectionStringApplication = configCli.EnvironmentGet().ConnectionStringApplication;

            // Website
            configServer.WebsiteList.Clear();
            foreach (var webSite in configCli.WebsiteList)
            {
                configServer.WebsiteList.Add(new ConfigServerWebsite()
                {
                    FolderNameAngular = webSite.FolderNameAngular,
                    DomainNameList = webSite.DomainNameList.Where(item => item.EnvironmentName == configCli.EnvironmentGet().EnvironmentName).Select(
                        // Copy config website values from Cli to Server
                        item => new ConfigServerWebsiteDomain { 
                            DomainName = item.DomainName, 
                            AppTypeName = item.AppTypeName, 
                            IsRedirectHttps = item.IsRedirectHttps, 
                            BingMapKey = item.BingMapKey, 
                            GoogleAnalyticsId = item.GoogleAnalyticsId,
                            GoogleAdSenseId = item.GoogleAdSenseId,
                            Custom = item.Custom,
                        }).ToList()
                });
            }

            ConfigServer.Save(configServer);
        }
    }

    /// <summary>
    /// Definition of DEV, TEST, PROD environments in ConfigCli.json file.
    /// </summary>
    public class ConfigCliEnvironment
    {
        /// <summary>
        /// Gets or sets Name. For example DEV, TEST or PROD.
        /// </summary>
        public string EnvironmentName { get; set; }

        /// <summary>
        /// Gets or sets IsUseDeveloperExceptionPage. If true, show detailed exceptions. Restart web server after value change!
        /// </summary>
        public bool IsUseDeveloperExceptionPage { get; set; }

        /// <summary>
        /// Gets or sets IsRedirectHttps. If true, http is redirected to https. By default this value is false. Restart web server after value change!
        /// If true, server middelware redirects to https for all websites.
        /// </summary>
        public bool IsRedirectHttps { get; set; }

        /// <summary>
        /// Gets or sets IsRedirectWww. If ture, non www is redirected to wwww. For example workplacex.org is redirected to www.workplacex.org.
        /// </summary>
        public bool IsRedirectWww { get; set; }

        /// <summary>
        /// Gets or sets ConnectionStringFramework. Can be different from ConnectionStringApplication, if framework relevant tables are stored on another database.
        /// </summary>
        public string ConnectionStringFramework { get; set; }

        /// <summary>
        /// Gets or sets ConnectionStringApplication. Database containing business data.
        /// </summary>
        public string ConnectionStringApplication { get; set; }

        /// <summary>
        /// Gets or sets ConnectionString for Framework and Application.
        /// </summary>
        [JsonIgnore]
        public string ConnectionString
        {
            get
            {
                return ConnectionStringApplication;
            }
            set
            {
                ConnectionStringFramework = value;
                ConnectionStringApplication = value;
            }
        }

        /// <summary>
        /// Gets or sets DeployAzureGitUrl. Used by CommandDeploy.
        /// </summary>
        public string DeployAzureGitUrl { get; set; }
    }

    public class ConfigCliExternal
    {
        /// <summary>
        /// Gets or sets ExternalGit. For example: https://username:password@dev.azure.com/company/repo/_git/repo.git
        /// Clones and calls prebuild method AppCli.CommandExternal(); in external cli build command.
        /// </summary>
        public string ExternalGit { get; set; }

        /// <summary>
        /// Gets or sets ExternalProjectName. For example MyApp. Used to build ExternalFolderName ExternalGit/MyApp/ Cli build command calls this .NET script.
        /// </summary>
        public string ExternalProjectName { get; set; }
    }

    /// <summary>
    /// Include layout website.
    /// </summary>
    public class ConfigCliWebsite
    {
        /// <summary>
        /// Gets or sets FolderNameAngular. This is the Angular folder. The following commands are executed: "npm install", "npm run build:ssr". 
        /// </summary>
        public string FolderNameAngular { get; set; }

        /// <summary>
        /// Gets or sets FolderNameAngularIndex. Two websites with same FolderNameAngular get same index.
        /// </summary>
        internal int? FolderNameAngularIndex { get; set; }

        /// <summary>
        /// Gets FolderNameAngularWebsite. For example Website01.
        /// </summary>
        internal string FolderNameAngularWebsite => string.Format("Website{0:00}/", FolderNameAngularIndex.GetValueOrDefault() + 1);

        /// <summary>
        /// Gets or sets FolderNameAngularIsDuplicate. True, if another website has the same FolderNameAngular.
        /// </summary>
        internal bool FolderNameAngularIsDuplicate { get; set; }

        /// <summary>
        /// Gets DomainNameList. Domains mapped to this layout website.
        /// </summary>
        public List<ConfigCliWebsiteDomain> DomainNameList { get; set; }

        /// <summary>
        /// Returns property DomainNameList as text.
        /// </summary>
        public string DomainNameListToString()
        {
            string result = null;
            bool isFirst = true;
            if (DomainNameList != null)
            {
                foreach (var item in DomainNameList)
                {
                    string domainName = item.DomainName;
                    if (isFirst)
                    {
                        isFirst = false;
                    }
                    else
                    {
                        result += "; ";
                    }
                    result += domainName;
                }
            }
            return result;
        }
    }

    /// <summary>
    /// Mapping DomainName to AppTypeName.
    /// </summary>
    public class ConfigCliWebsiteDomain
    {
        /// <summary>
        /// Gets or sets EnvironmentName. This is the currently selected environment.
        /// </summary>
        public string EnvironmentName { get; set; }

        /// <summary>
        /// Gets or sets DomainName. For example "localhost".
        /// </summary>
        public string DomainName { get; set; }

        /// <summary>
        /// Gets or sets AppTypeName. Needs to derrive from AppJson. For example: "Application.AppJson, Application". If null, index.html is rendered without server side rendering.
        /// </summary>
        public string AppTypeName { get; set; }

        /// <summary>
        /// Gets or sets IsRedirectHttps. If true, it redirects on website level. See also property ConfigCliEnvironment.IsRedirectHttps.
        /// </summary>
        public bool IsRedirectHttps { get; set; }

        /// <summary>
        /// Gets or sets BingMapKey. See also class BingMap.
        /// </summary>
        public string BingMapKey { get; set; }

        /// <summary>
        /// Gets or sets GoogleAnalyticsId. This id is for Google Analytics 4. For example "G-XXXXXXXXXX".
        /// </summary>
        public string GoogleAnalyticsId { get; set; }


        /// <summary>
        /// Gets or sets GoogleAdSenseId. For example "ca-pub-XXXXXXXXXXXXXXXX".
        /// </summary>
        public string GoogleAdSenseId { get; set; }

        /// <summary>
        /// Gets or sets Custom.
        /// </summary>
        public object Custom { get; set; }
    }
}
