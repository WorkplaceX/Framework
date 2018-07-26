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
            UtilCli.Start(folderName, "npm", "install");
            UtilCli.Start(folderName, "npm", "run build:ssr"); // Build Universal to folder Framework/Client/dist/
        }

        protected internal override void Execute()
        {
            BuildClient();
            string folderName = UtilFramework.FolderName + "Application.Server/";
            UtilCli.DotNet(folderName, "build");
        }
    }
}
