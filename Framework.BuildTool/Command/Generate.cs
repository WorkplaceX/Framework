namespace Framework.BuildTool
{
    public class CommandGenerate : Command
    {
        public CommandGenerate() 
            : base("generate", "Generate CSharp DTO's")
        {
            this.Framework = OptionAdd("-f|--framework", "For internal use only!");

        }

        public readonly Option Framework;


        public override void Run()
        {
            if (Framework.IsOn == false)
            {
                DataAccessLayer.Script.Run(false);
                UtilFramework.Log(string.Format("File updated. ({0})", DataAccessLayer.ConnectionManager.DatabaseGenerateFileName));
            }
            else
            {
                DataAccessLayer.Script.Run(true);
                UtilFramework.Log(string.Format("File updated. ({0})", DataAccessLayer.ConnectionManager.DatabaseGenerateFrameworkFileName));
            }
        }
    }
}
