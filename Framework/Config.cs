using Newtonsoft.Json;
using System.ComponentModel;

namespace Framework
{
    public class ConfigFramework
    {
        [DefaultValue(true)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool IsServerSideRendering { get; set; }

        /// <summary>
        /// Gets or sets IsIndexHtml. If true, custom "Application.Server/Framework/index.html" file is used.
        /// </summary>
        [DefaultValue(true)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool IsCustomIndexHtml { get; set; }

        private static string FileName
        {
            get
            {
                return UtilFramework.FolderName + "ConfigFramework.json";
            }
        }

        internal static ConfigFramework Load()
        {
            return UtilFramework.ConfigLoad<ConfigFramework>(FileName);
        }

        internal static void Save(ConfigFramework configFramework)
        {
            UtilFramework.ConfigSave(configFramework, FileName);
        }
    }
}
