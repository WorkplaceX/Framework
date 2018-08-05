using System;
using System.IO;

namespace Framework.Cli
{
    /// <summary>
    /// Cli build command.
    /// </summary>
    public class CommandBuild : CommandBase
    {
        public CommandBuild(AppCliBase appCli)
            : base(appCli, "build", "Build client and server")
        {

        }

        private static void BuildClient()
        {
            string folderName = UtilFramework.FolderName + "Framework/Client/";
            UtilCli.Npm(folderName, "install --loglevel error");
            UtilCli.Npm(folderName, "run build:ssr"); // Build Universal to folder Framework/Client/dist/ // For ci stderror see also package.json: "webpack:server --progress --colors (removed); ng build --output-hashing none --no-progress (added); ng run --no-progress (added)

            string folderNameSource = UtilFramework.FolderName + "Framework/Client/dist/";
            string folderNameDest = UtilFramework.FolderName + "Application.Server/Framework/dist/";

            UtilCli.FolderDelete(folderNameDest);
            UtilFramework.Assert(!Directory.Exists(folderNameDest));

            UtilCli.FolderCopy(folderNameSource, folderNameDest, "*.*", true);
            UtilFramework.Assert(Directory.Exists(folderNameDest));

            File.Delete(folderNameDest + "browser/3rdpartylicenses.txt"); // Prevent "dotnet : warning: LF will be replaced by CRLF" beeing written to stderr during deployment.
        } 

        private static void BuildServer()
        {
            string folderName = UtilFramework.FolderName + "Application.Server/";
            string folderNamePublish = UtilFramework.FolderName + "Application.Server/bin/Debug/netcoreapp2.0/publish/";

            // UtilCli.DotNet(folderName, "build"); // Use publish instead to build.

            UtilCli.FolderNameDelete(folderNamePublish);
            UtilFramework.Assert(!Directory.Exists(folderNamePublish), "Delete folder failed!");
            UtilCli.DotNet(folderName, "publish");
            UtilFramework.Assert(Directory.Exists(folderNamePublish), "Deploy failed!");

            string fileNameSource = UtilFramework.FolderName + "ConfigFramework.json";
            string fileNameDest = folderNamePublish + "ConfigFramework.json";
            UtilCli.FileCopy(fileNameSource, fileNameDest);
        }

        /// <summary>
        /// Copy from ConfigCli to ConfigFramework.
        /// </summary>
        private static void BuildWebsiteConfigFrameworkUpdate()
        {
            Console.WriteLine("Update ConfigFramework");
            var configCli = ConfigCli.Load();
            var configFramework = ConfigFramework.Load();

            configFramework.WebsiteList.Clear();

            foreach (var webSite in configCli.WebsiteList)
            {
                configFramework.WebsiteList.Add(new ConfigFrameworkWebsite() { DomainName = webSite.DomainName });
            }

            ConfigFramework.Save(configFramework);
        }

        private static void BuildWebsiteNpm(ConfigCliWebsite config)
        {
            if (config.FolderNameNpmBuild != null)
            {
                string folderName = UtilFramework.FolderName + config.FolderNameNpmBuild;
                UtilCli.Npm(folderName, "install --loglevel error");
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
                UtilCli.FolderDelete(folderNameDest);
                UtilCli.FolderCopy(folderNameSource, folderNameDest, "*.*", true);
                UtilFramework.Assert(UtilCli.FolderNameExist(folderNameDest));
                Console.WriteLine(string.Format("### Build Website (End) - {0}", website.DomainName));
            }
        }

        protected internal override void Execute()
        {
            // Init config
            ConfigCli.Init(AppCli);
            ConfigFramework.Init();
            BuildWebsiteConfigFrameworkUpdate();

            // Build
            UtilCli.VersionBuild(() => {
                BuildClient();
                BuildServer();
            });
            BuildWebsite();
        }
    }
}
