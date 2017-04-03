using Framework.Build;

namespace Build.Airport
{
    public class Script
    {
        /// <summary>
        /// Load Airport.xlsx into database.
        /// </summary>
        public static void Run()
        {
            string connectionString = Framework.Server.ConnectionManager.ConnectionString;
            string fileName = Framework.Util.FolderName + "Submodule/Office/bin/Debug/Office.exe";
            // SqlDrop
            {
                string command = "SqlDrop";
                string arguments = command + " " + "\"" + connectionString + "\"";
                Util.Start(Framework.Util.FolderName, fileName, arguments);
            }
            // SqlCreate
            {
                string command = "SqlCreate";
                string arguments = command + " " + "\"" + connectionString + "\"";
                Util.Start(Framework.Util.FolderName, fileName, arguments);
            }
            // Run
            {
                string command = "Run";
                string folderName = Framework.Util.FolderName + "Submodule/Build/Airport/";
                string arguments = command + " " + "\"" + connectionString + "\"" + " " + "\"" + folderName + "\"";
                Util.Start(Framework.Util.FolderName, fileName, arguments);
            }
        }
    }
}
