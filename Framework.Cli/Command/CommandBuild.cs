namespace Framework.Cli.Command
{
    using Framework.Cli.Config;
    using Framework.Config;
    using Microsoft.Extensions.CommandLineUtils;
    using System;
    using System.IO;
    using System.Linq;

    /// <summary>
    /// Cli build command.
    /// </summary>
    internal class CommandBuild : CommandBase
    {
        public CommandBuild(AppCli appCli)
            : base(appCli, "build", "Build Angular client and ASP.NET Core server")
        {

        }

        private CommandOption optionClientOnly;

        /// <summary>
        /// Gets or sets IsOptionClientOnly. Can be used by other commands. For example command start.
        /// </summary>
        internal bool IsOptionClientOnly;

        protected internal override void Register(CommandLineApplication configuration)
        {
            optionClientOnly = configuration.Option("-c|--client", "Build angular client only.", CommandOptionType.NoValue);
        }

        /// <summary>
        /// Build Framework/Framework.Angular/application/.
        /// </summary>
        private static void BuildAngular()
        {
            // Copy folder Application.Website
            {
                // Delete folder Application.Website
                string folderNameApplicationWebSite = UtilFramework.FolderName + "Framework/Framework.Angular/application/src/Application.Website";
                UtilCli.FolderDelete(folderNameApplicationWebSite);
                UtilFramework.Assert(!Directory.Exists(folderNameApplicationWebSite));

                // Copy folder CustomComponent
                string folderNameSource = UtilFramework.FolderName + "Application.Website/Shared/CustomComponent/";
                string folderNameDest = UtilFramework.FolderName + "Framework/Framework.Angular/application/src/Application.Website/Shared/CustomComponent/";
                UtilCli.FolderCopy(folderNameSource, folderNameDest, "*.*", true);

                // Copy empty index.html file
                UtilCli.FileCopy(UtilFramework.FolderName + "Framework/Framework.Angular/application/src/index.html", UtilFramework.FolderName + "Framework/Framework.Angular/application/src/Application.Website/Default/index.html");

                // Ensure folder exists now
                UtilFramework.Assert(Directory.Exists(folderNameApplicationWebSite));
            }

            // Build SSR
            {
                string folderName = UtilFramework.FolderName + "Framework/Framework.Angular/application/";
                UtilCli.Npm(folderName, "install --loglevel error"); // Angular install. --loglevel error prevent writing to STDERROR "npm WARN optional SKIPPING OPTIONAL DEPENDENCY"
                UtilCli.Npm(folderName, "run build:ssr", isRedirectStdErr: true); // Build Server-side Rendering (SSR) to folder Framework/Framework.Angular/application/server/dist/ // TODO Bug report Angular build writes to stderr. Repo steps: Delete node_modules and run npm install and then build.
            }

            // Copy output dist folder
            {
                string folderNameSource = UtilFramework.FolderName + "Framework/Framework.Angular/application/dist/application/";
                string folderNameDest = UtilFramework.FolderName + "Application.Server/Framework/Framework.Angular/";

                // Copy folder
                UtilCli.FolderDelete(folderNameDest);
                UtilFramework.Assert(!Directory.Exists(folderNameDest));
                UtilCli.FolderCopy(folderNameSource, folderNameDest, "*.*", true);
                UtilFramework.Assert(Directory.Exists(folderNameDest));
            }
        }

        /// <summary>
        /// Copy ConfigServer.json to publish folder.
        /// </summary>
        internal static void ConfigServerPublish()
        {
            string folderNamePublish = UtilFramework.FolderName + "Application.Server/bin/Debug/netcoreapp3.1/publish/";

            string fileNameSource = UtilFramework.FolderName + "ConfigServer.json";
            string fileNameDest = folderNamePublish + "ConfigServer.json";
            UtilCli.FileCopy(fileNameSource, fileNameDest);
        }

        private static void BuildServer()
        {
            string folderName = UtilFramework.FolderName + "Application.Server/";
            string folderNamePublish = UtilFramework.FolderName + "Application.Server/bin/Debug/netcoreapp3.1/publish/";

            UtilCli.FolderNameDelete(folderNamePublish);
            UtilFramework.Assert(!Directory.Exists(folderNamePublish), "Delete folder failed!");
            UtilCli.DotNet(folderName, "publish"); // Use publish instead to build.
            UtilFramework.Assert(Directory.Exists(folderNamePublish), "Deploy failed!");

            ConfigServerPublish();
        }

        /// <summary>
        /// Execute "npm run build" command.
        /// </summary>
        private static void BuildWebsiteNpm(ConfigCliWebsite website)
        {
            string folderNameNpmBuild = UtilFramework.FolderNameParse(website.FolderNameNpmBuild);
            if (UtilFramework.StringNull(folderNameNpmBuild) != null)
            {
                string folderName = UtilFramework.FolderName + folderNameNpmBuild;
                UtilCli.Npm(folderName, "install --loglevel error"); // --loglevel error prevent writing to STDERR "npm WARN optional SKIPPING OPTIONAL DEPENDENCY"
                UtilCli.Npm(folderName, "run build");
            }
        }

        /// <summary>
        /// Build all master Websites. For example: "Application.Website/MasterDefault"
        /// </summary>
        private void BuildWebsite()
        {
            var configCli = ConfigCli.Load();
            foreach (var website in configCli.WebsiteList)
            {
                Console.WriteLine(string.Format("### Build Website (Begin) - {0}", website.DomainNameListToString()));
                BuildWebsiteNpm(website);
                string folderNameServer = UtilFramework.FolderNameParse(website.FolderNameServerGet(configCli));
                UtilFramework.Assert(folderNameServer != null, "FolderNameServer can not be null!");
                UtilFramework.Assert(folderNameServer.StartsWith("Application.Server/Framework/Application.Website/"), "FolderNameServer has to start with 'Application.Server/Framework/Application.Website/'!");

                string folderNameDist = UtilFramework.FolderNameParse(website.FolderNameDist);
                if (folderNameDist != null)
                {
                    string folderNameSource = UtilFramework.FolderName + folderNameDist;
                    string folderNameDest = UtilFramework.FolderName + folderNameServer;
                    if (!UtilCli.FolderNameExist(folderNameSource))
                    {
                        throw new Exception(string.Format("Folder does not exist! ({0})", folderNameDest));
                    }

                    // Copy folder
                    UtilCli.FolderDelete(folderNameDest);
                    UtilFramework.Assert(!UtilCli.FolderNameExist(folderNameDest));
                    UtilCli.FolderCopy(folderNameSource, folderNameDest, "*.*", true);
                    UtilFramework.Assert(UtilCli.FolderNameExist(folderNameDest));
                }

                Console.WriteLine(string.Format("### Build Website (End) - {0}", website.DomainNameListToString()));
            }
        }

        protected internal override void Execute()
        {
            // Build master Website(s) (npm) includes for example Bootstrap
            BuildWebsite(); // Has to be before dotnet publish! It will copy site to publish/Framework/Application.Website/

            UtilCli.VersionBuild(() => {
                // Build Angular client (npm)
                BuildAngular();

                if (!(optionClientOnly?.Value() == "on" || IsOptionClientOnly))
                {
                    // Build .NET Core server (dotnet)
                    BuildServer();
                }
            });
        }
    }
}
