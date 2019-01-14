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
        /// Gets or sets IsServerSideRendering. By default this value is set to true. Can be changed on the deployed server for trouble shooting.
        /// </summary>
        public bool IsServerSideRendering { get; set; }

        /// <summary>
        /// Gets or sets IsUseDeveloperExceptionPage. By default this value is set to false. Can be changed on the deployed server for trouble shooting.
        /// </summary>
        public bool IsUseDeveloperExceptionPage { get; set; }

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
                    website.DomainNameList = new List<string>();
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
        /// Gets or sets FolderNameServer. Folder relative to "Application.Server/Framework/Website/".
        /// </summary>
        public string FolderNameServer;

        /// <summary>
        /// Gets or sets AppTypeName. Needs to derrive from AppJson.
        /// </summary>
        public string AppTypeName;

        /// <summary>
        /// Gets or sets DomainNameList. For example (example.com) or empty for default website.
        /// </summary>
        public List<string> DomainNameList { get; set; }
    }
}