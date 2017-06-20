namespace Framework.Tool
{
    public class CommandOpen : Command
    {
        public CommandOpen()
            : base("open", "Open code in Visual Studio Code")
        {
            this.Client = OptionAdd("-c|--client", "Open Angular Client in Visual Studio Code");
            this.Server = OptionAdd("-s|--server", "Open Angular Server in Visual Studio Code");
            this.Solution = OptionAdd("-n|--solution", "Open Application.sln in Visual Studio");
            this.Universal = OptionAdd("-u|--universal", "Open Angular Universal in Visual Studio Code");
        }

        public readonly Option Client;

        public readonly Option Server;

        public readonly Option Solution;

        public readonly Option Universal;


        public override void Run()
        {
            if (Client.IsOn)
            {
                UtilTool.OpenVisualStudioCode(Util.FolderName + "Submodule/Client/");
            }
            if (Server.IsOn)
            {
                UtilTool.OpenVisualStudioCode(Util.FolderName + "Server/");
            }
            if (Solution.IsOn)
            {
                UtilTool.OpenBrowser(Util.FolderName + "Application.sln");
            }
            if (Universal.IsOn)
            {
                UtilTool.OpenVisualStudioCode(Framework.Util.FolderName + "Submodule/Universal/");
            }
        }
    }
}
