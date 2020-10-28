namespace Framework.Cli.Generate
{
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
            UtilCli.ConsoleWriteLineColor("Generate CSharp classes from database schema and write (*.cs) files", ConsoleColor.Green);
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

            // Read Integrate data from database and save (*.cs) files.
            UtilCli.ConsoleWriteLineColor("Generate CSharp code for Integrate data and write to (*.cs) files", ConsoleColor.Green);
            GenerateIntegrateResult generateIntegrateResult = null;
            try
            {
                // TableNameCSharp defined in method AppCli.CommandGenerateFilter();
                List<string> tableNameCSharpApplicationFilterList = null;
                if (isFrameworkDb == false)
                {
                    tableNameCSharpApplicationFilterList = metaCSharp.List.GroupBy(item => item.SchemaNameCSharp + "." + item.TableNameCSharp).Select(item => item.Key).ToList();
                }

                generateIntegrateResult = appCli.CommandGenerateIntegrateInternal(isDeployDb: false, tableNameCSharpApplicationFilterList);
            }
            catch (SqlException exception)
            {
                isSuccessful = false;
                string message = string.Format("Read Integrate data from database failed! This can happen after an sql schema change. Try to run generate script again! ({0})", exception.Message);
                UtilCli.ConsoleWriteLineColor(message, ConsoleColor.Red);
            }
            if (generateIntegrateResult != null)
            {
                Run(generateIntegrateResult);
                new GenerateCSharpIntegrate().Run(out string cSharpCli, isFrameworkDb, isApplication: false, integrateList: generateIntegrateResult.Result);
                new GenerateCSharpIntegrate().Run(out string cSharpApplication, isFrameworkDb, isApplication: true, integrateList: generateIntegrateResult.Result);
                if (isFrameworkDb == false)
                {
                    UtilFramework.FileSave(UtilFramework.FolderName + "Application.Cli/Database/DatabaseIntegrate.cs", cSharpCli);
                    UtilFramework.FileSave(UtilFramework.FolderName + "Application.Database/Database/DatabaseIntegrate.cs", cSharpApplication);
                }
                else
                {
                    UtilFramework.FileSave(UtilFramework.FolderName + "Framework/Framework.Cli/Database/DatabaseIntegrate.cs", cSharpCli);
                    UtilFramework.FileSave(UtilFramework.FolderName + "Framework/Framework/Database/DatabaseIntegrate.cs", cSharpApplication);
                }
                UtilCli.ConsoleWriteLineColor("Generate CSharp code for Integrate data and write to (*.cs) files successful!", ConsoleColor.Green);
            }

            return isSuccessful;
        }

        /// <summary>
        /// Console log Integrate table relation.
        /// </summary>
        private static void Run(GenerateIntegrateResult generateIntegrateResult)
        {
            bool isFirst = true;
            List<string> result = new List<string>();
            foreach (var item in generateIntegrateResult.Result)
            {
                var fieldList = UtilDalUpsertIntegrate.FieldIntegrateList(item.TypeRow, generateIntegrateResult.ResultReference);
                foreach (var field in fieldList)
                {
                    if (field.IsId)
                    {
                        UtilDalType.TypeRowToTableNameSql(item.TypeRow, out string schemaNameSql, out string tableNameSql);
                        UtilDalType.TypeRowToTableNameSql(field.TypeRowReference, out string schemaNameSqlReference, out string tableNameSqlReference);

                        if (isFirst)
                        {
                            isFirst = false;
                            Console.WriteLine("Integrate Table Relation");
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
