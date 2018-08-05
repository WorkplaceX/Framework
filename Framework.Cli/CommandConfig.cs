namespace Framework.Cli
{
    using Microsoft.Extensions.CommandLineUtils;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    public class CommandConfig : CommandBase
    {
        public CommandConfig(AppCliBase appCli)
            : base(appCli, "config", "Read and write configuration")
        {

        }

        private CommandArgument jsonArgument;

        private CommandArgument azureGitUrlArgument;

        private CommandArgument websiteArgument;

        protected internal override void Register(CommandLineApplication configuration)
        {
            jsonArgument = configuration.Argument("json", "Get or set configuration");
            azureGitUrlArgument = configuration.Argument("azureGitUrl", "Get or set Azure git url");
            websiteArgument = configuration.Argument("website", "Add (include) a website in this repo.");
        }

        private void Website()
        {
            // Input DomainName
            UtilFramework.ConsoleWriteLineColor("Add (include) a website", ConsoleColor.Yellow);
            Console.WriteLine("Enter domain name. For example: 'example.com' or empty for 'default':");
            Console.Write(">");
            string domainName = Console.ReadLine();
            if (domainName == "")
            {
                domainName = "default";
            }

            // Input FolderName
            Console.WriteLine("Enter npm build folder name. Or empty if no build. For example: 'Website/'. In this folder ci will call npm install; npm build;");
            Console.Write(">");
            string folderNameNpmBuild = Console.ReadLine();
            if (folderNameNpmBuild.StartsWith("/") || folderNameNpmBuild.StartsWith(@"\"))
            {
                folderNameNpmBuild = folderNameNpmBuild.Substring(1);
            }
            if (folderNameNpmBuild.EndsWith("/") || folderNameNpmBuild.EndsWith(@"\"))
            {
                folderNameNpmBuild = folderNameNpmBuild.Substring(0, folderNameNpmBuild.Length - 1);
            }
            folderNameNpmBuild = folderNameNpmBuild.Replace(@"\", "/");
            folderNameNpmBuild += "/";
            string folderNameNpmBuildCheck = UtilFramework.FolderName + folderNameNpmBuild;
            if (!Directory.Exists(folderNameNpmBuildCheck))
            {
                UtilFramework.ConsoleWriteLineColor(string.Format("Folder does not exist! ({0})", folderNameNpmBuild), ConsoleColor.Red);
            }

            // Input FolderNameDist
            Console.WriteLine("Enter dist folder name. For example 'Website/dist/'. Content of this folder will be copied to 'Application.Server/Framework/Website/{DomainName}'");
            Console.Write(">");
            string folderNameDist = Console.ReadLine();
            if (folderNameDist.StartsWith("/") || folderNameDist.StartsWith(@"\"))
            {
                folderNameDist = folderNameDist.Substring(1);
            }
            if (folderNameDist.EndsWith("/") || folderNameDist.EndsWith(@"\"))
            {
                folderNameDist = folderNameDist.Substring(0, folderNameDist.Length - 1);
            }
            folderNameDist = folderNameDist.Replace(@"\", "/");
            folderNameDist += "/";
            string folderNameDistCheck = UtilFramework.FolderName + folderNameDist;
            if (!Directory.Exists(folderNameDistCheck))
            {
                UtilFramework.ConsoleWriteLineColor(string.Format("Folder does not exist! ({0})", folderNameDist), ConsoleColor.Red);
            }

            // Add Website
            ConfigCliWebsite website = new ConfigCliWebsite();
            website.DomainName = domainName;
            website.FolderNameNpmBuild = folderNameNpmBuild;
            website.FolderNameDist = folderNameDist;
            ConfigCli configCli = ConfigCli.Load();
            if (configCli.WebsiteList == null)
            {
                configCli.WebsiteList = new List<ConfigCliWebsite>();
            }
            ConfigCliWebsite websiteFind = configCli.WebsiteList.Where(item => item.DomainName.ToLower() == domainName).SingleOrDefault();
            if (websiteFind != null)
            {
                configCli.WebsiteList.Remove(websiteFind);
            }
            configCli.WebsiteList.Add(website);

            ConfigCli.Save(configCli);
        }

        protected internal override void Execute()
        {
            ConfigCli configCli = ConfigCli.Load();

            // Command "json"
            if (UtilCli.ArgumentValueIsExist(this, jsonArgument))
            {
                if (UtilCli.ArgumentValue(this, jsonArgument, out string json))
                {
                    // Write
                    try
                    {
                        configCli = UtilFramework.ConfigFromJson<ConfigCli>(json);
                    }
                    catch (Exception exception)
                    {
                        throw new Exception("ConfigCliJson invalid!", exception);
                    }
                    ConfigCli.Save(configCli);
                }
            }

            // Command "azureGitUrl"
            if (UtilCli.ArgumentValueIsExist(this, azureGitUrlArgument))
            {
                if (UtilCli.ArgumentValue(this, azureGitUrlArgument, out string value))
                {
                    // Write
                    configCli.AzureGitUrl = value;
                    ConfigCli.Save(configCli);
                }
                else
                {
                    // Read
                    Console.WriteLine(azureGitUrlArgument.Name + "=" + configCli.AzureGitUrl);
                }
            }

            // Command "website"
            if (UtilCli.ArgumentValueIsExist(this, websiteArgument))
            {
                Website();
            }

            // Read
            {
                configCli = ConfigCli.Load();
                Console.WriteLine();
                UtilFramework.ConsoleWriteLineColor("Add environment variable to ci build server: (Value including double quotation marks!)", ConsoleColor.Green);
                string json = UtilFramework.ConfigToJson(configCli, isIndented: false);
                json = json.Replace("\"", "'"); // To use it in command prompt.
                UtilFramework.ConsoleWriteLineColor("ConfigCliJson=", ConsoleColor.DarkGreen);
                UtilFramework.ConsoleWriteLineColor(string.Format("\"{0}\"", json), ConsoleColor.DarkGreen);
                Console.WriteLine();
            }
        }
    }
}
