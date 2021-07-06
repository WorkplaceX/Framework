namespace Framework.Cli.Generate
{
    using Framework.DataAccessLayer;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using static Framework.Cli.AppCli;

    internal static class NamingConventionIntegrate
    {
        public static bool TableNameIsIntegrate(string tableNameCSharp)
        {
            return tableNameCSharp.EndsWith("Integrate");
        }
    }

    internal class GenerateCSharpIntegrate
    {
        /// <summary>
        /// Generate CSharp namespace for every database schema.
        /// </summary>
        /// <param name="isFrameworkDb">If true, generate CSharp code for Framework library (internal use only) otherwise generate code for Application.</param>
        /// <param name="isApplication">If false, generate CSharp code for cli. If true, generate code for Application or Framework.</param>
        private static void GenerateCSharpSchemaName(List<GenerateIntegrateItem> integrateList, bool isFrameworkDb, bool isApplication, string folderNameBlob, StringBuilder result)
        {
            integrateList = integrateList.Where(item => item.IsFrameworkDb == isFrameworkDb && item.IsApplication == isApplication).ToList();
            var schemaNameCSharpList = integrateList.GroupBy(item => item.SchemaNameCSharp, (key, group) => key);
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
                result.AppendLine(string.Format("namespace DatabaseIntegrate.{0}", schemaNameCSharp));
                result.AppendLine(string.Format("{{"));
                result.AppendLine(string.Format("    using System;")); // Used for method Guid.Parse();
                result.AppendLine(string.Format("    using System.Collections.Generic;"));
                result.AppendLine(string.Format("    using System.Globalization;")); // Used for property CultureInfo.InvariantCulture;
                if (isApplication)
                {
                    // See also class IdNameEnumAttribute
                    result.AppendLine(string.Format("    using System.Linq;"));
                    result.AppendLine(string.Format("    using System.Threading.Tasks;"));
                    result.AppendLine(string.Format("    using Framework.DataAccessLayer;"));
                }
                result.AppendLine(string.Format("    using Database.{0};", schemaNameCSharp));
                result.AppendLine();
                GenerateCSharpTableNameClass(integrateList.Where(item => item.SchemaNameCSharp == schemaNameCSharp).ToList(), isFrameworkDb, isApplication, folderNameBlob, result);
                result.AppendLine(string.Format("}}"));
            }
        }

        /// <summary>
        /// Generate static CSharp class for every database table.
        /// </summary>
        private static void GenerateCSharpTableNameClass(List<GenerateIntegrateItem> integrateList, bool isFrameworkDb, bool isApplication, string folderNameBlob, StringBuilder result)
        {
            bool isFirst = true;
            foreach (var integrate in integrateList)
            {
                if (isFirst)
                {
                    isFirst = false;
                }
                else
                {
                    result.AppendLine();
                }

                // Use one "Database" and "DatabaseIntegrate" namespace for Framework and Application
                string classNameExtension = ""; // "Table"; // See also method CommandDeployDbIntegrateInternal();

                // Framework, Application
                if (isFrameworkDb)
                {
                    classNameExtension += "Framework";
                }
                else
                {
                    classNameExtension += "App";
                }

                // Cli, Application
                if (isApplication)
                {
                    classNameExtension += "";
                }
                else
                {
                    classNameExtension += "Cli";
                }

                // Integrate comment
                string commentAppTypeName = null;
                if (integrate.AppTypeName != null)
                {
                    commentAppTypeName = "AppTypeName=" + integrate.AppTypeName + "; ";
                }
                string comment = string.Format("Integrate Sql={0}; {2}Assembly={1};", integrate.TableNameSql, isApplication ? "App" : "Cli", commentAppTypeName);

                result.AppendLine(string.Format("    /// <summary>"));
                result.AppendLine(string.Format("    /// {0}", comment));
                result.AppendLine(string.Format("    /// </summary>"));
                result.AppendLine(string.Format("    public static class {0}{1}{2}", integrate.TableNameCSharp, classNameExtension, integrate.AppTypeName?.Replace(".", "")));
                result.AppendLine(string.Format("    {{"));
                if (isApplication)
                {
                    GenerateCSharpNameEnum(integrate, result);
                }
                result.AppendLine(string.Format("        public static List<{0}> RowList", integrate.TableNameCSharp));
                result.AppendLine(string.Format("        {{"));
                result.AppendLine(string.Format("            get"));
                result.AppendLine(string.Format("            {{"));
                result.AppendLine(string.Format("                var result = new List<{0}>", integrate.TableNameCSharp));
                result.AppendLine(string.Format("                {{", integrate.TableNameCSharp));
                GenerateCSharpRowIntegrate(integrate, folderNameBlob, result);
                result.AppendLine(string.Format("                }};", integrate.TableNameCSharp));
                result.AppendLine(string.Format("                return result;"));
                result.AppendLine(string.Format("            }}"));
                result.AppendLine(string.Format("        }}"));
                result.AppendLine(string.Format("    }}"));
            }
        }

        private static void GenerateCSharpNameEnum(GenerateIntegrateItem integrate, StringBuilder result)
        {
            var fieldList = UtilDalType.TypeRowToFieldList(integrate.TypeRow);
            var fieldId = fieldList.SingleOrDefault(item => item.FieldNameCSharp == "Id"); // See also FieldIntegrate.IsKey
            var fieldIdName = fieldList.SingleOrDefault(item => item.FieldNameCSharp == "IdName"); // See also FieldIntegrate.IsKey
            if (fieldIdName != null) 
            {
                result.Append(string.Format("        public enum IdEnum {{ [IdEnum(null)]None = 0"));
                List<string> nameExceptList = new List<string>();
                int count = 0;
                foreach (Row row in integrate.RowList)
                {
                    count += 1;
                    string idName = (string)fieldIdName.PropertyInfo.GetValue(row);
                    string nameCSharp = UtilGenerate.NameCSharp(idName, nameExceptList);
                    result.Append(string.Format(", [IdEnum(\"{0}\")]{1} = {2}", idName, nameCSharp, count * -1)); // Count * -1 to ensure there is no relation between enum id and database record id!
                }
                result.AppendLine(string.Format(" }}"));
                result.AppendLine();
                result.AppendLine(string.Format("        public static {0} Row(this IdEnum value)", integrate.TableNameCSharp));
                result.AppendLine(string.Format("        {{"));
                result.AppendLine(string.Format("            return RowList.Where(item => item.IdName == IdEnumAttribute.IdNameFromEnum(value)).SingleOrDefault();"));
                result.AppendLine(string.Format("        }}"));
                result.AppendLine();
                result.AppendLine(string.Format("        public static IdEnum IdName(string value)"));
                result.AppendLine(string.Format("        {{"));
                result.AppendLine(string.Format("            return IdEnumAttribute.IdNameToEnum<IdEnum>(value);"));
                result.AppendLine(string.Format("        }}"));
                result.AppendLine();
                result.AppendLine(string.Format("        public static string IdName(this IdEnum value)"));
                result.AppendLine(string.Format("        {{"));
                result.AppendLine(string.Format("            return IdEnumAttribute.IdNameFromEnum(value);"));
                result.AppendLine(string.Format("        }}"));
                result.AppendLine();
                result.AppendLine(string.Format("        public static async Task<int> Id(this IdEnum value)"));
                result.AppendLine(string.Format("        {{"));
                result.AppendLine(string.Format("            return (await Data.Query<{0}>().Where(item => item.IdName == IdEnumAttribute.IdNameFromEnum(value)).QueryExecuteAsync()).Single().Id;", integrate.TableNameCSharp));
                result.AppendLine(string.Format("        }}"));
                result.AppendLine();
            }
        }

        private static void GenerateCSharpRowIntegrate(GenerateIntegrateItem integrateItem, string folderNameBlob, StringBuilder result)
        {
            var fieldNameIdCSharpReferenceList = integrateItem.Owner.ResultReference.Where(item => item.TypeRowIntegrate == integrateItem.TypeRow).Select(item => item.FieldNameIdCSharp).ToList();
            var fieldList = UtilDalType.TypeRowToFieldList(integrateItem.TypeRow);
            foreach (Row row in integrateItem.RowList)
            {
                result.Append(string.Format("                    new {0} {{", integrateItem.TableNameCSharp));
                bool isFirst = true;
                foreach (var field in fieldList)
                {
                    if (isFirst)
                    {
                        isFirst = false;
                        result.Append(" ");
                    }
                    else
                    {
                        result.Append(", ");
                    }
                    object value = field.PropertyInfo.GetValue(row);
                    if (fieldNameIdCSharpReferenceList.Contains(field.FieldNameCSharp) || field.FieldNameCSharp == "Id")
                    {
                        UtilFramework.Assert(value == null || value.GetType() == typeof(int));

                        // Unlike IdName, Id can change from database to database.
                        value = 0;
                    }

                    // Blob FileName
                    string fileName = null;
                    if (integrateItem.Owner.ResultBlob.ContainsKey(row.GetType()))
                    {
                        if (integrateItem.Owner.ResultBlob[row.GetType()].TryGetValue(field.FieldNameCSharp, out var fileNameFunc))
                        {
                            fileName = fileNameFunc(row);
                            fileName = string.Format("{0}.{1}.{2}", integrateItem.SchemaNameCSharp, integrateItem.TableNameCSharp, fileName);
                        }
                    }

                    // Generate field
                    GenerateCSharpRowIntegrateField(field, value, fileName, integrateItem.IsApplication, folderNameBlob, result);
                }
                result.Append(" },");
                result.AppendLine();
            }
        }

        /// <summary>
        /// Generate CSharp property with value.
        /// </summary>
        private static void GenerateCSharpRowIntegrateField(UtilDalType.Field field, object value, string fileName, bool isApplication, string folderNameBlob, StringBuilder result)
        {
            string fieldNameCSharp = field.FieldNameCSharp;
            FrameworkType frameworkType = UtilDalType.FrameworkTypeFromEnum(field.FrameworkTypeEnum);
            string valueCSharp;
            if (value is string)
            {
                valueCSharp = UtilCliInternal.EscapeCSharpString(value.ToString());
            }
            else
            {
                valueCSharp = frameworkType.ValueToCSharp(value);
            }

            // Blob write
            if (fileName != null && !isApplication && (value is byte[] || value is string))
            {
                var fileNameFull = folderNameBlob + fileName;
                if (value is byte[])
                {
                    File.WriteAllBytes(fileNameFull, (byte[])value);
                    valueCSharp = $"Framework.Cli.UtilCli.BlobReadData(\"{ fileName }\")";
                }
                if (value is string)
                {
                    File.WriteAllText(fileNameFull, (string)value);
                    valueCSharp = $"Framework.Cli.UtilCli.BlobReadText(\"{ fileName }\")";
                }
            }
            
            result.Append(string.Format("{0} = {1}", fieldNameCSharp, valueCSharp));
        }

        /// <summary>
        /// Generate CSharp code for file DatabaseIntegrate.cs
        /// </summary>
        /// <param name="isApplication">If false, generate code for cli. If true, generate code for Application.</param>
        public void Run(out string cSharp, bool isFrameworkDb, bool isApplication, List<GenerateIntegrateItem> integrateList, string folderNameBlob)
        {
            StringBuilder result = new StringBuilder();
            result.AppendLine("// Do not modify this file. It's generated by Framework.Cli generate command."); // File DatabaseIntegrate.cs
            result.AppendLine();
            GenerateCSharpSchemaName(integrateList, isFrameworkDb, isApplication, folderNameBlob, result);
            cSharp = result.ToString();
        }
    }
}
