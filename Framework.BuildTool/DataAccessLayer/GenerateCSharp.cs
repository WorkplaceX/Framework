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
            var tableNameList = metaCSharp.List.Where(item => item.Schema.SchemaName == schemaName).GroupBy(item => new { item.Schema.TableName, item.TableNameCSharp }, (key, group) => key).ToArray();
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
                result.AppendLine(string.Format("    [SqlName(\"{0}\")]", item.TableNameCSharp));
                result.AppendLine(string.Format("    public partial class {0} : Row", item.TableNameCSharp));
                result.AppendLine("    {");
                FieldNameProperty(metaCSharp, schemaName, item.TableName, result);
                result.AppendLine("    }");
                result.AppendLine();
                FieldNameClass(metaCSharp, schemaName, item.TableName, result);
            }
        }

        /// <summary>
        /// Generate CSharp property for every database field.
        /// </summary>
        private static void FieldNameProperty(MetaCSharp metaCSharp, string schemaName, string tableName, StringBuilder result)
        {
            var fieldNameList = metaCSharp.List.Where(item => item.Schema.SchemaName == schemaName && item.Schema.TableName == tableName).ToArray();
            bool isFirst = true;
            foreach (var item in fieldNameList)
            {
                if (isFirst)
                {
                    isFirst = false;
                }
                else
                {
                    result.AppendLine();
                }
                string typeCSharp = Util.SqlTypeToCSharpType(item.Schema.SqlType, item.Schema.IsNullable);
                result.AppendLine(string.Format("        [SqlName(\"{0}\")]", item.Schema.FieldName));
                result.AppendLine(string.Format("        [TypeCell(typeof({0}))]", item.TableNameCSharp + "_" + item.FieldNameCSharp));
                result.AppendLine(string.Format("        public " + typeCSharp + " {0} {{ get; set; }}", item.FieldNameCSharp));
            }
        }

        /// <summary>
        /// Generate CSharp class for every database field.
        /// </summary>
        private static void FieldNameClass(MetaCSharp metaCSharp, string schemaName, string tableName, StringBuilder result)
        {
            var fieldNameList = metaCSharp.List.Where(item => item.Schema.SchemaName == schemaName && item.Schema.TableName == tableName).ToArray();
            bool isFirst = true;
            foreach (var item in fieldNameList)
            {
                if (isFirst)
                {
                    isFirst = false;
                }
                else
                {
                    result.AppendLine();
                }
                string typeCSharp = Util.SqlTypeToCSharpType(item.Schema.SqlType, item.Schema.IsNullable);
                result.AppendLine("    public partial class " + item.TableNameCSharp + "_" + item.FieldNameCSharp + " : Cell<" + item.TableNameCSharp + "> { }");
            }
        }

        /// <summary>
        /// Generate CSharp code.
        /// </summary>
        public void Run(out string cSharp)
        {
            StringBuilder result = new StringBuilder();
            SchemaName(MetaCSharp, result);
            cSharp = result.ToString();
        }
    }
}
