namespace Framework.BuildTool
{
    public class CommandCheck : Command
    {
        public CommandCheck() 
            : base("check", "Test all ConnectionManager values (Server and BuildTool)")
        {

        }

        public override void Run()
        {
            ConnectionManagerCheck.Run();
        }
    }
}
