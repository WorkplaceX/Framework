using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Office
{
    class Program
    {
        static void Main(string[] args)
        {
            // args = new string[] { "SqlCreate", "Data Source=(LocalDB)\\MSSQLLocalDB; Initial Catalog=Database; Integrated Security=True" };
            // args = new string[] { "Run", "Data Source=(LocalDB)\\MSSQLLocalDB; Initial Catalog=Database; Integrated Security=True", @"C:\Temp\GitHb2\Research\Framework\Build\Airport\" };
            if (args.Length == 0)
            {
                Console.WriteLine("Office load Excel files into database");
                Console.WriteLine("Office.exe Run ConnectionString FolderName");
                Console.WriteLine("Office.exe SqlCreate ConnectionString");
                Console.WriteLine("Office.exe SqlDrop ConnectionString");
            }
            else
            {
                string command = args[0];
                ConnectionManager.Prefix = "Import";
                if (command == "Run")
                {
                    string connectionString = args[1];
                    string folderName = args[2];
                    ConnectionManager.ConnectionStringList["Default"] = connectionString;
                    ConnectionManager.ConnectionKey = "Default";
                    ConnectionManager.ExcelLocalFolder = folderName;
                    Script.Run();
                }
                if (command == "SqlCreate")
                {
                    string connectionString = args[1];
                    ConnectionManager.ConnectionStringList["Default"] = connectionString;
                    ConnectionManager.ConnectionKey = "Default";
                    Script.SqlCreate();
                }
                if (command == "SqlDrop")
                {
                    string connectionString = args[1];
                    ConnectionManager.ConnectionStringList["Default"] = connectionString;
                    ConnectionManager.ConnectionKey = "Default";
                    Script.SqlDrop();
                }
            }
        }
    }
}
