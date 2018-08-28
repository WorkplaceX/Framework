// Do not modify this file. It's generated by Framework.Cli.

namespace Database.dbo
{
    using Framework.Dal;
    using System;

    [SqlTable("dbo", "FrameworkColumn")]
    public class FrameworkColumn : Row
    {
        [SqlField("Id", typeof(FrameworkColumn_Id), true)]
        public int Id { get; set; }

        [SqlField("TableId", typeof(FrameworkColumn_TableId))]
        public int TableId { get; set; }

        [SqlField("ColumnNameCSharp", typeof(FrameworkColumn_ColumnNameCSharp))]
        public string ColumnNameCSharp { get; set; }

        [SqlField("ColumnNameSql", typeof(FrameworkColumn_ColumnNameSql))]
        public string ColumnNameSql { get; set; }

        [SqlField("IsExist", typeof(FrameworkColumn_IsExist))]
        public bool IsExist { get; set; }
    }

    public class FrameworkColumn_Id : Cell<FrameworkColumn> { }

    public class FrameworkColumn_TableId : Cell<FrameworkColumn> { }

    public class FrameworkColumn_ColumnNameCSharp : Cell<FrameworkColumn> { }

    public class FrameworkColumn_ColumnNameSql : Cell<FrameworkColumn> { }

    public class FrameworkColumn_IsExist : Cell<FrameworkColumn> { }

    [SqlTable("dbo", "FrameworkColumnBuiltIn")]
    public class FrameworkColumnBuiltIn : Row
    {
        [SqlField("Id", typeof(FrameworkColumnBuiltIn_Id))]
        public int Id { get; set; }

        [SqlField("Name", typeof(FrameworkColumnBuiltIn_Name))]
        public string Name { get; set; }
    }

    public class FrameworkColumnBuiltIn_Id : Cell<FrameworkColumnBuiltIn> { }

    public class FrameworkColumnBuiltIn_Name : Cell<FrameworkColumnBuiltIn> { }

    [SqlTable("dbo", "FrameworkConfigColumn")]
    public class FrameworkConfigColumn : Row
    {
        [SqlField("Id", typeof(FrameworkConfigColumn_Id), true)]
        public int Id { get; set; }

        [SqlField("IsBuiltIn", typeof(FrameworkConfigColumn_IsBuiltIn))]
        public bool IsBuiltIn { get; set; }

        [SqlField("ConfigName", typeof(FrameworkConfigColumn_ConfigName))]
        public string ConfigName { get; set; }

        [SqlField("TypeName", typeof(FrameworkConfigColumn_TypeName))]
        public string TypeName { get; set; }

        [SqlField("FieldName", typeof(FrameworkConfigColumn_FieldName))]
        public string FieldName { get; set; }

        [SqlField("Text", typeof(FrameworkConfigColumn_Text))]
        public string Text { get; set; }

        [SqlField("Description", typeof(FrameworkConfigColumn_Description))]
        public string Description { get; set; }

        [SqlField("IsVisible", typeof(FrameworkConfigColumn_IsVisible))]
        public bool IsVisible { get; set; }

        [SqlField("IsReadOnly", typeof(FrameworkConfigColumn_IsReadOnly))]
        public bool IsReadOnly { get; set; }

        [SqlField("Sort", typeof(FrameworkConfigColumn_Sort))]
        public double? Sort { get; set; }
    }

    public class FrameworkConfigColumn_Id : Cell<FrameworkConfigColumn> { }

    public class FrameworkConfigColumn_IsBuiltIn : Cell<FrameworkConfigColumn> { }

    public class FrameworkConfigColumn_ConfigName : Cell<FrameworkConfigColumn> { }

    public class FrameworkConfigColumn_TypeName : Cell<FrameworkConfigColumn> { }

    public class FrameworkConfigColumn_FieldName : Cell<FrameworkConfigColumn> { }

    public class FrameworkConfigColumn_Text : Cell<FrameworkConfigColumn> { }

    public class FrameworkConfigColumn_Description : Cell<FrameworkConfigColumn> { }

