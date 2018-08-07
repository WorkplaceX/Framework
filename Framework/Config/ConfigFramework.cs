﻿namespace Framework.Config
{
    using Framework.Server;
    using System.Collections.Generic;
    using System.IO;

    public class ConfigFramework
    {
        public bool IsServerSideRendering { get; set; }

        public bool IsUseDeveloperExceptionPage { get; set; }

        public string ConnectionStringFramework { get; set; }

        public string ConnectionStringApplication { get; set; }

        public static string ConnectionString(bool isFrameworkDb)
        {
            var configFramework = ConfigFramework.Load();
            if (isFrameworkDb == false)
            {
                return configFramework.ConnectionStringApplication;
            }
            else
            {
                return configFramework.ConnectionStringFramework;
            }
        }

        public List<ConfigFrameworkWebsite> WebsiteList { get; set; }

        private static string FileName
        {
            get
            {
                if (UtilServer.IsIssServer == false)
                {
                    return UtilFramework.FolderName + "ConfigFramework.json";
                }
                else
                {
                    return UtilServer.FolderNameContentRoot() + "ConfigFramework.json";
                }
            }
        }

        /// <summary>
        /// Init default file ConfigFramework.json
        /// </summary>
        internal static void Init()
        {
            if (!File.Exists(FileName))
            {
                ConfigFramework configFramework = new ConfigFramework();
                configFramework.IsServerSideRendering = true;
                configFramework.WebsiteList = new List<ConfigFrameworkWebsite>();
                Save(configFramework);
            }
        }

        internal static ConfigFramework Load()
        {
            var result = UtilFramework.ConfigLoad<ConfigFramework>(FileName);
            if (result.WebsiteList == null)
            {
                result.WebsiteList = new List<ConfigFrameworkWebsite>();
            }
            return result;
        }

        internal static void Save(ConfigFramework configFramework)
        {
            UtilFramework.ConfigSave(configFramework, FileName);
        }
    }

    public class ConfigFrameworkWebsite
    {
        public string DomainName { get; set; }
    }
}