namespace Framework.Cli
{
    using DatabaseIntegrate.dbo;
    using System;

    class Program
    {
        static void Main(string[] args)
        {
            var appCli = new AppCli();
            appCli.Run(args);
        }
    }
}
