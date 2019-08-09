namespace Framework.Cli.Generate
{
    using Framework.Cli.Command;
    using System;
    using System.Collections.Generic;
    using System.Data.SqlClient;
    using static Framework.Cli.Command.AppCli;

    /// <summary>
    /// Generate CSharp code for database tables.
    /// </summary>
    public static class Script
    {
        /// <summary>
        /// Script to generate CSharp code. Returns true, if succsesful.
        /// </summary>
        /// <param name="isFrameworkDb">If true, generate CSharp code for Framework library (internal use only) otherwise generate code for Application.</param>
        public static bool Run(bool isFrameworkDb, AppCli appCli)
        {
            bool isSuccessful = true;

            MetaSql metaSql = new MetaSql(isFrameworkDb, appCli);
            MetaCSharp metaCSharp = new MetaCSharp(metaSql);

            // Generate CSharp classes from database schema and save (*.cs) files.
            new CSharpGenerate(metaCSharp).Run(isFrameworkDb, out string cSharp);
            if (isFrameworkDb == false)
            {
                UtilFramework.FileSave(UtilFramework.FolderName + "Application.Database/Database/Database.cs", cSharp);
            }
            else
            {
                UtilFramework.FileSave(UtilFramework.FolderName + "Framework/Framework/Database/Database.cs", cSharp);
            }
            UtilCli.ConsoleWriteLineColor("Generate CSharp classes from database schema and write (*.cs) files succsesful!", ConsoleColor.Green);

            // Read BuiltIn data from database and save (*.cs) files.
            List<GenerateBuiltInItem> builtInlist = null;
            try
            {
                builtInlist = appCli.CommandGenerateBuiltInListInternal();
            }
            catch (SqlException exception)
            {
                isSuccessful = false;
                string message = string.Format("Read BuiltIn data from database failed! This can happen after an sql schema change. Try to run generate script again! ({0})", exception.Message);
                UtilCli.ConsoleWriteLineColor(message, ConsoleColor.Red);
            }
            if (builtInlist != null)
            {
                new GenerateCSharpBuiltIn().Run(out string cSharpCli, isFrameworkDb, isApplication: false, builtInList: builtInlist);
                new GenerateCSharpBuiltIn().Run(out string cSharpApplication, isFrameworkDb, isApplication: true, builtInList: builtInlist);
                if (isFrameworkDb == false)
                {
                    UtilFramework.FileSave(UtilFramework.FolderName + "Application.Cli/Database/DatabaseBuiltIn.cs", cSharpCli);
                    UtilFramework.FileSave(UtilFramework.FolderName + "Application.Database/Database/DatabaseBuiltIn.cs", cSharpApplication);
                }
                else
                {
                    UtilFramework.FileSave(UtilFramework.FolderName + "Framework/Framework.Cli/Database/DatabaseBuiltIn.cs", cSharpCli);
                    UtilFramework.FileSave(UtilFramework.FolderName + "Framework/Framework/Database/DatabaseBuiltIn.cs", cSharpApplication);
                }
                UtilCli.ConsoleWriteLineColor("Generate CSharp code for BuiltIn data and write to (*.cs) files successful!", ConsoleColor.Green);
            }

            return isSuccessful;
        }
    }
}
