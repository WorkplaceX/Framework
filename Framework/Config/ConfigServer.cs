﻿namespace Framework.Config
{
    using Framework.App;
    using Framework.DataAccessLayer;
    using Framework.Server;
    using Microsoft.Extensions.Configuration;
    using System;
    using System.Collections.Generic;
    using System.IO;

    /// <summary>
    /// Configuration used by deployed web server. This file is generated by cli build command. It is an
    /// extract of ConfigCli depending on current EnvironmentName. See also method ConfigCli.CopyConfigCliToConfigServer();
    /// </summary>
    internal class ConfigServer
    {
        /// <summary>
        /// Gets or sets EnvironmentName. For example DEV, TEST or PROD.
        /// </summary>
        public string EnvironmentName { get; set; }

        /// <summary>
        /// Gets or sets IsUseDeveloperExceptionPage. If true, show detailed exceptions. Restart web server after value change!
        /// </summary>
        public bool IsUseDeveloperExceptionPage { get; set; }

        /// <summary>
        /// Gets or sets IsUseHttpsRedirection. If true, http is redirected to https. By default this value is false. Restart web server after value change!
        /// </summary>
        public bool IsUseHttpsRedirection { get; set; }

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
        /// Gets or sets BingMapKey. See also class BingMap.
        /// </summary>
        public string BingMapKey { get; set; }

        /// <summary>
        /// Returns ConnectionString. Used by WebServer  and Cli.
        /// </summary>
        public static string ConnectionString(bool isFrameworkDb)
        {
            string connectionStringApplication = null;
            string connectionStringFramework = null;

            // Application running on WebServer? (or cli)
            if (UtilServer.Context != null)
            {
                var configuration = (IConfiguration)UtilServer.Context.RequestServices.GetService(typeof(IConfiguration));

                // ConnectionString defined in file appsettings.json (or Azure) has higher priority than file ConfigServer.json.
                // Typically this is used on a PROD WebServer only. No ConnectionString on CI server is needed in ConfigCli secret.
                // For appsettings.json see also: https://docs.microsoft.com/en-us/ef/core/miscellaneous/connection-strings
                connectionStringApplication = UtilFramework.StringNull(ConfigurationExtensions.GetConnectionString(configuration, "ConnectionStringApplication"));
                connectionStringFramework = UtilFramework.StringNull(ConfigurationExtensions.GetConnectionString(configuration, "ConnectionStringFramework"));
            }

            if (isFrameworkDb == false)
            {
                if (connectionStringApplication != null)
                {
                    return connectionStringApplication;
                }
                return ConfigServer.Load().ConnectionStringApplication;
            }
            else
            {
                if (connectionStringFramework != null)
                {
                    return connectionStringFramework;
                }
                return ConfigServer.Load().ConnectionStringFramework;
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

        public List<ConfigServerWebsite> WebsiteList { get; set; }

        /// <summary>
        /// Gets ConfigServer.json. Created und updated by CommandBuild. See also publish folder.
        /// </summary>
        private static string FileName
        {
            get
            {
                if (UtilServer.IsIssServer == false)
                {
                    return UtilFramework.FolderName + "ConfigServer.json";
                }
                else
                {
                    return UtilServer.FolderNameContentRoot() + "ConfigServer.json";
                }
            }
        }

        /// <summary>
        /// Returns default ConfigServer.json
        /// </summary>
        private static ConfigServer Init()
        {
            ConfigServer result = new ConfigServer();
            result.IsServerSideRendering = true;
            result.WebsiteList = new List<ConfigServerWebsite>();
            return result;
        }

        internal static ConfigServer Load()
        {
            ConfigServer result;
            if (File.Exists(FileName))
            {
                result = UtilFramework.ConfigLoad<ConfigServer>(FileName);
            }
            else
            {
                result = Init();
            }

            if (result.WebsiteList == null)
            {
                result.WebsiteList = new List<ConfigServerWebsite>();
            }

            var folderNameAngularList = new Dictionary<string, int>();
            int folderNameAngularIndex = 0;
            foreach (var website in result.WebsiteList)
            {
                // Init DomainNameList
                if (website.DomainNameList == null)
                {
                    website.DomainNameList = new List<ConfigServerWebsiteDomain>();
                }

                // Init FolderNameAngularIndex
                var folderNameAngular = UtilFramework.FolderNameParse(website.FolderNameAngular);
                if (folderNameAngular != null)
                {
                    if (folderNameAngularList.ContainsKey(folderNameAngular) == false)
                    {
                        website.FolderNameAngularIndex = folderNameAngularIndex;
                        folderNameAngularIndex += 1;
                    }
                    else
                    {
                        website.FolderNameAngularIsDuplicate = true;
                        website.FolderNameAngularIndex = folderNameAngularList[folderNameAngular];
                    }
                }
            }
            return result;
        }

        internal static void Save(ConfigServer configServer)
        {
            UtilFramework.ConfigSave(configServer, FileName);
        }
    }

    internal class ConfigServerWebsite
    {
        /// <summary>
        /// Returns FolderNameServer. For example: "Application.Server/Framework/Application.Website/Website01/browser/".
        /// </summary>
        public string FolderNameServerGet(AppSelector appSelector, string prefixRemove)
        {
            string result = "Application.Server/Framework/Application.Website/" + appSelector.Website.FolderNameAngularWebsite + "browser/";
            result = result.Substring(prefixRemove.Length);

            return result;
        }

        /// <summary>
        /// Gets or sets FolderNameAngular. This is the FolderName when running on the server.
        /// </summary>
        public string FolderNameAngular { get; set; }

        /// <summary>
        /// Gets or sets FolderNameAngularIndex. Two websites with same FolderNameAngular get same index.
        /// </summary>
        internal int? FolderNameAngularIndex { get; set; }

        /// <summary>
        /// Gets FolderNameAngularWebsite. For example Website01.
        /// </summary>
        internal string FolderNameAngularWebsite => string.Format("Website{0:00}/", FolderNameAngularIndex.GetValueOrDefault() + 1);

        /// <summary>
        /// Gets FolderNameAngularPort. Used for SSR when running in Visual Studio.
        /// </summary>
        internal int FolderNameAngularPort => 4000 + 1 + FolderNameAngularIndex.GetValueOrDefault();

        /// <summary>
        /// Gets or sets FolderNameAngularIsDuplicate. True, if another website has the same FolderNameAngular.
        /// </summary>
        internal bool FolderNameAngularIsDuplicate { get; set; }

        public List<ConfigServerWebsiteDomain> DomainNameList { get; set; }
    }

    /// <summary>
    /// DomainName to AppTypeName.
    /// </summary>
    internal class ConfigServerWebsiteDomain
    {
        public string DomainName { get; set; }

        /// <summary>
        /// Gets or sets AppTypeName. Needs to derrive from AppJson. For example: "Application.AppJson, Application". If null, index.html is rendered without server side rendering.
        /// </summary>
        public string AppTypeName { get; set; }
    }
}
