namespace Framework.BuildTool
{
    public class CommandGenerate : Command
    {
        public CommandGenerate(AppBuildTool appBuildTool) 
            : base("generate", "Generate CSharp DTO's")
        {
            this.AppBuildTool = appBuildTool;
            this.Framework = OptionAdd("-f|--framework", "For internal use only!");
        }

        public readonly AppBuildTool AppBuildTool;

        public readonly Option Framework;

        public override void Run()
        {
            if (Framework.IsOn == false)
            {
                DataAccessLayer.Script.Run(false, AppBuildTool);
                UtilFramework.Log(string.Format("File updated. ({0})", DataAccessLayer.ConnectionManager.DatabaseGenerateFileName));
            }
            else
            {
                DataAccessLayer.Script.Run(true, AppBuildTool);
                UtilFramework.Log(string.Format("File updated. ({0})", DataAccessLayer.ConnectionManager.DatabaseGenerateFrameworkFileName));
            }
        }
    }
}