    public class FrameworkConfigColumn_IsVisible : Cell<FrameworkConfigColumn> { }

    public class FrameworkConfigColumn_IsReadOnly : Cell<FrameworkConfigColumn> { }

    public class FrameworkConfigColumn_Sort : Cell<FrameworkConfigColumn> { }

    [SqlTable("dbo", "FrameworkConfigGrid")]
    public class FrameworkConfigGrid : Row
    {
        [SqlField("Id", typeof(FrameworkConfigGrid_Id), true)]
        public int Id { get; set; }

        [SqlField("IsBuiltIn", typeof(FrameworkConfigGrid_IsBuiltIn))]
        public bool IsBuiltIn { get; set; }

        [SqlField("ConfigName", typeof(FrameworkConfigGrid_ConfigName))]
        public string ConfigName { get; set; }

        [SqlField("TypeName", typeof(FrameworkConfigGrid_TypeName))]
        public string TypeName { get; set; }

        [SqlField("RowCountMax", typeof(FrameworkConfigGrid_RowCountMax))]
        public int? RowCountMax { get; set; }

        [SqlField("IsInsert", typeof(FrameworkConfigGrid_IsInsert))]
        public bool? IsInsert { get; set; }
    }

    public class FrameworkConfigGrid_Id : Cell<FrameworkConfigGrid> { }

    public class FrameworkConfigGrid_IsBuiltIn : Cell<FrameworkConfigGrid> { }

    public class FrameworkConfigGrid_ConfigName : Cell<FrameworkConfigGrid> { }

    public class FrameworkConfigGrid_TypeName : Cell<FrameworkConfigGrid> { }

    public class FrameworkConfigGrid_RowCountMax : Cell<FrameworkConfigGrid> { }

    public class FrameworkConfigGrid_IsInsert : Cell<FrameworkConfigGrid> { }

    [SqlTable("dbo", "FrameworkScript")]
    public class FrameworkScript : Row
    {
        [SqlField("Id", typeof(FrameworkScript_Id), true)]
        public int Id { get; set; }

        [SqlField("FileName", typeof(FrameworkScript_FileName))]
        public string FileName { get; set; }

        [SqlField("Date", typeof(FrameworkScript_Date))]
        public DateTime? Date { get; set; }
    }

    public class FrameworkScript_Id : Cell<FrameworkScript> { }

    public class FrameworkScript_FileName : Cell<FrameworkScript> { }

    public class FrameworkScript_Date : Cell<FrameworkScript> { }

    [SqlTable("dbo", "FrameworkTable")]
    public class FrameworkTable : Row
    {
        [SqlField("Id", typeof(FrameworkTable_Id), true)]
        public int Id { get; set; }

        [SqlField("TableNameCSharp", typeof(FrameworkTable_TableNameCSharp))]
        public string TableNameCSharp { get; set; }

        [SqlField("TableNameSql", typeof(FrameworkTable_TableNameSql))]
        public string TableNameSql { get; set; }

        [SqlField("IsExist", typeof(FrameworkTable_IsExist))]
        public bool IsExist { get; set; }
    }

    public class FrameworkTable_Id : Cell<FrameworkTable> { }

    public class FrameworkTable_TableNameCSharp : Cell<FrameworkTable> { }

    public class FrameworkTable_TableNameSql : Cell<FrameworkTable> { }

    public class FrameworkTable_IsExist : Cell<FrameworkTable> { }

    [SqlTable("dbo", "FrameworkTableBuiltIn")]
    public class FrameworkTableBuiltIn : Row
    {
        [SqlField("Id", typeof(FrameworkTableBuiltIn_Id))]
        public int Id { get; set; }

        [SqlField("Name", typeof(FrameworkTableBuiltIn_Name))]
        public string Name { get; set; }
    }

    public class FrameworkTableBuiltIn_Id : Cell<FrameworkTableBuiltIn> { }

    public class FrameworkTableBuiltIn_Name : Cell<FrameworkTableBuiltIn> { }
}
