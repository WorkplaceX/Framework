namespace Framework
{
    using Framework.Server;
    using Microsoft.AspNetCore.Hosting;
    using System;
    using System.Collections.Generic;
    using System.IO;

    public class ConfigFramework
    {
        public bool IsServerSideRendering { get; set; }

        /// <summary>
        /// Gets or sets IsIndexHtml. If true, custom "Application.Server/Framework/index.html" file is used.
        /// </summary>
        public bool IsCustomIndexHtml { get; set; }

        public List<ConfigFrameworkWebsite> WebsiteList { get; set; }

        private static string FileName(IHostingEnvironment env)
        {
            if (UtilServer.IsIssServer == false)
            {
                return UtilFramework.FolderName + "ConfigFramework.json";
            }
            else
            {
                if (env == null)
                {
                    throw new Exception("Env is null!");
                }
                return UtilServer.FolderNameContentRoot(env) + "ConfigFramework.json";
            }
        }

        /// <summary>
        /// Init default file ConfigFramework.json
        /// </summary>
        internal static void Init(IHostingEnvironment env = null)
        {
            if (!File.Exists(FileName(env)))
            {
                ConfigFramework configFramework = new ConfigFramework();
                configFramework.IsServerSideRendering = true;
                configFramework.IsCustomIndexHtml = true;
                configFramework.WebsiteList = new List<ConfigFrameworkWebsite>();
                Save(configFramework, env);
            }
        }

        internal static ConfigFramework Load(IHostingEnvironment env = null)
        {
            var result = UtilFramework.ConfigLoad<ConfigFramework>(FileName(env));
            if (result.WebsiteList == null)
            {
                result.WebsiteList = new List<ConfigFrameworkWebsite>();
            }
            return result;
        }

        internal static void Save(ConfigFramework configFramework, IHostingEnvironment env = null)
        {
            UtilFramework.ConfigSave(configFramework, FileName(env));
        }
    }

    public class ConfigFrameworkWebsite
    {
        public string DomainName { get; set; }
    }
}
