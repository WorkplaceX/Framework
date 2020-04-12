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
    public class CommandBuild : CommandBase
    {
        public CommandBuild(AppCli appCli)
            : base(appCli, "build", "Build Angular client and .NET server")
        {

        }

        private CommandOption optionClientOnly;

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

                // Copy CustomComponent
                string folderNameSource = UtilFramework.FolderName + "Application.Website/CustomComponent/";
                string folderNameDest = UtilFramework.FolderName + "Framework/Framework.Angular/application/src/Application.Website/CustomComponent/";
                UtilCli.FolderCopy(folderNameSource, folderNameDest, "*.*", true);

                // Create empty index.html file
                UtilCli.FileCreate(UtilFramework.FolderName + "Framework/Framework.Angular/application/src/Application.Website/Default/index.html");

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

                // Rename styles.css to frameworkStyle.css
                UtilCli.FileRename(UtilFramework.FolderName + "Application.Server/Framework/Framework.Angular/browser/styles.css", UtilFramework.FolderName + "Application.Server/Framework/Framework.Angular/browser/frameworkStyle.css");
            }
        }

        /// <summary>
        /// Copy ConfigWebServer.json to publish folder.
        /// </summary>
        internal static void ConfigWebServerPublish()
        {
            string folderNamePublish = UtilFramework.FolderName + "Application.Server/bin/Debug/netcoreapp3.1/publish/";

            string fileNameSource = UtilFramework.FolderName + "ConfigWebServer.json";
            string fileNameDest = folderNamePublish + "ConfigWebServer.json";
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

            ConfigWebServerPublish();
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
        /// Build all Websites. For example: "WebsiteDefault/"
        /// </summary>
        private static void BuildWebsite()
        {
            var configCli = ConfigCli.Load();
            foreach (var website in configCli.EnvironmentGet().WebsiteList)
            {
                Console.WriteLine(string.Format("### Build Website (Begin) - {0}", website.DomainNameListToString()));
                BuildWebsiteNpm(website);
                string folderNameServer = UtilFramework.FolderNameParse(website.FolderNameServer);
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

        /// <summary>
        /// Copy from file ConfigCli.json to ConfigWebServer.json
        /// </summary>
        private static void BuildConfigWebServer()
        {
            Console.WriteLine("Copy runtime specific values from ConfigCli to ConfigWebServer"); // There is also other values not needed for runtime like DeployAzureGitUrl.
            var configCli = ConfigCli.Load();
            var configWebServer = ConfigWebServer.Load();

            // ConnectionString
            configWebServer.ConnectionStringFramework = configCli.EnvironmentGet().ConnectionStringFramework;
            configWebServer.ConnectionStringApplication = configCli.EnvironmentGet().ConnectionStringApplication;

            // Website
            configWebServer.WebsiteList.Clear();
            foreach (var webSite in configCli.EnvironmentGet().WebsiteList)
            {
                configWebServer.WebsiteList.Add(new ConfigWebServerWebsite() {
                    FolderNameServer = webSite.FolderNameServer,
                    AppTypeName = webSite.AppTypeName,
                    DomainNameList = webSite.DomainNameList.Select(item => item).ToList() });
            }

            ConfigWebServer.Save(configWebServer);
        }

        internal static void InitConfigWebServer(AppCli appCli)
        {
            // Init config
            ConfigCli.Init(appCli);
            ConfigWebServer.Init();

            // Config
            BuildConfigWebServer();
        }

        protected internal override void Execute()
        {
            InitConfigWebServer(AppCli); // Copy ConnectionString from ConfigCli.json to ConfigWebServer.json.

            // Build Website(s) (npm) includes for example Bootstrap
            BuildWebsite(); // Has to be before dotnet publish! It will copy site to publish/Framework/Application.Website/

            UtilCli.VersionBuild(() => {
                // Build Angular client (npm)
                BuildAngular();

                if (!(optionClientOnly.Value() == "on"))
                {
                    // Build .NET Core server (dotnet)
                    BuildServer();
                }
            });
        }
    }
}
