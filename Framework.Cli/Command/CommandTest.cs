namespace Framework.Cli.Command
{
    using System.Runtime.InteropServices;

    /// <summary>
    /// Cli start command.
    /// </summary>
    public class CommandTest : CommandBase
    {
        public CommandTest(AppCli appCli)
            : base(appCli, "test", "Run unit tests")
        {

        }

        protected internal override void Execute()
        {
            string folderName = UtilFramework.FolderName + "Framework/Framework.Test/";
            UtilCli.DotNet(folderName, "run");
        }
    }
}
