namespace Framework.BuildTool.DataAccessLayer
{
    using Database.dbo;
    using System;
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
        private static void SchemaName(MetaCSharp metaCSharp, FrameworkConfigGridView[] configGridList, StringBuilder result)
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
                TableName(metaCSharp, configGridList, item.SchemaName, result);
                result.AppendLine("}");
            }
        }

        /// <summary>
        /// Return (null, 4, true, "Text").
        /// </summary>
        private static string CSharpParam<T>(T value)
        {
            Type type = UtilFramework.TypeUnderlying(typeof(T));
            string result;
            if (value == null)
            {
                result = Activator.CreateInstance(type).ToString();
            }
            else
            {
                result = value.ToString();
            }
            if (type == typeof(bool))
            {
                result = result.ToLower();
            }
            if (type == typeof(string))
            {
                result = string.Format("\"{0}\"", value);
            }
            return result;
        }

        private static void CSharpParam<T>(T valueDefault, T value, out string param, out string paramIsNull)
        {
            if (value == null)
            {
                value = valueDefault;
            }
            param = CSharpParam(value);
            if (value == null)
            {
                paramIsNull = CSharpParam(true);
            }
            else
            {
                paramIsNull = CSharpParam(false);
            }
        }

        /// <summary>
        /// Generate CSharp ConfigGrid attributes.
        /// </summary>
        private static void TableNameAttribute(MetaCSharp metaCSharp, FrameworkConfigGridView[] configGridList, string schemaName, string tableNameCSharp, StringBuilder result)
        {
            foreach (var config in configGridList.Where(itemConfig => itemConfig.TableNameCSharp == schemaName + "." + tableNameCSharp))
            {
                if (config.PageRowCountDefault != null || config.PageRowCount != null || config.IsInsertDefault != null || config.IsInsert != null)
                {
                    CSharpParam(null, config.GridName, out string gridNameParam, out string gridNameIsNullParam);
                    CSharpParam(config.PageRowCountDefault, config.PageRowCount, out string pageRowCountParam, out string pageRowCountIsNullParam);
                    CSharpParam(config.IsInsertDefault, config.IsInsert, out string isInsertParam, out string isInsertIsNullParam);
                    result.AppendLine(string.Format("    [ConfigGrid({0}, {1}, {2}, {3}, {4})]", gridNameParam, pageRowCountParam, pageRowCountIsNullParam, isInsertParam, isInsertIsNullParam));
                }
            }
        }

        /// <summary>
        /// Generate CSharp class for every database table.
        /// </summary>
        private static void TableName(MetaCSharp metaCSharp, FrameworkConfigGridView[] configGridList, string schemaName, StringBuilder result)
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
                TableNameAttribute(metaCSharp, configGridList, item.SchemaName, item.TableName, result);
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
        public void Run(FrameworkConfigGridView[] configGridList, out string cSharp)
        {
            StringBuilder result = new StringBuilder();
            result.AppendLine("// Do not modify this file. It's generated by BuildTool generate command.");
            result.AppendLine("//");
            SchemaName(MetaCSharp, configGridList, result);
            cSharp = result.ToString();
        }
    }
}
