namespace Framework.BuildTool
{
    public class CommandUnitTest : Command
    {
        public CommandUnitTest() 
            : base("unitTest", "Run unit tests")
        {

        }

        public override void Run()
        {
            UtilBuildTool.DotNetRun(Framework.Util.FolderName + "Submodule/UnitTest/");
        }
    }
}
