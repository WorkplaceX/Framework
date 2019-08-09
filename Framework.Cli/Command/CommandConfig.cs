namespace Framework.Cli.Command
{
    using Framework.Cli.Config;
    using Microsoft.Extensions.CommandLineUtils;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    public class CommandConfig : CommandBase
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
            argumentWebsite = configuration.Argument("website", "Add (include) a website to ci build.");
        }

        private void ArgumentWebsite()
        {
            // Input FolderNameServer
            UtilCli.ConsoleWriteLineColor("Add (include) a website", ConsoleColor.Yellow);
            Console.WriteLine("Enter FolderNameServer. For example: 'example.com':");
            Console.Write(">");
            string folderNameServer = Console.ReadLine();
            folderNameServer = UtilFramework.FolderNameParse(folderNameServer);

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
                UtilCli.ConsoleWriteLineColor(string.Format("Type not found! ({0})", appTypeName), ConsoleColor.Red);
            }

            // Input FolderName
            Console.WriteLine("Enter npm build folder name. Or empty if no build. For example: 'Website/'. In this folder ci calls npm install; npm build;");
            Console.Write(">");
            string folderNameNpmBuild = Console.ReadLine();
            folderNameNpmBuild = UtilFramework.FolderNameParse(folderNameNpmBuild);

            string folderNameNpmBuildCheck = UtilFramework.FolderName + folderNameNpmBuild;
            if (!Directory.Exists(folderNameNpmBuildCheck))
            {
                UtilCli.ConsoleWriteLineColor(string.Format("Folder does not exist! ({0})", folderNameNpmBuild), ConsoleColor.Red);
            }

            // Input FolderNameDist
            Console.WriteLine("Enter dist folder name. For example 'Website/dist/'. Content of this folder is copied to 'Application.Server/Framework/Website/{FolderNameServer}'");
            Console.Write(">");
            string folderNameDist = Console.ReadLine();
            folderNameDist = UtilFramework.FolderNameParse(folderNameDist);
            string folderNameDistCheck = UtilFramework.FolderName + folderNameDist;
            if (!Directory.Exists(folderNameDistCheck))
            {
                UtilCli.ConsoleWriteLineColor(string.Format("Folder does not exist! ({0})", folderNameDist), ConsoleColor.Red);
            }

            // Add Website
            ConfigCliWebsite website = new ConfigCliWebsite();
            website.FolderNameServer = folderNameServer;
            website.DomainNameList = new List<string>() { domainName };
            website.AppTypeName = appTypeName;
            website.FolderNameNpmBuild = folderNameNpmBuild;
            website.FolderNameDist = folderNameDist;
            ConfigCli configCli = ConfigCli.Load();
            if (configCli.WebsiteList == null)
            {
                configCli.WebsiteList = new List<ConfigCliWebsite>();
            }
            ConfigCliWebsite websiteFind = configCli.WebsiteList.Where(item => item.FolderNameServer.ToLower() == folderNameServer.ToLower()).SingleOrDefault();
            if (websiteFind != null)
            {
                configCli.WebsiteList.Remove(websiteFind);
            }
            configCli.WebsiteList.Add(website);

            ConfigCli.Save(configCli);
        }

        /// <summary>
        /// Write config ConnectionStringFramework and ConnectionStringApplication to ConfigCli.json and ConfigWebServer.json.
        /// </summary>
        private void ArgumentConnectionString()
        {
            ConfigCli configCli = ConfigCli.Load();
            if (UtilCli.ArgumentValue(this, argumentConnectionString, out string connectionString))
            {
                // Write
                configCli.ConnectionStringFramework = connectionString;
                configCli.ConnectionStringApplication = connectionString;
                ConfigCli.Save(configCli);

                // Copy values to ConfigWebServer.json
                CommandBuild.InitConfigWebServer(AppCli); // Update ConfigWebServer.json 
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
                configCli.ConnectionStringFramework = connectionString;
                ConfigCli.Save(configCli);
            }
            else
            {
                // Read
                UtilCli.ConsoleWriteLinePassword(argumentConnectionStringFramework.Name + "=" + configCli.ConnectionStringFramework);
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
                configCli.ConnectionStringApplication = connectionString;
                ConfigCli.Save(configCli);
            }
            else
            {
                // Read
                UtilCli.ConsoleWriteLinePassword(argumentConnectionStringApplication.Name + "=" + configCli.ConnectionStringApplication);
            }
        }

        protected internal override void Execute()
        {
            ConfigCli.Init(AppCli);
            ConfigCli configCli = ConfigCli.Load();

            // Command "json"
            if (UtilCli.ArgumentValueIsExist(this, argumentJson))
            {
                if (UtilCli.ArgumentValue(this, argumentJson, out string json))
                {
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
            if (UtilCli.ArgumentValueIsExist(this, argumentDeployAzureGitUrl))
            {
                if (UtilCli.ArgumentValue(this, argumentDeployAzureGitUrl, out string value))
                {
                    // Write
                    configCli.DeployAzureGitUrl = value;
                    ConfigCli.Save(configCli);
                }
                else
                {
                    // Read
                    Console.WriteLine(argumentDeployAzureGitUrl.Name + "=" + configCli.DeployAzureGitUrl);
                }
            }

            // Command "connectionString"
            if (UtilCli.ArgumentValueIsExist(this, argumentConnectionString))
            {
                ArgumentConnectionString();
            }

            // Command "connectionStringFramework"
            if (UtilCli.ArgumentValueIsExist(this, argumentConnectionStringFramework))
            {
                ArgumentConnectionStringFramework();
            }

            // Command "connectionStringApplication"
            if (UtilCli.ArgumentValueIsExist(this, argumentConnectionStringApplication))
            {
                ArgumentConnectionStringApplication();
            }

            // Command "website"
            if (UtilCli.ArgumentValueIsExist(this, argumentWebsite))
            {
                ArgumentWebsite();
            }

            // Read
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
