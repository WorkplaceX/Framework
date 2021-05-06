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
        /// <param name="isOnly">If true, do not run integrate program.</param>
        public static bool Run(bool isFrameworkDb, AppCli appCli, bool isOnly)
        {
            bool isSuccessful = true;
            MetaSql metaSql = new MetaSql(isFrameworkDb);

            // Custom sql table and field filtering for code generation.
            var list = metaSql.List;
            var typeRowCalculatedList = new List<Type>(); // Calculated row.
            
            if (isFrameworkDb == false)
            {
                // Call method CommandGenerateFilter();
                Run(ref list, ref typeRowCalculatedList, appCli);
            }

            MetaCSharp metaCSharp = new MetaCSharp(list);

            // Generate CSharp classes from database schema and save (*.cs) files.
            UtilCliInternal.ConsoleWriteLineColor("Generate CSharp classes from database schema and write (*.cs) files", ConsoleColor.Green);
            new CSharpGenerate(metaCSharp).Run(isFrameworkDb, out string cSharp);
            if (isFrameworkDb == false)
            {
                UtilFramework.FileSave(UtilFramework.FolderName + "Application.Database/Database/Database.cs", cSharp);
            }
            else
            {
                UtilFramework.FileSave(UtilFramework.FolderName + "Framework/Framework/Database/Database.cs", cSharp);
            }
            UtilCliInternal.ConsoleWriteLineColor("Generate CSharp classes from database schema and write (*.cs) files succsesful!", ConsoleColor.Green);

            if (!isOnly)
            {
                // Read Integrate data from database and save (*.cs) files.
                UtilCliInternal.ConsoleWriteLineColor("Generate CSharp code for Integrate data and write to (*.cs) files", ConsoleColor.Green);
                GenerateIntegrateResult generateIntegrateResult = null;
                try
                {
                    // TableNameCSharp defined in method AppCli.CommandGenerateFilter();
                    List<string> tableNameCSharpApplicationFilterList = null;
                    if (isFrameworkDb == false)
                    {
                        tableNameCSharpApplicationFilterList = metaCSharp.List.GroupBy(item => item.SchemaNameCSharp + "." + item.TableNameCSharp).Select(item => item.Key).ToList();
                        var tableNameCSharpCalculatedList = typeRowCalculatedList.Select(item => UtilDalType.TypeRowToTableNameCSharp(item)).ToList();
                        tableNameCSharpApplicationFilterList.AddRange(tableNameCSharpCalculatedList);
                    }

                    generateIntegrateResult = appCli.CommandGenerateIntegrateInternal(isDeployDb: false, tableNameCSharpApplicationFilterList);
                }
                catch (SqlException exception)
                {
                    isSuccessful = false;
                    string message = string.Format("Error! Read Integrate data from database failed! This can happen after an sql schema change. Try to run generate script again! ({0})", exception.Message);
                    UtilCliInternal.ConsoleWriteLineColor(message, ConsoleColor.Red); // Error
                }
                if (generateIntegrateResult != null)
                {
                    Run(generateIntegrateResult);
                    UtilCliInternal.FolderDelete(UtilFramework.FolderName + "Application.Cli/Database/Blob/");
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
                    UtilCliInternal.ConsoleWriteLineColor("Generate CSharp code for Integrate data and write to (*.cs) files successful!", ConsoleColor.Green);
                }

            }
            return isSuccessful;
        }

        /// <summary>
        /// Call method CommandGenerateFilter();
        /// </summary>
        private static void Run(ref MetaSqlSchema[] list, ref List<Type> typeRowCalculatedList, AppCli appCli)
        {
            var args = new GenerateFilterArgs(list);
            var result = new GenerateFilterResult();
            
            // Args for calculated row
            var assemblyList = appCli.AssemblyList(isIncludeApp: true);
            List<Type> typeRowList = UtilDalType.TypeRowList(assemblyList);
            foreach (Type typeRow in typeRowList)
            {
                if (UtilDalType.TypeRowIsTableNameSql(typeRow) == false) // Calculated row
                {
                    args.TypeRowCalculatedList.Add(typeRow);
                }
            }
            
            // Call method CommandGenerateFilter();
            appCli.CommandGenerateFilter(args, result);
            
            // Result
            if (result.FieldSqlList != null)
            {
                list = result.List;
            }
            if (result.TypeRowCalculatedList == null)
            {
                typeRowCalculatedList = args.TypeRowCalculatedList;
            }
            else
            {
                typeRowCalculatedList = result.TypeRowCalculatedList;
            }
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
