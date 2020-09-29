namespace Framework.Cli.Config
{
    using Framework.Cli.Command;
    using Framework.Config;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    /// <summary>
    /// Configuration used by all cli commands.
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
            var result = EnvironmentList.SingleOrDefault(item => item.EnvironmentName?.ToUpper() == EnvironmentNameGet());
            if (result == null)
            {
                result = new ConfigCliEnvironment { EnvironmentName = EnvironmentNameGet() };
                EnvironmentList.Add(result);
                EnvironmentName = EnvironmentNameGet();
            }
            return result;
        }

        /// <summary>
        /// Gets or sets WebsiteList. Multiple domain names can be served by one ASP.NET Core instance.
        /// </summary>
        public List<ConfigCliWebsite> WebsiteList { get; set; }

        /// <summary>
        /// Gets or sets ExternalGit. For example: https://username:password@dev.azure.com/company/repo/_git/repo.git
        /// Clones and calls prebuild script in folder External.Prebuild\ in cli build command.
        /// </summary>
        public string ExternalGit { get; set; }

        /// <summary>
        /// Gets or sets ExternalProjectName. For example MyApp. Used to build ExternalFolderName ExternalGit/MyApp/ Cli build command calls this .NET script.
        /// </summary>
        public string ExternalProjectName { get; set; }

        /// <summary>
        /// Returns ConnectionString of application or framework.
        /// </summary>
        public static string ConnectionString(bool isFrameworkDb)
        {
            var configCli = ConfigCli.Load();
            if (isFrameworkDb == false)
            {
                return configCli.EnvironmentGet().ConnectionStringApplication;
            }
            else
            {
                return configCli.EnvironmentGet().ConnectionStringFramework;
            }
        }

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
            foreach (var website in result.WebsiteList)
            {
                if (website.DomainNameList == null)
                {
                    website.DomainNameList = new List<ConfigCliWebsiteDomain>();
                }
            }
            return result;
        }

        internal static void Save(ConfigCli configCli)
        {
            UtilFramework.ConfigSave(configCli, FileName);
        }

        /// <summary>
        /// Copy from file ConfigCli.json to ConfigServer.json
        /// </summary>
        public static void ConfigToServer()
        {
            // Console.WriteLine("Copy runtime specific values from ConfigCli to ConfigServer"); // There is also other values not needed for runtime like DeployAzureGitUrl.
            var configCli = ConfigCli.Load();
            var configServer = ConfigServer.Load();

            // Environment
            configServer.EnvironmentName = configCli.EnvironmentGet().EnvironmentName;
            configServer.IsUseDeveloperExceptionPage = configCli.EnvironmentGet().IsUseDeveloperExceptionPage;

            // ConnectionString
            configServer.ConnectionStringFramework = configCli.EnvironmentGet().ConnectionStringFramework;
            configServer.ConnectionStringApplication = configCli.EnvironmentGet().ConnectionStringApplication;

            // Website
            configServer.WebsiteList.Clear();
            foreach (var webSite in configCli.WebsiteList)
            {
                configServer.WebsiteList.Add(new ConfigServerWebsite()
                {
                    FolderNameDist = webSite.FolderNameDist,
                    DomainNameList = webSite.DomainNameList.Where(item => item.EnvironmentName == configCli.EnvironmentGet().EnvironmentName).Select(item => new ConfigServerWebsiteDomain { DomainName = item.DomainName, AppTypeName = item.AppTypeName }).ToList()
                });
            }

            ConfigServer.Save(configServer);
        }
    }

    /// <summary>
    /// Allows definition of DEV, TEST, PROD environments in ConfigCli.json file.
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
        /// Gets or sets ConnectionStringFramework. Can be different from ConnectionStringApplication, if framework relevant tables are stored on another database.
        /// </summary>
        public string ConnectionStringFramework { get; set; }

        /// <summary>
        /// Gets or sets ConnectionStringApplication. Database containing business data.
        /// </summary>
        public string ConnectionStringApplication { get; set; }

        /// <summary>
        /// Gets or sets DeployAzureGitUrl. Used by CommandDeploy.
        /// </summary>
        public string DeployAzureGitUrl { get; set; }
    }

    /// <summary>
    /// Include "external" website.
    /// </summary>
    public class ConfigCliWebsite
    {
        /// <summary>
        /// Gets or sets FolderNameNpmBuild. In this folder the following commands are executed: "npm install", "npm build". 
        /// </summary>
        public string FolderNameNpmBuild { get; set; }

        /// <summary>
        /// Gets or sets FolderNameDist. For example: "Application.Website/LayoutDefault/dist". Content of this folder will be copied to "Application.Server/Framework/Application.Website/Master01".
        /// </summary>
        public string FolderNameDist { get; set; }

        /// <summary>
        /// Gets DomainNameList. Domains mapped to this layout website.
        /// </summary>
        public List<ConfigCliWebsiteDomain> DomainNameList { get; set; }

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
    }
}
