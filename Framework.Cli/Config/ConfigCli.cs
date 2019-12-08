namespace Framework.Cli.Config
{
    using Framework.Cli.Command;
    using System.Collections.Generic;
    using System.IO;

    /// <summary>
    /// Configuration used by all cli commands.
    /// </summary>
    public class ConfigCli
    {
        /// <summary>
        /// Gets or sets ConnectionStringFramework. Can be different from ConnectionStringApplication, if framework relevant tables are stored on another database.
        /// </summary>
        public string ConnectionStringFramework { get; set; }

        /// <summary>
        /// Gets or sets ConnectionStringApplication. Database containing business data.
        /// </summary>
        public string ConnectionStringApplication { get; set; }

        /// <summary>
        /// Returns ConnectionString of application or framework.
        /// </summary>
        public static string ConnectionString(bool isFrameworkDb)
        {
            var configCli = ConfigCli.Load();
            if (isFrameworkDb == false)
            {
                return configCli.ConnectionStringApplication;
            }
            else
            {
                return configCli.ConnectionStringFramework;
            }
        }

        /// <summary>
        /// Gets or sets WebsiteList. Multiple domain names can be served by one ASP.NET Core instance.
        /// </summary>
        public List<ConfigCliWebsite> WebsiteList { get; set; }

        /// <summary>
        /// Gets or sets DeployAzureGitUrl. Used by CommandDeploy.
        /// </summary>
        public string DeployAzureGitUrl { get; set; }

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
                configCli.WebsiteList = new List<ConfigCliWebsite>();
                appCli.InitConfigCli(configCli);
                Save(configCli);

                CommandBuild.InitConfigWebServer(appCli);
            }
        }

        internal static ConfigCli Load()
        {
            var result = UtilFramework.ConfigLoad<ConfigCli>(FileName);
            if (result.WebsiteList == null)
            {
                result.WebsiteList = new List<ConfigCliWebsite>();
            }
            foreach (var website in result.WebsiteList)
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
    /// Include "external" website.
    /// </summary>
    public class ConfigCliWebsite
    {
        /// <summary>
        /// Gets or sets FolderNameServer. For example: "Application.Server/Framework/Website/Default".
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
        public string AppTypeName;

        /// <summary>
        /// Gets or sets FolderNameNpmBuild. In this folder the following commands are executed: "npm install", "npm build". 
        /// </summary>
        public string FolderNameNpmBuild { get; set; }

        /// <summary>
        /// Gets or sets FolderNameDist. For example: "Website/Default/dist". Content of this folder will be copied to FolderNameServer".
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
