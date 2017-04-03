using System;
using System.Collections.Generic;
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
            Console.WriteLine();
            Console.WriteLine("All test successful!");
            // Console.ReadLine();
        }
    }
}
