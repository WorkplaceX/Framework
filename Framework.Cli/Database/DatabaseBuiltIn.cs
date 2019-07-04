// Do not modify this file. It's generated by Framework.Cli.

namespace DatabaseFrameworkBuiltIn.dbo
{
    using System.Collections.Generic;
    using DatabaseFramework.dbo;

    public static class FrameworkConfigGridBuiltInCli
    {
        public static List<FrameworkConfigGridBuiltIn> List
        {
            get
            {
                var result = new List<FrameworkConfigGridBuiltIn>();
                result.Add(new FrameworkConfigGridBuiltIn() { Id = 1, IdName = "dbo.FrameworkConfigFieldDisplay; ", TableId = 3, TableIdName = "dbo.FrameworkConfigFieldDisplay", TableNameCSharp = "dbo.FrameworkConfigFieldDisplay", ConfigName = null, RowCountMax = null, IsAllowInsert = null, IsExist = true });
                result.Add(new FrameworkConfigGridBuiltIn() { Id = 2, IdName = "dbo.FrameworkConfigGridDisplay; ", TableId = 6, TableIdName = "dbo.FrameworkConfigGridDisplay", TableNameCSharp = "dbo.FrameworkConfigGridDisplay", ConfigName = null, RowCountMax = null, IsAllowInsert = null, IsExist = true });
                return result;
            }
        }
    }

