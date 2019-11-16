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
            string folderName = UtilFramework.FolderName + "Framework/Framework.Angular/application/";
            UtilCli.Npm(folderName, "install"); // Angular install
            UtilCli.Npm(folderName, "run build:ssr"); // Build Server-side Rendering (SSR) to folder Framework/Framework.Angular/application/dist

            string folderNameSource = UtilFramework.FolderName + "Framework/Framework.Angular/application/dist/";
            string folderNameDest = UtilFramework.FolderName + "Application.Server/Framework/dist/";

            // Copy folder
            UtilCli.FolderDelete(folderNameDest);
            UtilFramework.Assert(!Directory.Exists(folderNameDest));
            UtilCli.FolderCopy(folderNameSource, folderNameDest, "*.*", true);
            UtilFramework.Assert(Directory.Exists(folderNameDest));

            // indexEmpty.html
            string fileName = folderNameDest + "browser/indexEmpty.html"; // See also Framework/Framework.Angular/application/server.ts
            File.WriteAllText(fileName, "<app-root></app-root>");
        }

        private static void BuildServer()
        {
            string folderName = UtilFramework.FolderName + "Application.Server/";
            string folderNamePublish = UtilFramework.FolderName + "Application.Server/bin/Debug/netcoreapp2.2/publish/";

            UtilCli.FolderNameDelete(folderNamePublish);
            UtilFramework.Assert(!Directory.Exists(folderNamePublish), "Delete folder failed!");
            UtilCli.DotNet(folderName, "publish"); // Use publish instead to build.
            UtilFramework.Assert(Directory.Exists(folderNamePublish), "Deploy failed!");

            string fileNameSource = UtilFramework.FolderName + "ConfigWebServer.json";
            string fileNameDest = folderNamePublish + "ConfigWebServer.json";
            UtilCli.FileCopy(fileNameSource, fileNameDest);
        }

        /// <summary>
        /// Execute "npm run build" command.
        /// </summary>
        private static void BuildWebsiteNpm(ConfigCliWebsite config)
        {
            string folderNameNpmBuild = UtilFramework.FolderNameParse(config.FolderNameNpmBuild);
            if (UtilFramework.StringNull(folderNameNpmBuild) != null)
            {
                string folderName = UtilFramework.FolderName + folderNameNpmBuild;
                UtilCli.Npm(folderName, "install --loglevel error --no-save"); // Prevent changin package-lock.json. See also:  https://github.com/npm/npm/issues/20934
                UtilCli.Npm(folderName, "run build");
            }
        }

        /// <summary>
        /// Build for example: "WebsiteDefault/"
        /// </summary>
        private static void BuildWebsite()
        {
            var configCli = ConfigCli.Load();
            foreach (var website in configCli.WebsiteList)
            {
                Console.WriteLine(string.Format("### Build Website (Begin) - {0}", website.DomainNameListToString()));
                BuildWebsiteNpm(website);

                string folderNameServer = UtilFramework.FolderNameParse(website.FolderNameServer);
                UtilFramework.Assert(folderNameServer != null, "FolderNameServer can not be null!");

                string folderNameDist = UtilFramework.FolderNameParse(website.FolderNameDist);
                UtilFramework.Assert(folderNameDist != null, "FolderNameDist can not be null!");

                string folderNameSource = UtilFramework.FolderName + folderNameDist;
                string folderNameDest = UtilFramework.FolderName + "Application.Server/Framework/Website/" + folderNameServer;
                if (!UtilCli.FolderNameExist(folderNameSource))
                {
                    throw new Exception(string.Format("Folder does not exist! ({0})", folderNameDest));
                }

                // Copy folder
                UtilCli.FolderDelete(folderNameDest);
                UtilFramework.Assert(!UtilCli.FolderNameExist(folderNameDest));
                UtilCli.FolderCopy(folderNameSource, folderNameDest, "*.*", true);
                UtilFramework.Assert(UtilCli.FolderNameExist(folderNameDest));

                Console.WriteLine(string.Format("### Build Website (End) - {0}", website.DomainNameListToString()));
            }
        }

        /// <summary>
        /// Copy from ConfigCli to ConfigWebServer.
        /// </summary>
        private static void BuildConfigWebServer()
        {
            Console.WriteLine("Copy runtime specific values from ConfigCli to ConfigWebServer"); // There is also other values not needed for runtime like DeployAzureGitUrl.
            var configCli = ConfigCli.Load();
            var configWebServer = ConfigWebServer.Load();

            // ConnectionString
            configWebServer.ConnectionStringFramework = configCli.ConnectionStringFramework;
            configWebServer.ConnectionStringApplication = configCli.ConnectionStringApplication;

            // Website
            configWebServer.WebsiteList.Clear();
            foreach (var webSite in configCli.WebsiteList)
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

            UtilCli.VersionBuild(() => {
                BuildAngular();

                // BuildClient
                if (!(optionClientOnly.Value() == "on"))
                {
                    BuildServer();
                }
            });

            // Build WebSite
            BuildWebsite();
        }
    }
}
