namespace Framework.BuildTool
{
    public class CommandGenerate : Command
    {
        public CommandGenerate() 
            : base("generate", "Generate CSharp DTO's")
        {

        }

        public override void Run()
        {
            Util.Log(string.Format("File updated. ({0})", Build.DataAccessLayer.ConnectionManager.DatabaseLockFileName));
        }
    }
}
