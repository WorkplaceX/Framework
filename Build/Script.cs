namespace Build
{
    public class Script : Framework.Build.ScriptBase
    {
        public Script(string[] args) 
            : base(args)
        {

        }

        public override void RunSql()
        {
            Airport.Script.Run();
            base.RunSql();
        }
    }
}