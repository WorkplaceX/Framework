namespace Framework.Cli.Command
{
    using Framework.Cli.Config;
    using Framework.Config;
    using Microsoft.Extensions.CommandLineUtils;
    using System;
    using System.IO;

    /// <summary>
    /// Cli build command.
    /// </summary>
    public class CommandBuild : CommandBase
    {
        public CommandBuild(AppCli appCli)
            : base(appCli, "build", "Build client and server")
        {

        }

        private CommandOption optionClientOnly;

        protected internal override void Register(CommandLineApplication configuration)
        {
            optionClientOnly = configuration.Option("-c|--client", "Build angular client only.", CommandOptionType.NoValue);
        }

        private static void BuildClient()
        {
            string folderName = UtilFramework.FolderName + "Framework/Client/";
            UtilCli.Npm(folderName, "install --loglevel error --no-save"); // Prevent changin package-lock.json. See also:  https://github.com/npm/npm/issues/20934
            UtilCli.Npm(folderName, "run build:ssr"); // Build Universal to folder Framework/Client/dist/ // For ci stderror see also package.json: "webpack:server --progress --colors (removed); ng build --output-hashing none --no-progress (added); ng run --no-progress (added)

            string folderNameSource = UtilFramework.FolderName + "Framework/Client/dist/";
            string folderNameDest = UtilFramework.FolderName + "Application.Server/Framework/dist/";

            // Copy folder
            UtilCli.FolderDelete(folderNameDest);
            UtilFramework.Assert(!Directory.Exists(folderNameDest));
            UtilCli.FolderCopy(folderNameSource, folderNameDest, "*.*", true);
            UtilFramework.Assert(Directory.Exists(folderNameDest));

            // Copy styles.css to frameworkStyle.css
            UtilCli.FileCopy(folderNameDest + "browser/styles.css", folderNameDest + "browser/frameworkStyle.css"); // Output file name styles.css can not be changed in angular.json!

            // indexEmpty.html
            string fileName = folderNameDest + "browser/indexEmpty.html";
            File.WriteAllText(fileName, "<data-app></data-app>");
        } 

        private static void BuildServer()
        {
            string folderName = UtilFramework.FolderName + "Application.Server/";
            string folderNamePublish = UtilFramework.FolderName + "Application.Server/bin/Debug/netcoreapp2.1/publish/";

            UtilCli.FolderNameDelete(folderNamePublish);
            UtilFramework.Assert(!Directory.Exists(folderNamePublish), "Delete folder failed!");
            UtilCli.DotNet(folderName, "publish"); // Use publish instead to build.
            UtilFramework.Assert(Directory.Exists(folderNamePublish), "Deploy failed!");

            string fileNameSource = UtilFramework.FolderName + "ConfigWebServer.json";
            string fileNameDest = folderNamePublish + "ConfigWebServer.json";
            UtilCli.FileCopy(fileNameSource, fileNameDest);
        }

        private static void BuildWebsiteNpm(ConfigCliWebsite config)
        {
            if (config.FolderNameNpmBuild != null)
            {
                string folderName = UtilFramework.FolderName + config.FolderNameNpmBuild;
                UtilCli.Npm(folderName, "install --loglevel error --no-save"); // Prevent changin package-lock.json. See also:  https://github.com/npm/npm/issues/20934
                UtilCli.Npm(folderName, "run build");
            }
        }

        private static void BuildWebsite()
        {
            var configCli = ConfigCli.Load();
            foreach (var website in configCli.WebsiteList)
            {
                Console.WriteLine(string.Format("### Build Website (Begin) - {0}", website.DomainName));
                BuildWebsiteNpm(website);

                string folderNameSource = UtilFramework.FolderName + website.FolderNameDist;
                string folderNameDest = UtilFramework.FolderName + "Application.Server/Framework/Website/" + website.DomainName + "/";
                if (!UtilCli.FolderNameExist(folderNameSource))
                {
                    throw new Exception(string.Format("Folder does not exist! ({0})", folderNameDest));
                }

                // Copy folder
                UtilCli.FolderDelete(folderNameDest);
                UtilFramework.Assert(!UtilCli.FolderNameExist(folderNameDest));
                UtilCli.FolderCopy(folderNameSource, folderNameDest, "*.*", true);
                UtilFramework.Assert(UtilCli.FolderNameExist(folderNameDest));

                Console.WriteLine(string.Format("### Build Website (End) - {0}", website.DomainName));
            }
        }

        /// <summary>
        /// Copy from ConfigCli to ConfigWebServer.
        /// </summary>
        private static void BuildConfigWebServer()
        {
            Console.WriteLine("Copy values from ConfigCli to ConfigWebServer");
            var configCli = ConfigCli.Load();
            var configWebServer = ConfigWebServer.Load();

            // ConnectionString
            configWebServer.ConnectionStringFramework = configCli.ConnectionStringFramework;
            configWebServer.ConnectionStringApplication = configCli.ConnectionStringApplication;

            // Website
            configWebServer.WebsiteList.Clear();
            foreach (var webSite in configCli.WebsiteList)
            {
                configWebServer.WebsiteList.Add(new ConfigWebServerWebsite() { DomainName = webSite.DomainName });
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
            InitConfigWebServer(AppCli);

            // Build
            BuildWebsite();
            UtilCli.VersionBuild(() => {
                BuildClient();
                if (!(optionClientOnly.Value() == "on"))
                {
                    BuildServer();
                }
            });
        }
    }
}
