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
                configCli.WebsiteList = new List<ConfigCliWebsite>();
                appCli.InitConfigCli(configCli);
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
        /// Gets or sets IsUseDeveloperExceptionPage. If true, show detailed exceptions.
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
        /// Returns FolderNameServer. For example: "Application.Server/Framework/Application.Website/Website01/".
        /// </summary>
        public string FolderNameServerGet(ConfigCli configCli)
        {
            return string.Format("Application.Server/Framework/Application.Website/Master{0:00}/", configCli.WebsiteList.IndexOf(this) + 1);
        }

        /// <summary>
        /// Gets or sets FolderNameNpmBuild. In this folder the following commands are executed: "npm install", "npm build". 
        /// </summary>
        public string FolderNameNpmBuild { get; set; }

        /// <summary>
        /// Gets or sets FolderNameDist. For example: "Application.Website/MasterDefault/dist". Content of this folder will be copied to "Application.Server/Framework/Application.Website/Master01".
        /// </summary>
        public string FolderNameDist { get; set; }

        /// <summary>
        /// Gets or sets Git repo if "external" website is in an other git repo.
        /// </summary>
        public ConfigCliWebsiteGit Git { get; set; }

        /// <summary>
        /// Gets DomainNameList. Domains mapped to this master website.
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

    public class ConfigCliWebsiteGit
    {
        /// <summary>
        /// Gets or sets GitUrl. Applicable if external website to include is in another git repo.
        /// </summary>
        public string GitUrl { get; set; }

        /// <summary>
        /// Gets or sets GitUser. For example if git repo is not a public repo.
        /// </summary>
        public string GitUser { get; set; }

        /// <summary>
        /// Gets or sets GitPassword. For example if git repo is not a public repo.
        /// </summary>
        public string GitPassword { get; set; }
    }
}
