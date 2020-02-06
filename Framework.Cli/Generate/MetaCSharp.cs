namespace Framework.Cli.Generate
{
    using Framework.DataAccessLayer;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Meta information to generate CSharp code.
    /// </summary>
    public class MetaCSharp
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public MetaCSharp(MetaSql metaSql)
        {
            SchemaName(metaSql.List);
        }

        private void SchemaName(MetaSqlSchema[] dataList)
        {
            string[] schemaNameList = dataList.GroupBy(item => item.SchemaName, (key, group) => key).ToArray();
            List<string> nameExceptList = new List<string>();
            foreach (string schemaName in schemaNameList)
            {
                string schemaNameCSharp = UtilGenerate.NameCSharp(schemaName, nameExceptList);
                TableName(dataList, schemaName, schemaNameCSharp);
            }
        }

        private void TableName(MetaSqlSchema[] dataList, string schemaName, string schemaNameCSharp)
        {
            string[] tableNameList = dataList.Where(item => item.SchemaName == schemaName).GroupBy(item => item.TableName, (key, group) => key).ToArray();
            List<string> nameExceptList = new List<string>();
            foreach (string tableName in tableNameList)
            {
                string tableNameCSharp = UtilGenerate.NameCSharp(tableName, nameExceptList);
                FieldName(dataList, schemaName, schemaNameCSharp, tableName, tableNameCSharp);
            }
        }

        private void FieldName(MetaSqlSchema[] dataList, string schemaName, string schemaNameCSharp, string tableName, string tableNameCSharp)
        {
            MetaSqlSchema[] fieldList = dataList.Where(item => item.SchemaName == schemaName && item.TableName == tableName).ToArray();
            List<string> nameExceptList = new List<string>();
            nameExceptList.Add(tableName); // CSharp propery can not have same name like class.
            foreach (MetaSqlSchema field in fieldList)
            {
                string fieldNameCSharp = UtilGenerate.NameCSharp(field.FieldName, nameExceptList);
                FrameworkTypeEnum frameworkTypeEnum = UtilDalType.SqlTypeToFrameworkTypeEnum(field.SqlType);
                List.Add(new MetaCSharpSchema()
                {
                    Schema = field,
                    SchemaNameCSharp = schemaNameCSharp,
                    TableNameCSharp = tableNameCSharp,
                    FieldNameCSharp = fieldNameCSharp,
                    IsPrimaryKey = field.IsPrimaryKey,
                    FrameworkTypeEnum = frameworkTypeEnum,
                });
            }
        }

        public readonly List<MetaCSharpSchema> List = new List<MetaCSharpSchema>();
    }

    public class MetaCSharpSchema
    {
        public MetaSqlSchema Schema { get; set; }

        public string SchemaNameCSharp { get; set; }

        public string TableNameCSharp { get; set; }

        public string FieldNameCSharp { get; set; }

        public bool IsPrimaryKey { get; set; }

        public FrameworkTypeEnum FrameworkTypeEnum { get; set; }
    }
}
