namespace Framework
{
    public class ConfigFramework
    {
        public bool IsServerSideRendering { get; set; }

        /// <summary>
        /// Gets or sets IsIndexHtml. If true, custom "Application.Server/Framework/index.html" file is used.
        /// </summary>
        public bool IsCustomIndexHtml { get; set; }

        private static string FileName
        {
            get
            {
                return UtilFramework.FolderName + "ConfigFramework.json";
            }
        }

        private static string FileNameDefault
        {
            get
            {
                return UtilFramework.FolderName + "ConfigFrameworkDefault.json";
            }
        }

        internal static ConfigFramework Load()
        {
            return UtilFramework.ConfigLoad<ConfigFramework>(FileName, FileNameDefault);
        }

        internal static void Save(ConfigFramework configFramework)
        {
            UtilFramework.ConfigSave(configFramework, FileName);
        }
    }
}
