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
                UtilCliInternal.ConsoleWriteLineColor(string.Format("Warning! Type not found! ({0})", appTypeName), ConsoleColor.Yellow); // Warning
            }

            // Add Website
            ConfigCliWebsite website = new ConfigCliWebsite();
            website.DomainNameList = new List<ConfigCliWebsiteDomain>();
            website.DomainNameList.Add(new ConfigCliWebsiteDomain() { EnvironmentName = configCli.EnvironmentNameGet(), DomainName = domainName, AppTypeName = appTypeName });

            configCli.WebsiteList.Add(website);

            ConfigCli.Save(configCli);
        }

        /// <summary>
        /// Write config ConnectionStringFramework and ConnectionStringApplication to ConfigCli.json and ConfigServer.json.
        /// </summary>
        private void ArgumentConnectionString()
        {
            ConfigCli configCli = ConfigCli.Load();
            if (UtilCliInternal.ArgumentValue(this, argumentConnectionString, out string connectionString))
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
            if (UtilCliInternal.ArgumentValue(this, argumentConnectionStringFramework, out string connectionString))
            {
                // Write
                configCli.EnvironmentGet().ConnectionStringFramework = connectionString;
                ConfigCli.Save(configCli);
            }
            else
            {
                // Read
                UtilCliInternal.ConsoleWriteLinePassword(argumentConnectionStringFramework.Name + "=" + configCli.EnvironmentGet().ConnectionStringFramework);
            }
        }

        /// <summary>
        /// Read or write config ConnectionStringApplication.
        /// </summary>
        private void ArgumentConnectionStringApplication()
        {
            ConfigCli configCli = ConfigCli.Load();
            if (UtilCliInternal.ArgumentValue(this, argumentConnectionStringApplication, out string connectionString))
            {
                // Write
                configCli.EnvironmentGet().ConnectionStringApplication = connectionString;
                ConfigCli.Save(configCli);
            }
            else
            {
                // Read
                UtilCliInternal.ConsoleWriteLinePassword(argumentConnectionStringApplication.Name + "=" + configCli.EnvironmentGet().ConnectionStringApplication);
            }
        }

        /// <summary>
        /// Removes ConnectionString from ConfigCli. Used for CI server if WebServer manages ConnectionString.
        /// </summary>
        private static void ConnectionStringRemove(ConfigCli configCli)
        {
            if (configCli.EnvironmentList != null)
            {
                foreach (var environment in configCli.EnvironmentList)
                {
                    environment.ConnectionStringFramework = null;
                    environment.ConnectionStringApplication = null;
                }
            }
        }

        /// <summary>
        /// Removes not by environment selected Environment and DomainName.
        /// </summary>
        private static void EnvironmentRemove(ConfigCli configCli)
        {
            var environmentName = configCli.EnvironmentName;

            if (configCli.EnvironmentList != null)
            {
                foreach (var environment in configCli.EnvironmentList.ToArray())
                {
                    if (environment.EnvironmentName != environmentName)
                    {
                        configCli.EnvironmentList.Remove(environment);
                    }
                }
            }

            if (configCli.WebsiteList != null)
            {
                foreach (var website in configCli.WebsiteList)
                {
                    if (website.DomainNameList != null)
                    {
                        foreach (var domainName in website.DomainNameList.ToArray())
                        {
                            if (domainName.EnvironmentName != environmentName)
                            {
                                website.DomainNameList.Remove(domainName);
                            }
                        }
                    }
                }
            }
        }

        protected internal override void Execute()
        {
            ConfigCli configCli = ConfigCli.Load();

            // Command "json"
            if (UtilCliInternal.ArgumentValueIsDelete(this, argumentJson))
            {
                if (UtilCliInternal.ArgumentValue(this, argumentJson, out string json))
                {
                    // Set ConfigCli.json with command: ".\wpx.cmd config json='{}'"
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
            if (UtilCliInternal.ArgumentValueIsDelete(this, argumentDeployAzureGitUrl))
            {
                if (UtilCliInternal.ArgumentValue(this, argumentDeployAzureGitUrl, out string value))
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
            if (UtilCliInternal.ArgumentValueIsDelete(this, argumentConnectionString))
            {
                ArgumentConnectionString();
                return;
            }

            // Command "connectionStringFramework"
            if (UtilCliInternal.ArgumentValueIsDelete(this, argumentConnectionStringFramework))
            {
                ArgumentConnectionStringFramework();
                return;
            }

            // Command "connectionStringApplication"
            if (UtilCliInternal.ArgumentValueIsDelete(this, argumentConnectionStringApplication))
            {
                ArgumentConnectionStringApplication();
                return;
            }

            // Command "website"
            if (UtilCliInternal.ArgumentValueIsDelete(this, argumentWebsite))
            {
                ArgumentWebsite();
                return;
            }

            // Print ConfigCli.json to screen
            {
                configCli = ConfigCli.Load();
                Console.WriteLine();
                UtilCliInternal.ConsoleWriteLineColor("Add the following environment variable to ci build server: (Value including double quotation marks!)", ConsoleColor.Green);

                // Remove ConnectionString
                if (UtilCliInternal.ArgumentValueIsDelete(this, argumentJson) == false) // No user interaction when json argument used.
                {
                    if (UtilCliInternal.ConsoleReadYesNo("Include ConnectionString? (CI Server does not need it if managed by WebServer)") == false)
                    {
                        ConnectionStringRemove(configCli);
                    }
                }

                // Remove for example DomainName localhost from DEV environment if PROD environment is selected.
                EnvironmentRemove(configCli);

                string json = UtilFramework.ConfigToJson(configCli, isIndented: false);
                json = json.Replace("\"", "'"); // To use it in command prompt.
                UtilCliInternal.ConsoleWriteLineColor(configCli.EnvironmentGet().EnvironmentName + " " + "ConfigCli=", ConsoleColor.DarkGreen);
                UtilCliInternal.ConsoleWriteLineColor(string.Format("\"{0}\"", json), ConsoleColor.DarkGreen);
                Console.WriteLine();
            }
        }
    }
}
