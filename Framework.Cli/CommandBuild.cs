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

        private void BuildServer()
        {
            // Read UtilFramework.cs
            string fileName = UtilFramework.FolderName + "Framework/Framework/UtilFramework.cs";
            string text = File.ReadAllText(fileName);

            string find = "return \"Build (local)\"; // See also: method CommandBuild.BuildServer();";
            string replace = string.Format("return \"Build ({0} - {1})\";", DateTime.Now.ToUniversalTime().ToString("yyyy-MM-dd HH:mm"), System.Environment.MachineName);

            // Write UtilFramework.cs
            string textNew = UtilFramework.Replace(text, find, replace);
            File.WriteAllText(fileName, textNew);

            string folderName = UtilFramework.FolderName + "Application.Server/";
            UtilCli.DotNet(folderName, "build");

            File.WriteAllText(fileName, text); // Back to original text.
        }

        protected internal override void Execute()
        {
            BuildClient();
            BuildServer();
        }
    }
}
