namespace Framework.Cli.Config
{
    using Framework.Cli.Command;
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
        /// Init default file ConfigCli.json and ConfigWebServer.json
        /// </summary>
        internal static void Init(AppCli appCli)
        {
            if (!File.Exists(FileName))
            {
                ConfigCli configCli = new ConfigCli();
                configCli.EnvironmentGet().WebsiteList = new List<ConfigCliWebsite>();
                appCli.InitConfigCli(configCli);
                Save(configCli);

                CommandBuild.InitConfigWebServer(appCli);
            }
        }

        internal static ConfigCli Load()
        {
            var result = UtilFramework.ConfigLoad<ConfigCli>(FileName);
            if (result.EnvironmentGet().WebsiteList == null)
            {
                result.EnvironmentGet().WebsiteList = new List<ConfigCliWebsite>();
            }
            foreach (var website in result.EnvironmentGet().WebsiteList)
            {
                if (website.DomainNameList == null)
                {
                    website.DomainNameList = new List<string>();
                }
            }
            return result;
        }

        internal static void Save(ConfigCli configCli)
        {
            UtilFramework.ConfigSave(configCli, FileName);
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
        /// Gets or sets ConnectionStringFramework. Can be different from ConnectionStringApplication, if framework relevant tables are stored on another database.
        /// </summary>
        public string ConnectionStringFramework { get; set; }

        /// <summary>
        /// Gets or sets ConnectionStringApplication. Database containing business data.
        /// </summary>
        public string ConnectionStringApplication { get; set; }

        /// <summary>
        /// Gets or sets WebsiteList. Multiple domain names can be served by one ASP.NET Core instance.
        /// </summary>
        public List<ConfigCliWebsite> WebsiteList { get; set; }

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
        /// Gets or sets FolderNameServer. For example: "Application.Server/Framework/Application.Website/Default".
        /// </summary>
        public string FolderNameServer { get; set; }

        /// <summary>
        /// Gets or sets DomainNameList. For example "example.com".
        /// </summary>
        public List<string> DomainNameList { get; set; }

        public string DomainNameListToString()
        {
            string result = null;
            bool isFirst = true;
            if (DomainNameList != null)
            {
                foreach (string domainName in DomainNameList)
                {
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

        /// <summary>
        /// Gets or sets AppTypeName. Needs to derrive from AppJson. For example: "Application.AppJson, Application". If null, index.html is rendered without server side rendering.
        /// </summary>
        public string AppTypeName { get; set; }

        /// <summary>
        /// Gets or sets FolderNameNpmBuild. In this folder the following commands are executed: "npm install", "npm build". 
        /// </summary>
        public string FolderNameNpmBuild { get; set; }

        /// <summary>
        /// Gets or sets FolderNameDist. For example: "Application.Website/Default/dist". Content of this folder will be copied to FolderNameServer".
        /// </summary>
        public string FolderNameDist { get; set; }

        /// <summary>
        /// Gets or sets Git repo if "external" website is in an other git repo.
        /// </summary>
        public ConfigCliWebsiteGit Git { get; set; }
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
