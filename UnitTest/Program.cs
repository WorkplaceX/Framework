using Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace UnitTest
{
    public class Program
    {
        public static void Main(string[] args)
        {
            new UnitTest.DataAccessLayer.UnitTest().Run();
            new UnitTest.Json.UnitTest().Run();
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
