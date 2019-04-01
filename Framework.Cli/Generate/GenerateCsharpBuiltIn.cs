namespace Framework.Cli.Generate
{
    using Framework.Cli.Config;
    using Framework.DataAccessLayer;
    using System;
    using System.Collections.Generic;
    using System.Data.SqlClient;
    using System.Linq;
    using System.Text;

    public static class NamingConventionBuiltIn
    {
        public static bool TableNameIsBuiltIn(string tableNameCSharp)
        {
            return tableNameCSharp.EndsWith("BuiltIn");
        }
    }

    public class GenerateCSharpBuiltIn
    {
        public GenerateCSharpBuiltIn(MetaCSharp metaCSharp)
        {
            this.MetaCSharp = metaCSharp;
        }

        public readonly MetaCSharp MetaCSharp;

        /// <summary>
        /// Generate CSharp namespace for every database schema.
        /// </summary>
        private static void GenerateCSharpSchemaName(MetaCSharp metaCSharp, bool isFrameworkDb, bool isApplication, StringBuilder result)
        {
            var schemaNameList = metaCSharp.List.GroupBy(item => new { SchemaNameSql = item.Schema.SchemaName, item.SchemaNameCSharp }, (key, group) => key).ToArray();
            bool isFirst = true;
            foreach (var item in schemaNameList)
            {
                if (isFirst)
                {
                    isFirst = false;
                }
                else
                {
                    result.AppendLine();
                }
                if (GenerateCSharpTableNameClass(metaCSharp, item.SchemaNameSql, isFrameworkDb, isApplication, null) > 0) // Generate CSharp schema only if it contains tables.
                {
                    result.AppendLine(string.Format("namespace DatabaseBuiltIn.{0}", item.SchemaNameCSharp));
                    result.AppendLine(string.Format("{{"));
                    result.AppendLine(string.Format("    using System.Collections.Generic;"));
                    result.AppendLine(string.Format("    using Database.{0};", item.SchemaNameCSharp));
                    result.AppendLine();
                    GenerateCSharpTableNameClass(metaCSharp, item.SchemaNameSql, isFrameworkDb, isApplication, result);
                    result.AppendLine(string.Format("}}"));
                }
            }
        }

        /// <summary>
        /// Generate static CSharp class for every database table.
        /// </summary>
        private static int GenerateCSharpTableNameClass(MetaCSharp metaCSharp, string schemaNameSql, bool isFrameworkDb, bool isApplication, StringBuilder result)
        {
            var tableNameList = metaCSharp.List.Where(item => item.Schema.SchemaName == schemaNameSql).GroupBy(item => new { item.Schema.SchemaName, item.Schema.TableName, item.TableNameCSharp }, (key, group) => key).ToArray();
            tableNameList = tableNameList.Where(item => NamingConventionBuiltIn.TableNameIsBuiltIn(item.TableNameCSharp)).ToArray();
            if (result != null)
            {
                List<string> nameExceptList = new List<string>();
                bool isFirst = true;
                foreach (var item in tableNameList)
                {
                    if (isFirst)
                    {
                        isFirst = false;
                    }
                    else
                    {
                        result.AppendLine();
                    }
                    string classNameExtension = "Cli";
                    if (isApplication)
                    {
                        classNameExtension = "Application";
                    }
                    result.AppendLine(string.Format("    public static class {0}{1}", item.TableNameCSharp, classNameExtension));
                    result.AppendLine(string.Format("    {{"));
                    result.AppendLine(string.Format("        public static List<{0}> List", item.TableNameCSharp));
                    result.AppendLine(string.Format("        {{"));
                    result.AppendLine(string.Format("            get"));
                    result.AppendLine(string.Format("            {{"));
                    result.AppendLine(string.Format("                var result = new List<{0}>();", item.TableNameCSharp));
                    GenerateCSharpRowBuiltIn(metaCSharp, item.SchemaName, item.TableNameCSharp, isFrameworkDb, isApplication, result);
                    result.AppendLine(string.Format("                return result;"));
                    result.AppendLine(string.Format("            }}"));
                    result.AppendLine(string.Format("        }}"));
                    result.AppendLine(string.Format("    }}"));
                }
            }
            return tableNameList.Length;
        }

        private static void GenerateCSharpRowBuiltIn(MetaCSharp metaCSharp, string schemaNameSql, string tableNameCSharp, bool isFrameworkDb, bool isApplication, StringBuilder result)
        {
            var fieldNameList = metaCSharp.List.Where(item => item.Schema.SchemaName == schemaNameSql && item.Schema.TableName == tableNameCSharp).ToList();
            string tableNameSql = fieldNameList.First().Schema.TableName;
            string connectionString = ConfigCli.ConnectionString(isFrameworkDb);

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string tableNameWithSchemaSql = UtilDalType.TableNameWithSchemaSql(schemaNameSql, tableNameSql);
                using (var sqlCommand = new SqlCommand(string.Format("SELECT * FROM {0}", tableNameWithSchemaSql), connection))
                {
                    using (var reader = sqlCommand.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            result.Append(string.Format("                result.Add(new {0}()", tableNameCSharp));
                            bool isFirst = true;
                            for (int fieldIndex = 0; fieldIndex < reader.FieldCount; fieldIndex++)
                            {
                                string fieldNameSql = reader.GetName(fieldIndex);
                                MetaCSharpSchema field = fieldNameList.Where(item => item.Schema.FieldName == fieldNameSql).SingleOrDefault();
                                if (field != null)
                                {
                                    if (isFirst)
                                    {
                                        result.Append(" { ");
                                        isFirst = false;
                                    }
                                    else
                                    {
                                        result.Append(", ");
                                    }
                                    object value = reader.GetValue(fieldIndex);
                                    if (value == DBNull.Value)
                                    {
                                        value = null;
                                    }
                                    GenerateCSharpRowBuiltInField(field, value, result);
                                }
                            }
                            if (isFirst == false)
                            {
                                result.Append(" }");
                            }
                            result.Append(");");
                            result.AppendLine();
                        }
                    }
                }
            }
        }

        private static void GenerateCSharpRowBuiltInField(MetaCSharpSchema field, object value, StringBuilder result)
        {
            string fieldNameCSharp = field.FieldNameCSharp;
            string fieldTypeCSharp = UtilGenerate.SqlTypeToCSharpType(field.Schema.SqlType, field.Schema.IsNullable);
            var frameworkTypeEnum = UtilDalType.SqlTypeToFrameworkTypeEnum(field.Schema.SqlType);
            FrameworkType frameworkType = UtilDalType.FrameworkTypeFromEnum(frameworkTypeEnum);
            string valueCSharp = frameworkType.ValueToCSharp(value);
            result.Append(string.Format("{0} = {1}", fieldNameCSharp, valueCSharp));
        }

        /// <summary>
        /// Generate CSharp code.
        /// </summary>
        /// <param name="isApplication">If false, generate code for cli. If true, generate code for Application.</param>
        public void Run(out string cSharp, bool isFrameworkDb, bool isApplication)
        {
            StringBuilder result = new StringBuilder();
            result.AppendLine("// Do not modify this file. It's generated by Framework.Cli.");
            result.AppendLine();
            GenerateCSharpSchemaName(MetaCSharp, isFrameworkDb, isApplication, result);
            cSharp = result.ToString();
        }
    }
}
