using Framework;
using System;
using System.Diagnostics;

namespace UnitTest
{
    public class Program
    {
        public static void Main(string[] args)
        {
            new Application.UnitTest().Run();
            new DataAccessLayer.UnitTest().Run();
            new Json.UnitTest().Run();
            UtilFramework.Log("");
            UtilFramework.Log("All test successful!");
            if (Debugger.IsAttached)
            {
                Console.WriteLine("Press Enter...");
                Console.ReadLine();
            }
        }
    }
}
