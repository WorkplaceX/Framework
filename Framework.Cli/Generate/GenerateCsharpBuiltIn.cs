namespace Framework.Cli.Generate
{
    using Framework.DataAccessLayer;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using static Framework.Cli.Command.AppCli;

    public static class NamingConventionBuiltIn
    {
        public static bool TableNameIsBuiltIn(string tableNameCSharp)
        {
            return tableNameCSharp.EndsWith("BuiltIn");
        }
    }

    public class GenerateCSharpBuiltIn
    {
        /// <summary>
        /// Generate CSharp namespace for every database schema.
        /// </summary>
        private static void GenerateCSharpSchemaName(List<GenerateBuiltInItem> builtInList, bool isFrameworkDb, bool isApplication, StringBuilder result)
        {
            builtInList = builtInList.Where(item => item.IsFrameworkDb == isFrameworkDb && item.IsApplication == isApplication).ToList();
            var schemaNameCSharpList = builtInList.GroupBy(item => item.SchemaNameCSharp, (key, group) => key);
            bool isFirst = true;
            foreach (string schemaNameCSharp in schemaNameCSharpList)
            {
                if (isFirst)
                {
                    isFirst = false;
                }
                else
                {
                    result.AppendLine();
                }
                result.AppendLine(string.Format("namespace DatabaseBuiltIn.{0}", schemaNameCSharp));
                result.AppendLine(string.Format("{{"));
                result.AppendLine(string.Format("    using System.Collections.Generic;"));
                result.AppendLine(string.Format("    using Database.{0};", schemaNameCSharp));
                result.AppendLine();
                GenerateCSharpTableNameClass(builtInList.Where(item => item.SchemaNameCSharp == schemaNameCSharp).ToList(), isFrameworkDb, isApplication, result);
                result.AppendLine(string.Format("}}"));
            }
        }

        /// <summary>
        /// Generate static CSharp class for every database table.
        /// </summary>
        private static void GenerateCSharpTableNameClass(List<GenerateBuiltInItem> builtInList, bool isFrameworkDb, bool isApplication, StringBuilder result)
        {
            bool isFirst = true;
            foreach (var builtIn in builtInList)
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
                result.AppendLine(string.Format("    public static class {0}{1}", builtIn.TableNameCSharp, classNameExtension));
                result.AppendLine(string.Format("    {{"));
                result.AppendLine(string.Format("        public static List<{0}> List", builtIn.TableNameCSharp));
                result.AppendLine(string.Format("        {{"));
                result.AppendLine(string.Format("            get"));
                result.AppendLine(string.Format("            {{"));
                result.AppendLine(string.Format("                var result = new List<{0}>();", builtIn.TableNameCSharp));
                GenerateCSharpRowBuiltIn(builtIn, result);
                result.AppendLine(string.Format("                return result;"));
                result.AppendLine(string.Format("            }}"));
                result.AppendLine(string.Format("        }}"));
                result.AppendLine(string.Format("    }}"));
            }
        }

        private static void GenerateCSharpRowBuiltIn(GenerateBuiltInItem builtInItem, StringBuilder result)
        {
            var fieldList = UtilDalType.TypeRowToFieldList(builtInItem.TypeRow);
            foreach (Row row in builtInItem.RowList)
            {
                result.Append(string.Format("                result.Add(new {0}()", builtInItem.TableNameCSharp));
                bool isFirst = true;
                foreach (var field in fieldList)
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
                    object value = field.PropertyInfo.GetValue(row);
                    GenerateCSharpRowBuiltInField(field, value, result);
                }
                if (isFirst == false)
                {
                    result.Append(" }");
                }
                result.Append(");");
                result.AppendLine();
            }
        }

        /// <summary>
        /// Generate CSharp property with value.
        /// </summary>
        private static void GenerateCSharpRowBuiltInField(UtilDalType.Field field, object value, StringBuilder result)
        {
            string fieldNameCSharp = field.FieldNameCSharp;
            FrameworkType frameworkType = UtilDalType.FrameworkTypeFromEnum(field.FrameworkTypeEnum);
            string valueCSharp = frameworkType.ValueToCSharp(value);
            result.Append(string.Format("{0} = {1}", fieldNameCSharp, valueCSharp));
        }

        /// <summary>
        /// Generate CSharp code.
        /// </summary>
        /// <param name="isApplication">If false, generate code for cli. If true, generate code for Application.</param>
        public void Run(out string cSharp, bool isFrameworkDb, bool isApplication, List<GenerateBuiltInItem> builtInList)
        {
            StringBuilder result = new StringBuilder();
            result.AppendLine("// Do not modify this file. It's generated by Framework.Cli.");
            result.AppendLine();
            GenerateCSharpSchemaName(builtInList, isFrameworkDb, isApplication, result);
            cSharp = result.ToString();
        }
    }
}
