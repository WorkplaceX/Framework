namespace Framework.Cli.Command
{
    using Framework.Cli.Config;
    using Microsoft.Extensions.CommandLineUtils;
    using System;
    using System.Collections.Generic;
    using System.IO;

    /// <summary>
    /// Cli config command.
    /// </summary>
    internal class CommandConfig : CommandBase
    {
        public CommandConfig(AppCli appCli)
            : base(appCli, "config", "Read and write configuration")
        {

        }

        private CommandArgument argumentJson;

        private CommandArgument argumentDeployAzureGitUrl;

        private CommandArgument argumentConnectionString;

        private CommandArgument argumentConnectionStringFramework;

        private CommandArgument argumentConnectionStringApplication;

        private CommandArgument argumentWebsite;

        protected internal override void Register(CommandLineApplication configuration)
        {
            argumentJson = configuration.Argument("json", "Get or set ci server configuration.");
            argumentDeployAzureGitUrl = configuration.Argument("deployAzureGitUrl", "Get or set Azure git url for deployment.");
            argumentConnectionString = configuration.Argument("connectionString", "Set same database ConnectionString for Framework and Application.");
            argumentConnectionStringFramework = configuration.Argument("connectionStringFramework", "Get or set database ConnectionString for Framework.");
            argumentConnectionStringApplication = configuration.Argument("connectionStringApplication", "Get or set database ConnectionString for Application.");
            argumentWebsite = configuration.Argument("website", "Add (include) a layout website to ci build.");
        }

        private void ArgumentWebsite()
        {
            ConfigCli configCli = ConfigCli.Load();

            // Input DomainName
            Console.WriteLine("Enter domain name. For example: 'example.com' or empty for default website:");
            Console.Write(">");
            string domainName = Console.ReadLine();

            // Input AppTypeName
            Console.WriteLine("Enter AppTypeName. For example: 'Application.AppMain, Application':");
            Console.Write(">");
            string appTypeName = Console.ReadLine();
            if (Type.GetType(appTypeName) == null)
            {
                UtilCli.ConsoleWriteLineColor(string.Format("Warning! Type not found! ({0})", appTypeName), ConsoleColor.Yellow); // Warning
            }

            // Input FolderName
            Console.WriteLine("Enter npm build folder name. Or empty if no build. For example: 'Application.Website/LayoutDefault/'. In this folder ci calls npm install; npm build;");
            Console.Write(">");
            string folderNameNpmBuild = Console.ReadLine();
            folderNameNpmBuild = UtilFramework.FolderNameParse(folderNameNpmBuild);

            string folderNameNpmBuildCheck = UtilFramework.FolderName + folderNameNpmBuild;
            if (!Directory.Exists(folderNameNpmBuildCheck))
            {
                UtilCli.ConsoleWriteLineColor(string.Format("Warning! Folder does not exist! ({0})", folderNameNpmBuild), ConsoleColor.Yellow); // Warning
            }

            // Input FolderNameDist
            Console.WriteLine("Enter dist folder name. For example 'Application.Website/LayoutDefault/dist/'. Content of this folder is copied to FolderNameServer");
            Console.Write(">");
            string folderNameDist = Console.ReadLine();
            folderNameDist = UtilFramework.FolderNameParse(folderNameDist);
            string folderNameDistCheck = UtilFramework.FolderName + folderNameDist;
            if (!Directory.Exists(folderNameDistCheck))
            {
                UtilCli.ConsoleWriteLineColor(string.Format("Warning! Folder does not exist! ({0})", folderNameDist), ConsoleColor.Yellow); // Warning
            }

            // Add Website
            ConfigCliWebsite website = new ConfigCliWebsite();
            website.DomainNameList = new List<ConfigCliWebsiteDomain>();
            website.DomainNameList.Add(new ConfigCliWebsiteDomain() { EnvironmentName = configCli.EnvironmentNameGet(), DomainName = domainName, AppTypeName = appTypeName });
            website.FolderNameNpmBuild = folderNameNpmBuild;
            website.FolderNameDist = folderNameDist;

            configCli.WebsiteList.Add(website);

            ConfigCli.Save(configCli);
        }

        /// <summary>
        /// Write config ConnectionStringFramework and ConnectionStringApplication to ConfigCli.json and ConfigServer.json.
        /// </summary>
        private void ArgumentConnectionString()
        {
            ConfigCli configCli = ConfigCli.Load();
            if (UtilCli.ArgumentValue(this, argumentConnectionString, out string connectionString))
            {
                // Write
                configCli.EnvironmentGet().ConnectionStringFramework = connectionString;
                configCli.EnvironmentGet().ConnectionStringApplication = connectionString;
                ConfigCli.Save(configCli);
            }
        }

        /// <summary>
        /// Read or write config ConnectionStringFramework.
        /// </summary>
        private void ArgumentConnectionStringFramework()
        {
            ConfigCli configCli = ConfigCli.Load();
            if (UtilCli.ArgumentValue(this, argumentConnectionStringFramework, out string connectionString))
            {
                // Write
                configCli.EnvironmentGet().ConnectionStringFramework = connectionString;
                ConfigCli.Save(configCli);
            }
            else
            {
                // Read
                UtilCli.ConsoleWriteLinePassword(argumentConnectionStringFramework.Name + "=" + configCli.EnvironmentGet().ConnectionStringFramework);
            }
        }

        /// <summary>
        /// Read or write config ConnectionStringApplication.
        /// </summary>
        private void ArgumentConnectionStringApplication()
        {
            ConfigCli configCli = ConfigCli.Load();
            if (UtilCli.ArgumentValue(this, argumentConnectionStringApplication, out string connectionString))
            {
                // Write
                configCli.EnvironmentGet().ConnectionStringApplication = connectionString;
                ConfigCli.Save(configCli);
            }
            else
            {
                // Read
                UtilCli.ConsoleWriteLinePassword(argumentConnectionStringApplication.Name + "=" + configCli.EnvironmentGet().ConnectionStringApplication);
            }
        }

        protected internal override void Execute()
        {
            ConfigCli configCli = ConfigCli.Load();

            // Command "json"
            if (UtilCli.ArgumentValueIsDelete(this, argumentJson))
            {
                if (UtilCli.ArgumentValue(this, argumentJson, out string json))
                {
                    // Set ConfigCli.json with command: ".\cli.cmd config json='{}'"
                    json = json.Trim('\"'); // Remove quotation marks at the begin and end. 
                    json = json.Replace("'", "\""); // To use it in command prompt.
                    // Write
                    try
                    {
                        configCli = UtilFramework.ConfigFromJson<ConfigCli>(json);
                    }
                    catch (Exception exception)
                    {
                        throw new Exception("ConfigCli invalid!", exception);
                    }
                    ConfigCli.Save(configCli);
                }
            }

            // Command "deployAzureGitUrl"
            if (UtilCli.ArgumentValueIsDelete(this, argumentDeployAzureGitUrl))
            {
                if (UtilCli.ArgumentValue(this, argumentDeployAzureGitUrl, out string value))
                {
                    // Write
                    configCli.EnvironmentGet().DeployAzureGitUrl = value;
                    ConfigCli.Save(configCli);
                }
                else
                {
                    // Read
                    Console.WriteLine(argumentDeployAzureGitUrl.Name + "=" + configCli.EnvironmentGet().DeployAzureGitUrl);
                }
                return;
            }

            // Command "connectionString"
            if (UtilCli.ArgumentValueIsDelete(this, argumentConnectionString))
            {
                ArgumentConnectionString();
                return;
            }

            // Command "connectionStringFramework"
            if (UtilCli.ArgumentValueIsDelete(this, argumentConnectionStringFramework))
            {
                ArgumentConnectionStringFramework();
                return;
            }

            // Command "connectionStringApplication"
            if (UtilCli.ArgumentValueIsDelete(this, argumentConnectionStringApplication))
            {
                ArgumentConnectionStringApplication();
                return;
            }

            // Command "website"
            if (UtilCli.ArgumentValueIsDelete(this, argumentWebsite))
            {
                ArgumentWebsite();
                return;
            }

            // Print ConfigCli.json to screen
            {
                configCli = ConfigCli.Load();
                Console.WriteLine();
                UtilCli.ConsoleWriteLineColor("Add the following environment variable to ci build server: (Value including double quotation marks!)", ConsoleColor.Green);
                string json = UtilFramework.ConfigToJson(configCli, isIndented: false);
                json = json.Replace("\"", "'"); // To use it in command prompt.
                UtilCli.ConsoleWriteLineColor("ConfigCli=", ConsoleColor.DarkGreen);
                UtilCli.ConsoleWriteLineColor(string.Format("\"{0}\"", json), ConsoleColor.DarkGreen);
                Console.WriteLine();
            }
        }
    }
}
