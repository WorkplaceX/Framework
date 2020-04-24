namespace Framework.Cli.Generate
{
    using Framework.Cli.Command;
    using System;
    using System.Collections.Generic;
    using Microsoft.Data.SqlClient;
    using static Framework.Cli.AppCli;
    using Framework.DataAccessLayer;
    using System.Linq;

    /// <summary>
    /// Generate CSharp code for database tables.
    /// </summary>
    internal static class Script
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
            GenerateBuiltInResult generateBuiltInResult = null;
            try
            {
                generateBuiltInResult = appCli.CommandGenerateBuiltInListInternal(); // TODO cli command generate is not BuiltIn table reference aware. See also TableNameSqlReferencePrefix. Therefore Id columns can not be omitted in generate. See also class GenerateBuiltInItem and DeployDbBuiltInItem.
            }
            catch (SqlException exception)
            {
                isSuccessful = false;
                string message = string.Format("Read BuiltIn data from database failed! This can happen after an sql schema change. Try to run generate script again! ({0})", exception.Message);
                UtilCli.ConsoleWriteLineColor(message, ConsoleColor.Red);
            }
            if (generateBuiltInResult != null)
            {
                Run(generateBuiltInResult, appCli);
                new GenerateCSharpBuiltIn().Run(out string cSharpCli, isFrameworkDb, isApplication: false, builtInList: generateBuiltInResult.Result);
                new GenerateCSharpBuiltIn().Run(out string cSharpApplication, isFrameworkDb, isApplication: true, builtInList: generateBuiltInResult.Result);
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

        /// <summary>
        /// Console log table relation.
        /// </summary>
        private static void Run(GenerateBuiltInResult generateBuiltInResult, AppCli appCli)
        {
            bool isFirst = true;
            List<string> result = new List<string>();
            foreach (var item in generateBuiltInResult.Result)
            {
                var fieldList = UtilDalUpsertBuiltIn.FieldBuiltInList(item.TypeRow, item.TableNameSqlReferencePrefix, appCli.AssemblyList(true, true));
                foreach (var field in fieldList)
                {
                    if (field.IsId)
                    {
                        UtilDalType.TypeRowToTableNameSql(item.TypeRow, out string schemaNameSql, out string tableNameSql);
                        UtilDalType.TypeRowToTableNameSql(field.TypeRowReference, out string schemaNameSqlReference, out string tableNameSqlReference);

                        if (isFirst)
                        {
                            isFirst = false;
                            Console.WriteLine("BuiltIn Table Relation");
                        }
                        result.Add(string.Format("[{0}].[{1}].[{2}] --> [{3}].[{4}]", schemaNameSql, tableNameSql, field.FieldNameIdSql, schemaNameSqlReference, tableNameSqlReference));
                    }
                }
            }

            foreach (var item in result.Distinct().OrderBy(item => item))
            {
                Console.WriteLine(item);
            }
        }
    }
}
