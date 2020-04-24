using System;

namespace Framework.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            UnitTest.Run();

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Unit tests successful!");
            Console.ResetColor();
        }
    }
}
