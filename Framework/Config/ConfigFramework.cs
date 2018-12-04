namespace Framework.Config
{
    using Framework.Dal;
    using Framework.Server;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;

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


        /// <summary>
        /// Returns ConnectionString for Application or Framework database.
        /// </summary>
        /// <param name="typeRow">Application or Framework data row.</param>
        public static string ConnectionString(Type typeRow)
        {
            bool isFrameworkDb = UtilDalType.TypeRowIsFrameworkDb(typeRow);
            return ConnectionString(isFrameworkDb);
        }

        public List<ConfigFrameworkWebsite> WebsiteList { get; set; }

        /// <summary>
        /// Gets ConfigFramework.json. Created und updated by CommandBuild. See also publish folder.
        /// </summary>
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
            if (!File.Exists(FileName))
            {
                throw new Exception(string.Format("File not fount! Try to run cli build command first ({0})", FileName));
            }
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
