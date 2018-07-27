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

            string folderNameSource = UtilFramework.FolderName + "Framework/Client/dist/browser/";
            string folderNameDest = UtilFramework.FolderName + "Application.Server/wwwroot/framework/";

            UtilCli.FolderCopy(folderNameSource, folderNameDest, "*.*", true);
        } 

        protected internal override void Execute()
        {
            BuildClient();
            string folderName = UtilFramework.FolderName + "Application.Server/";
            UtilCli.DotNet(folderName, "build");
        }
    }
}
