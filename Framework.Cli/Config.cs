namespace Framework.Cli
{
    using System.Collections.Generic;

    public class ConfigCli
    {
        public string AzureGitUrl { get; set; }

        public List<ConfigCliWebsite> WebsiteList { get; set; }

        private static string FileName
        {
            get
            {
                return UtilFramework.FolderName + "ConfigCli.json";
            }
        }

        private static string FileNameDefault
        {
            get
            {
                return UtilFramework.FolderName + "ConfigCliDefault.json";
            }
        }

        internal static ConfigCli Load()
        {
            return UtilFramework.ConfigLoad<ConfigCli>(FileName, FileNameDefault);
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
        /// Gets or sets DomainName. For example (example.com) or empty for default.
        /// </summary>
        public string DomainName { get; set; }

        /// <summary>
        /// Gets or sets DomainNameRedirect. Redirect domain. Empty if no redirect.
        /// </summary>
        public string DomainNameRedirect { get; set; }

        /// <summary>
        /// Gets or sets FolderNameNpmBuild. In this folder the following commands will be executed: "npm install", "npm build". Empty if GitUrl is used.
        /// </summary>
        public string FolderNameNpmBuild { get; set; }

        /// <summary>
        /// Gets or sets FolderNameDist. Content of this folder will be copied to Application.Server/Framework/WebsiteInclude/{DomainName}/. Empty if GitUrl is used.
        /// </summary>
        public string FolderNameDist { get; set; }

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

        /// <summary>
        /// Gets or sets GitFolderNameNpmBuild. In this folder the following commands will be executed: "npm install", "npm build".
        /// </summary>
        public string GitFolderNameNpmBuild { get; set; }

        /// <summary>
        /// Gets or sets GitFolderNameDist. Content of this folder will be copied to Application.Server/Framework/WebsiteInclude/{DomainName}/
        /// </summary>
        public string GitFolderNameDist { get; set; }
    }
}