    public static class FrameworkConfigFieldBuiltInCli
    {
        public static List<FrameworkConfigFieldBuiltIn> List
        {
            get
            {
                var result = new List<FrameworkConfigFieldBuiltIn>();
                result.Add(new FrameworkConfigFieldBuiltIn() { Id = 1, ConfigGridId = 1, ConfigGridIdName = "dbo.FrameworkConfigFieldDisplay; ", FieldId = 39, FieldIdName = "dbo.FrameworkConfigFieldDisplay; ConfigFieldDescription", TableNameCSharp = "dbo.FrameworkConfigFieldDisplay", ConfigName = null, FieldNameCSharp = "ConfigFieldDescription", Text = "Description", Description = "Description", IsVisible = null, IsReadOnly = null, Sort = null, IsExist = true });
                result.Add(new FrameworkConfigFieldBuiltIn() { Id = 2, ConfigGridId = 1, ConfigGridIdName = "dbo.FrameworkConfigFieldDisplay; ", FieldId = 41, FieldIdName = "dbo.FrameworkConfigFieldDisplay; ConfigFieldIsReadOnly", TableNameCSharp = "dbo.FrameworkConfigFieldDisplay", ConfigName = null, FieldNameCSharp = "ConfigFieldIsReadOnly", Text = "Readonly", Description = "Readonly", IsVisible = null, IsReadOnly = null, Sort = null, IsExist = true });
                result.Add(new FrameworkConfigFieldBuiltIn() { Id = 3, ConfigGridId = 1, ConfigGridIdName = "dbo.FrameworkConfigFieldDisplay; ", FieldId = 40, FieldIdName = "dbo.FrameworkConfigFieldDisplay; ConfigFieldIsVisible", TableNameCSharp = "dbo.FrameworkConfigFieldDisplay", ConfigName = null, FieldNameCSharp = "ConfigFieldIsVisible", Text = "Visible", Description = null, IsVisible = null, IsReadOnly = null, Sort = null, IsExist = true });
                result.Add(new FrameworkConfigFieldBuiltIn() { Id = 4, ConfigGridId = 1, ConfigGridIdName = "dbo.FrameworkConfigFieldDisplay; ", FieldId = 42, FieldIdName = "dbo.FrameworkConfigFieldDisplay; ConfigFieldSort", TableNameCSharp = "dbo.FrameworkConfigFieldDisplay", ConfigName = null, FieldNameCSharp = "ConfigFieldSort", Text = "Sort", Description = null, IsVisible = null, IsReadOnly = null, Sort = null, IsExist = true });
                result.Add(new FrameworkConfigFieldBuiltIn() { Id = 5, ConfigGridId = 1, ConfigGridIdName = "dbo.FrameworkConfigFieldDisplay; ", FieldId = 38, FieldIdName = "dbo.FrameworkConfigFieldDisplay; ConfigFieldText", TableNameCSharp = "dbo.FrameworkConfigFieldDisplay", ConfigName = null, FieldNameCSharp = "ConfigFieldText", Text = "Tex", Description = null, IsVisible = null, IsReadOnly = null, Sort = null, IsExist = true });
                result.Add(new FrameworkConfigFieldBuiltIn() { Id = 6, ConfigGridId = 1, ConfigGridIdName = "dbo.FrameworkConfigFieldDisplay; ", FieldId = 28, FieldIdName = "dbo.FrameworkConfigFieldDisplay; ConfigGridConfigName", TableNameCSharp = "dbo.FrameworkConfigFieldDisplay", ConfigName = null, FieldNameCSharp = "ConfigGridConfigName", Text = null, Description = null, IsVisible = false, IsReadOnly = null, Sort = null, IsExist = true });
                result.Add(new FrameworkConfigFieldBuiltIn() { Id = 7, ConfigGridId = 1, ConfigGridIdName = "dbo.FrameworkConfigFieldDisplay; ", FieldId = 29, FieldIdName = "dbo.FrameworkConfigFieldDisplay; ConfigGridIsExist", TableNameCSharp = "dbo.FrameworkConfigFieldDisplay", ConfigName = null, FieldNameCSharp = "ConfigGridIsExist", Text = null, Description = null, IsVisible = false, IsReadOnly = null, Sort = null, IsExist = true });
                result.Add(new FrameworkConfigFieldBuiltIn() { Id = 8, ConfigGridId = 1, ConfigGridIdName = "dbo.FrameworkConfigFieldDisplay; ", FieldId = 26, FieldIdName = "dbo.FrameworkConfigFieldDisplay; ConfigGridTableIdName", TableNameCSharp = "dbo.FrameworkConfigFieldDisplay", ConfigName = null, FieldNameCSharp = "ConfigGridTableIdName", Text = null, Description = null, IsVisible = false, IsReadOnly = null, Sort = null, IsExist = true });
                result.Add(new FrameworkConfigFieldBuiltIn() { Id = 9, ConfigGridId = 1, ConfigGridIdName = "dbo.FrameworkConfigFieldDisplay; ", FieldId = 27, FieldIdName = "dbo.FrameworkConfigFieldDisplay; ConfigGridTableNameCSharp", TableNameCSharp = "dbo.FrameworkConfigFieldDisplay", ConfigName = null, FieldNameCSharp = "ConfigGridTableNameCSharp", Text = null, Description = null, IsVisible = false, IsReadOnly = null, Sort = null, IsExist = true });
                result.Add(new FrameworkConfigFieldBuiltIn() { Id = 10, ConfigGridId = 1, ConfigGridIdName = "dbo.FrameworkConfigFieldDisplay; ", FieldId = 32, FieldIdName = "dbo.FrameworkConfigFieldDisplay; FieldFieldNameCSharp", TableNameCSharp = "dbo.FrameworkConfigFieldDisplay", ConfigName = null, FieldNameCSharp = "FieldFieldNameCSharp", Text = "Field Name", Description = null, IsVisible = null, IsReadOnly = null, Sort = null, IsExist = true });
                result.Add(new FrameworkConfigFieldBuiltIn() { Id = 11, ConfigGridId = 1, ConfigGridIdName = "dbo.FrameworkConfigFieldDisplay; ", FieldId = 33, FieldIdName = "dbo.FrameworkConfigFieldDisplay; FieldFieldNameSql", TableNameCSharp = "dbo.FrameworkConfigFieldDisplay", ConfigName = null, FieldNameCSharp = "FieldFieldNameSql", Text = null, Description = null, IsVisible = false, IsReadOnly = null, Sort = null, IsExist = true });
                result.Add(new FrameworkConfigFieldBuiltIn() { Id = 12, ConfigGridId = 1, ConfigGridIdName = "dbo.FrameworkConfigFieldDisplay; ", FieldId = 34, FieldIdName = "dbo.FrameworkConfigFieldDisplay; FieldIsExist", TableNameCSharp = "dbo.FrameworkConfigFieldDisplay", ConfigName = null, FieldNameCSharp = "FieldIsExist", Text = null, Description = null, IsVisible = false, IsReadOnly = null, Sort = null, IsExist = true });
                result.Add(new FrameworkConfigFieldBuiltIn() { Id = 13, ConfigGridId = 2, ConfigGridIdName = "dbo.FrameworkConfigGridDisplay; ", FieldId = 64, FieldIdName = "dbo.FrameworkConfigGridDisplay; IsAllowInsert", TableNameCSharp = "dbo.FrameworkConfigGridDisplay", ConfigName = null, FieldNameCSharp = "IsAllowInsert", Text = "Allow Insert", Description = null, IsVisible = null, IsReadOnly = null, Sort = null, IsExist = true });
                result.Add(new FrameworkConfigFieldBuiltIn() { Id = 14, ConfigGridId = 2, ConfigGridIdName = "dbo.FrameworkConfigGridDisplay; ", FieldId = 65, FieldIdName = "dbo.FrameworkConfigGridDisplay; IsExist", TableNameCSharp = "dbo.FrameworkConfigGridDisplay", ConfigName = null, FieldNameCSharp = "IsExist", Text = null, Description = null, IsVisible = false, IsReadOnly = null, Sort = null, IsExist = true });
                result.Add(new FrameworkConfigFieldBuiltIn() { Id = 15, ConfigGridId = 2, ConfigGridIdName = "dbo.FrameworkConfigGridDisplay; ", FieldId = 63, FieldIdName = "dbo.FrameworkConfigGridDisplay; RowCountMax", TableNameCSharp = "dbo.FrameworkConfigGridDisplay", ConfigName = null, FieldNameCSharp = "RowCountMax", Text = "Row Count", Description = null, IsVisible = null, IsReadOnly = null, Sort = null, IsExist = true });
                result.Add(new FrameworkConfigFieldBuiltIn() { Id = 16, ConfigGridId = 2, ConfigGridIdName = "dbo.FrameworkConfigGridDisplay; ", FieldId = 60, FieldIdName = "dbo.FrameworkConfigGridDisplay; TableNameCSharp", TableNameCSharp = "dbo.FrameworkConfigGridDisplay", ConfigName = null, FieldNameCSharp = "TableNameCSharp", Text = "Table Name", Description = null, IsVisible = null, IsReadOnly = null, Sort = null, IsExist = true });
                result.Add(new FrameworkConfigFieldBuiltIn() { Id = 17, ConfigGridId = 2, ConfigGridIdName = "dbo.FrameworkConfigGridDisplay; ", FieldId = 61, FieldIdName = "dbo.FrameworkConfigGridDisplay; TableNameSql", TableNameCSharp = "dbo.FrameworkConfigGridDisplay", ConfigName = null, FieldNameCSharp = "TableNameSql", Text = null, Description = null, IsVisible = false, IsReadOnly = null, Sort = null, IsExist = true });
                return result;
            }
        }
    }
}
