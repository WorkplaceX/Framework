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

        private void BuildClient()
        {
            string folderName = UtilFramework.FolderName + "Framework/Client/";
            // UtilCli.Npm(folderName, "install --loglevel error");
            // UtilCli.Npm(folderName, "run build:ssr"); // Build Universal to folder Framework/Client/dist/ // For ci stderror see also package.json: "webpack:server --progress --colors (removed); ng build --output-hashing none --no-progress (added); ng run --no-progress (added)

            string folderNameSource = UtilFramework.FolderName + "Framework/Client/dist/";
            string folderNameDest = UtilFramework.FolderName + "Application.Server/Framework/dist/";

            UtilCli.FolderDelete(folderNameDest);
            UtilFramework.Assert(!Directory.Exists(folderNameDest));

            UtilCli.FolderCopy(folderNameSource, folderNameDest, "*.*", true);
            UtilFramework.Assert(Directory.Exists(folderNameDest));

            File.Delete(folderNameDest + "browser/3rdpartylicenses.txt"); // Prevent "dotnet : warning: LF will be replaced by CRLF" beeing written to stderr during deployment.
        } 

        private void BuildServer()
        {
            string folderName = UtilFramework.FolderName + "Application.Server/";
            string folderNamePublish = UtilFramework.FolderName + "Application.Server/bin/Debug/netcoreapp2.0/publish/";

            // UtilCli.DotNet(folderName, "build"); // Use publish instead to build.

            UtilCli.FolderNameDelete(folderNamePublish);
            UtilFramework.Assert(!Directory.Exists(folderNamePublish), "Delete folder failed!");
            UtilCli.DotNet(folderName, "publish");
            UtilFramework.Assert(Directory.Exists(folderNamePublish), "Deploy failed!");
        }

        protected internal override void Execute()
        {
            UtilCli.VersionBuild(() => {
                BuildClient();
                BuildServer();
            });
        }
    }
}
