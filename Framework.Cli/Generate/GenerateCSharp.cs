﻿namespace Framework.Cli.Generate
{
    using Framework.DataAccessLayer;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// Generate CSharp code.
    /// </summary>
    internal class CSharpGenerate
    {
        public CSharpGenerate(MetaCSharp metaCSharp)
        {
            this.MetaCSharp = metaCSharp;
        }

        public readonly MetaCSharp MetaCSharp;

        /// <summary>
        /// Generate CSharp namespace for every database schema.
        /// </summary>
        private static void SchemaName(bool isFrameworkDb, MetaCSharp metaCSharp, StringBuilder result)
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
                TableNameClass(metaCSharp, item.SchemaName, result);
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
                if (type == typeof(string))
                {
                    result = "null";
                }
                else
                {
                    result = Activator.CreateInstance(type).ToString();
                }
            }
            else
            {
                result = value.ToString();
            }
            if (type == typeof(bool))
            {
                result = result.ToLower();
            }
            if (type == typeof(string) && value != null)
            {
                string valueString = (string)(object)value;
                valueString = valueString.Replace("\"", @"\""");
                result = string.Format("\"{0}\"", valueString);
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
        /// Generate CSharp class for every database table.
        /// </summary>
        private static void TableNameClass(MetaCSharp metaCSharp, string schemaName, StringBuilder result)
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
                result.AppendLine(string.Format("    public class {0} : Row", item.TableNameCSharp));
                result.AppendLine("    {");
                FieldNameProperty(metaCSharp, schemaName, item.TableName, result);
                result.AppendLine("    }");
                // result.AppendLine();
                // FieldNameClass(metaCSharp, schemaName, item.TableName, result);
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
                if (item.FrameworkTypeEnum == FrameworkTypeEnum.None)
                {
                    UtilCliInternal.ConsoleWriteLineColor(string.Format("Warning! Type not supported by framework. ({0}.{1}.{2})", item.Schema.SchemaName, item.Schema.TableName, item.Schema.FieldName), ConsoleColor.Yellow); // Warning
                }
                else
                {
                    string typeCSharp = UtilGenerate.SqlTypeToCSharpType(item.Schema.SqlType, item.Schema.IsNullable);
                    if (item.IsPrimaryKey == false)
                    {
                        result.AppendLine(string.Format("        [SqlField(\"{0}\", {2})]", item.Schema.FieldName, item.TableNameCSharp + "_" + item.FieldNameCSharp, nameof(FrameworkTypeEnum) + "." + item.FrameworkTypeEnum.ToString()));
                    }
                    else
                    {
                        result.AppendLine(string.Format("        [SqlField(\"{0}\", {2}, {3})]", item.Schema.FieldName, item.TableNameCSharp + "_" + item.FieldNameCSharp, item.IsPrimaryKey.ToString().ToLower(), nameof(FrameworkTypeEnum) + "." + item.FrameworkTypeEnum.ToString()));
                    }
                    result.AppendLine(string.Format("        public " + typeCSharp + " {0} {{ get; set; }}", item.FieldNameCSharp));
                }
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
                result.AppendLine("    public class " + item.TableNameCSharp + "_" + item.FieldNameCSharp + " : Cell<" + item.TableNameCSharp + "> { }");
            }
        }

        /// <summary>
        /// Generate CSharp code for file Database.cs
        /// </summary>
        public void Run(bool isFrameworkDb, out string cSharp)
        {
            StringBuilder result = new StringBuilder();
            result.AppendLine("// Do not modify this file. It's generated by Framework.Cli generate command."); // File Database.cs
            result.AppendLine();
            SchemaName(isFrameworkDb, MetaCSharp, result);
            cSharp = result.ToString();
        }
    }
}
