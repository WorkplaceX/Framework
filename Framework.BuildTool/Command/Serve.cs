using Framework.Server;

namespace Framework.BuildTool
{
    public class CommandServe : Command
    {
        public CommandServe()
            : base("serve", "Serve .NET web page. Wait ca. 30 seconds.")
        {
            this.Client = OptionAdd("-c|--clientLiveDevelopment", "Start npm client live development server only.");
        }

        public readonly Option Client;

        public override void Run()
        {
            if (Client.IsOn)
            {
                UtilBuildTool.NpmRun(UtilFramework.FolderName + "Submodule/Client/", "start", isWait: false);
                UtilBuildTool.OpenBrowser("http://localhost:4200"); // Client live development.
            }
            else
            {
                UtilBuildTool.DotNetRun(UtilFramework.FolderName + "Server/", false);
                UtilServer.StartUniversalServer();
                UtilBuildTool.OpenBrowser("http://localhost:49324"); // See also: Server/Properties/launchSettings.json
            }
        }
    }
}
