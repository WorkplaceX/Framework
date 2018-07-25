namespace Framework.Cli
{
    public class ConfigCli
    {
        public string AzureGitUrl { get; set; }

        private static string FileName
        {
            get
            {
                return UtilFramework.FolderName + "ConfigCli.json";
            }
        }

        internal static ConfigCli Load()
        {
            return UtilFramework.ConfigLoad<ConfigCli>(FileName);
        }

        internal static void Save(ConfigCli configCli)
        {
            UtilFramework.ConfigSave(configCli, FileName);
        }
    }
}
