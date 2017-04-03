using Framework.Build;

namespace Build
{
    public class Program
    {
        public static void Main(string[] args)
        {
            do
            {
                Util.Log("");
                Util.Log("Build Command");
                Util.MethodExecute(new Script(args));
            }
            while (args.Length == 0); // Loop only if started without command line arguments.
        }
    }
}
