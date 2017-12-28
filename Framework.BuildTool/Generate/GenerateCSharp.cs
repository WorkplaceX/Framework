namespace Framework.BuildTool.DataAccessLayer
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// Generate CSharp code.
    /// </summary>
    public class CSharpGenerate
    {
        public CSharpGenerate(MetaCSharp metaCSharp)
        {
            this.MetaCSharp = metaCSharp;
        }

        public readonly MetaCSharp MetaCSharp;

        /// <summary>
        /// Generate CSharp namespace for every database schema.
        /// </summary>
        private static void SchemaName(MetaCSharp metaCSharp, StringBuilder result)
        {
            var schemaNameList = metaCSharp.List.GroupBy(item => new { item.Schema.SchemaName, item.SchemaNameCSharp }, (key, group) => key).ToArray();
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
                result.AppendLine(string.Format("namespace Database.{0}", item.SchemaNameCSharp));
                result.AppendLine("{");
                result.AppendLine("    using Framework.DataAccessLayer;");
                result.AppendLine("    using System;");
                result.AppendLine();
                TableName(metaCSharp, item.SchemaName, result);
                result.AppendLine("}");
            }
        }

        /// <summary>
        /// Generate CSharp class for every database table.
        /// </summary>
        private static void TableName(MetaCSharp metaCSharp, string schemaName, StringBuilder result)
        {
            var tableNameList = metaCSharp.List.Where(item => item.Schema.SchemaName == schemaName).GroupBy(item => new { item.Schema.SchemaName, item.Schema.TableName, item.TableNameCSharp }, (key, group) => key).ToArray();
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
                result.AppendLine(string.Format("    [SqlTable(\"{0}\", \"{1}\")]", item.SchemaName, item.TableName));
                result.AppendLine(string.Format("    public partial class {0} : Row", item.TableNameCSharp));
                result.AppendLine("    {");
                ColumnNameProperty(metaCSharp, schemaName, item.TableName, result);
                result.AppendLine("    }");
                result.AppendLine();
                ColumnNameClass(metaCSharp, schemaName, item.TableName, result);
            }
        }

        /// <summary>
        /// Generate CSharp property for every database column.
        /// </summary>
        private static void ColumnNameProperty(MetaCSharp metaCSharp, string schemaName, string tableName, StringBuilder result)
        {
            var columnNameList = metaCSharp.List.Where(item => item.Schema.SchemaName == schemaName && item.Schema.TableName == tableName).ToArray();
            bool isFirst = true;
            foreach (var item in columnNameList)
            {
                if (isFirst)
                {
                    isFirst = false;
                }
                else
                {
                    result.AppendLine();
                }
                string typeCSharp = UtilGenerate.SqlTypeToCSharpType(item.Schema.SqlType, item.Schema.IsNullable);
                result.AppendLine(string.Format("        [SqlColumn(\"{0}\", typeof({1}))]", item.Schema.ColumnName, item.TableNameCSharp + "_" + item.ColumnNameCSharp));
                result.AppendLine(string.Format("        public " + typeCSharp + " {0} {{ get; set; }}", item.ColumnNameCSharp));
            }
        }

        /// <summary>
        /// Generate CSharp class for every database column.
        /// </summary>
        private static void ColumnNameClass(MetaCSharp metaCSharp, string schemaName, string tableName, StringBuilder result)
        {
            var columnNameList = metaCSharp.List.Where(item => item.Schema.SchemaName == schemaName && item.Schema.TableName == tableName).ToArray();
            bool isFirst = true;
            foreach (var item in columnNameList)
            {
                if (isFirst)
                {
                    isFirst = false;
                }
                else
                {
                    result.AppendLine();
                }
                string typeCSharp = UtilGenerate.SqlTypeToCSharpType(item.Schema.SqlType, item.Schema.IsNullable);
                result.AppendLine("    public partial class " + item.TableNameCSharp + "_" + item.ColumnNameCSharp + " : Cell<" + item.TableNameCSharp + "> { }");
            }
        }

        /// <summary>
        /// Generate CSharp code.
        /// </summary>
        public void Run(out string cSharp)
        {
            StringBuilder result = new StringBuilder();
            result.AppendLine("// Do not modify this file. It's generated by BuildTool generate command.");
            result.AppendLine("//");
            SchemaName(MetaCSharp, result);
            cSharp = result.ToString();
        }
    }
}
