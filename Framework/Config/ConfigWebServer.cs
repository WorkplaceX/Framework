﻿namespace Framework.Config
{
    using Framework.DataAccessLayer;
    using Framework.Server;
    using System;
    using System.Collections.Generic;
    using System.IO;

    /// <summary>
    /// Configuration used by deployed web server. This file is generated by cli build command.
    /// </summary>
    internal class ConfigWebServer
    {
        /// <summary>
        /// Gets or sets EnvironmentName. For example DEV, TEST or PROD.
        /// </summary>
        public string EnvironmentName { get; set; }

        /// <summary>
        /// Gets or sets IsUseDeveloperExceptionPage. If true, show detailed exceptions.
        /// </summary>
        public bool IsUseDeveloperExceptionPage { get; set; }

        /// <summary>
        /// Gets or sets IsServerSideRendering. By default this value is true. Can be changed on the deployed server for trouble shooting.
        /// </summary>
        public bool IsServerSideRendering { get; set; }

        /// <summary>
        /// Gets or sets ConnectionStringFramework. This value is copied from ConfigCli.ConnectionStringFramework by cli build command.
        /// </summary>
        public string ConnectionStringFramework { get; set; }

        /// <summary>
        /// Gets or sets ConnectionStringApplication. This value is copied from CliConfig.ConnectionStringApplication by cli build command.
        /// </summary>
        public string ConnectionStringApplication { get; set; }

        /// <summary>
        /// Returns WebServer ConnectionString.
        /// </summary>
        public static string ConnectionString(bool isFrameworkDb)
        {
            var configWebServer = ConfigWebServer.Load();
            if (isFrameworkDb == false)
            {
                return configWebServer.ConnectionStringApplication;
            }
            else
            {
                return configWebServer.ConnectionStringFramework;
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

        public List<ConfigWebServerWebsite> WebsiteList { get; set; }

        /// <summary>
        /// Gets ConfigWebServer.json. Created und updated by CommandBuild. See also publish folder.
        /// </summary>
        private static string FileName
        {
            get
            {
                if (UtilServer.IsIssServer == false)
                {
                    return UtilFramework.FolderName + "ConfigWebServer.json";
                }
                else
                {
                    return UtilServer.FolderNameContentRoot() + "ConfigWebServer.json";
                }
            }
        }

        /// <summary>
        /// Init default file ConfigWebServer.json
        /// </summary>
        internal static void Init()
        {
            if (!File.Exists(FileName))
            {
                ConfigWebServer configWebServer = new ConfigWebServer();
                configWebServer.IsServerSideRendering = true;
                configWebServer.WebsiteList = new List<ConfigWebServerWebsite>();
                Save(configWebServer);
            }
        }

        internal static ConfigWebServer Load()
        {
            if (!File.Exists(FileName))
            {
                throw new Exception(string.Format("File not fount! Try to run cli build command first ({0})", FileName));
            }
            var result = UtilFramework.ConfigLoad<ConfigWebServer>(FileName);
            if (result.WebsiteList == null)
            {
                result.WebsiteList = new List<ConfigWebServerWebsite>();
            }
            foreach (var website in result.WebsiteList)
            {
                if (website.DomainNameList == null)
                {
                    website.DomainNameList = new List<ConfigWebServerWebsiteDomain>();
                }
            }
            return result;
        }

        internal static void Save(ConfigWebServer configWebServer)
        {
            UtilFramework.ConfigSave(configWebServer, FileName);
        }
    }

    internal class ConfigWebServerWebsite
    {
        /// <summary>
        /// Gets or sets FolderNameServer. For example: "Application.Server\Framework\Website\Default".
        /// </summary>
        public string FolderNameServer { get; set; }

        public string FolderNameServerGet(string prefixRemove)
        {
            UtilFramework.Assert(FolderNameServer != null && FolderNameServer.StartsWith("Application.Server/Framework/Application.Website/"), "FolderNameServer has to start with 'Application.Server/Framework/Application.Website/'!");
            UtilFramework.Assert(FolderNameServer.StartsWith(prefixRemove));
            string result = FolderNameServer;
            result = result.Substring(prefixRemove.Length);
            return result;
        }

        public List<ConfigWebServerWebsiteDomain> DomainNameList { get; set; }
    }

    /// <summary>
    /// DomainName to AppTypeName.
    /// </summary>
    internal class ConfigWebServerWebsiteDomain
    {
        public string DomainName { get; set; }

        /// <summary>
        /// Gets or sets AppTypeName. Needs to derrive from AppJson. For example: "Application.AppJson, Application". If null, index.html is rendered without server side rendering.
        /// </summary>
        public string AppTypeName { get; set; }
    }
}
