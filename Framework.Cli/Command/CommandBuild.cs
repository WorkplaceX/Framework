namespace Framework.Cli.Command
{
    using Framework.Cli.Config;
    using Microsoft.Extensions.CommandLineUtils;
    using System;
    using System.Collections.Generic;
    using System.IO;

    /// <summary>
    /// Cli build command.
    /// </summary>
    internal class CommandBuild : CommandBase
    {
        public CommandBuild(AppCli appCli)
            : base(appCli, "build", "Build Angular client and ASP.NET Core server")
        {

        }

        /// <summary>
        /// Build Angular client only.
        /// </summary>
        internal CommandOption OptionClientOnly;

        /// <summary>
        /// Build only first Angular client from WebsiteList.
        /// </summary>
        internal CommandOption OptionClientOnlyFirst; // Exclude further ExternalGit

        internal CommandOption OptionServerOnly;

        protected internal override void Register(CommandLineApplication configuration)
        {
            OptionClientOnly = configuration.Option("-c|--client", "Build Angular website only.", CommandOptionType.NoValue);
            OptionServerOnly = configuration.Option("-s|--server", "Build .NET server only.", CommandOptionType.NoValue);
            if (Directory.Exists(UtilFramework.FolderName + "ExternalGit"))
            {
                OptionClientOnlyFirst = configuration.Option("-f|--first", "Build Angular website first only.", CommandOptionType.NoValue);
            }
        }

        /// <summary>
        /// Copy ConfigServer.json to publish folder.
        /// </summary>
        internal static void ConfigServerPublish()
        {
            string folderNamePublish = UtilFramework.FolderName + "Application.Server/bin/Debug/net5.0/publish/";

            string fileNameSource = UtilFramework.FolderName + "ConfigServer.json";
            string fileNameDest = folderNamePublish + "ConfigServer.json";
            UtilCliInternal.FileCopy(fileNameSource, fileNameDest);
        }

        private static void BuildServer()
        {
            string folderName = UtilFramework.FolderName + "Application.Server/";
            string folderNamePublish = UtilFramework.FolderName + "Application.Server/bin/Debug/net5.0/publish/";

            UtilCliInternal.FolderNameDelete(folderNamePublish);
            UtilFramework.Assert(!Directory.Exists(folderNamePublish), "Delete folder failed!");
            UtilCliInternal.DotNet(folderName, "publish"); // Use publish instead to build.
            UtilFramework.Assert(Directory.Exists(folderNamePublish), "Deploy failed!");

            ConfigServerPublish();
        }

        /// <summary>
        /// Execute npm build command.
        /// </summary>
        private static void BuildWebsiteAngular(ConfigCliWebsite website)
        {
            string folderNameAngular = UtilFramework.StringNull(UtilFramework.FolderNameParse(website.FolderNameAngular));
            if (folderNameAngular != null)
            {
                string folderName = UtilFramework.FolderName + folderNameAngular;

                UtilCliInternal.Npm(folderName, "install --loglevel error"); // --loglevel error prevent writing to STDERR "npm WARN optional SKIPPING OPTIONAL DEPENDENCY"
                UtilCliInternal.Npm(folderName, "run build:ssr", isRedirectStdErr: true); // Build Server-side Rendering (SSR) to folder Framework/Application.Website/Website01/ // TODO Bug report Angular build writes to stderr. Repo steps: Delete node_modules and run npm install and then run build:ssr.
            }
        }

        /// <summary>
        /// Build all Angular Websites. For example: "Application.Website/"
        /// </summary>
        private void BuildWebsiteAngular()
        {
            var configCli = ConfigCli.Load();

            string folderNameServer = UtilFramework.FolderName + "Application.Server/Framework/Application.Website/";
            UtilCliInternal.FolderDelete(folderNameServer);

            var folderNameAngularList = new List<string>();
            foreach (var website in configCli.WebsiteList)
            {
                var folderNameAngular = UtilFramework.FolderNameParse(website.FolderNameAngular);
                if (folderNameAngular != null && !folderNameAngularList.Contains(folderNameAngular.ToLower()))
                {
                    folderNameAngularList.Add(folderNameAngular.ToLower());
                    Console.WriteLine(string.Format("### Build Website (Begin) - {0}", website.DomainNameListToString()));

                    // Delete dist folder
                    string folderNameDist = UtilFramework.FolderName + folderNameAngular + "dist/";
                    UtilCliInternal.FolderDelete(folderNameDist);

                    // npm build
                    BuildWebsiteAngular(website);

                    // Copy to server
                    string folderNameDest = folderNameServer + website.FolderNameAngularWebsite;
                    UtilCliInternal.FolderCreate(folderNameDest);
                    UtilCliInternal.FolderCopy(folderNameDist + "application/", folderNameDest);

                    Console.WriteLine(string.Format("### Build Website (End) - {0}", website.DomainNameListToString()));
                }

                if (OptionClientOnlyFirst.OptionGet())
                {
                    break;
                }
            }
        }

        /// <summary>
        /// Clone external git repo and call prebuild script.
        /// </summary>
        private static void ExternalGit()
        {
            var configCli = ConfigCli.Load();

            foreach (var external in configCli.ExternalGitList)
            {
                // Clone repo
                var externalGit = UtilFramework.StringNull(external.ExternalGit);
                if (externalGit != null)
                {
                    string externalFolderName = UtilFramework.FolderName + "ExternalGit/" + external.ExternalProjectName + "/";
                    if (!UtilCliInternal.FolderNameExist(externalFolderName))
                    {
                        Console.WriteLine("Git Clone ExternalGit");
                        UtilCliInternal.FolderCreate(externalFolderName);
                        externalGit += " ."; // See also: https://stackoverflow.com/questions/6224626/clone-contents-of-a-github-repository-without-the-folder-itself
                        UtilCliInternal.Start(externalFolderName, "git", "clone --recursive -q" + " " + externalGit); // --recursive clone also submodule Framework -q do not write to stderr on linux

                        UtilFramework.Assert(UtilCliInternal.FolderNameExist(externalFolderName), string.Format("Expected folder does not exist after git clone ({0}])!", externalFolderName));
                    }
                }
            }
        }

        /// <summary>
        /// Run method AppCli.CommandExternalGit(); on ExternalGit/ProjectName/
        /// </summary>
        private static void CommandExternal()
        {
            var configCli = ConfigCli.Load();

            foreach (var external in configCli.ExternalGitList)
            {
                // External git url
                var externalGit = UtilFramework.StringNull(external.ExternalGit);

                // Call command cli external (prebuild script)
                var externalProjectName = UtilFramework.StringNull(external.ExternalProjectName);

                if (externalGit != null && externalProjectName != null)
                {
                    string folderName = UtilFramework.FolderName + "ExternalGit/" + externalProjectName + "/" + "Application.Cli";
                    UtilCliInternal.DotNet(folderName, "run -- externalGit");
                }
            }
        }

        protected internal override void Execute()
        {
            // Clone external repo
            ExternalGit();

            // Run cli external command. Override for example custom components.
            if (!OptionClientOnlyFirst.OptionGet())
            {
                CommandExternal();
            }

            // Build Angular Website(s)
            if (OptionServerOnly.OptionGet() == false)
            {
                BuildWebsiteAngular();
            }

            // Version tag and build Build Angular and .NET Core server.
            UtilCliInternal.VersionBuild(() => {
                if (OptionClientOnly.OptionGet() == false)
                {
                    // Build .NET Core server (dotnet)
                    BuildServer();
                }
            });
        }
    }
}
