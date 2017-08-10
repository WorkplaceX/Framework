namespace Database.dbo
{
    using Framework.DataAccessLayer;
    using System;

    [SqlTable("dbo", "FrameworkApplication")]
    public partial class FrameworkApplication : Row
    {
        [SqlColumn("Id", typeof(FrameworkApplication_Id))]
        public int Id { get; set; }

        [SqlColumn("Name", typeof(FrameworkApplication_Name))]
        public string Name { get; set; }

        [SqlColumn("Path", typeof(FrameworkApplication_Path))]
        public string Path { get; set; }

        [SqlColumn("ApplicationTypeId", typeof(FrameworkApplication_ApplicationTypeId))]
        public int ApplicationTypeId { get; set; }

        [SqlColumn("IsActive", typeof(FrameworkApplication_IsActive))]
        public bool? IsActive { get; set; }
    }

    public partial class FrameworkApplication_Id : Cell<FrameworkApplication> { }

    public partial class FrameworkApplication_Name : Cell<FrameworkApplication> { }

    public partial class FrameworkApplication_Path : Cell<FrameworkApplication> { }

    public partial class FrameworkApplication_ApplicationTypeId : Cell<FrameworkApplication> { }

    public partial class FrameworkApplication_IsActive : Cell<FrameworkApplication> { }

    [SqlTable("dbo", "FrameworkApplicationType")]
    public partial class FrameworkApplicationType : Row
    {
        [SqlColumn("Id", typeof(FrameworkApplicationType_Id))]
        public int Id { get; set; }

        [SqlColumn("Name", typeof(FrameworkApplicationType_Name))]
        public string Name { get; set; }

        [SqlColumn("IsExist", typeof(FrameworkApplicationType_IsExist))]
        public bool? IsExist { get; set; }
    }

    public partial class FrameworkApplicationType_Id : Cell<FrameworkApplicationType> { }

    public partial class FrameworkApplicationType_Name : Cell<FrameworkApplicationType> { }

    public partial class FrameworkApplicationType_IsExist : Cell<FrameworkApplicationType> { }

    [SqlTable("dbo", "FrameworkApplicationView")]
    public partial class FrameworkApplicationView : Row
    {
        [SqlColumn("Id", typeof(FrameworkApplicationView_Id))]
        public int Id { get; set; }

        [SqlColumn("Name", typeof(FrameworkApplicationView_Name))]
        public string Name { get; set; }

        [SqlColumn("Path", typeof(FrameworkApplicationView_Path))]
        public string Path { get; set; }

        [SqlColumn("ApplicationTypeId", typeof(FrameworkApplicationView_ApplicationTypeId))]
        public int ApplicationTypeId { get; set; }

        [SqlColumn("Type", typeof(FrameworkApplicationView_Type))]
        public string Type { get; set; }

        [SqlColumn("IsExist", typeof(FrameworkApplicationView_IsExist))]
        public bool? IsExist { get; set; }

        [SqlColumn("IsActive", typeof(FrameworkApplicationView_IsActive))]
        public bool? IsActive { get; set; }
    }

    public partial class FrameworkApplicationView_Id : Cell<FrameworkApplicationView> { }

    public partial class FrameworkApplicationView_Name : Cell<FrameworkApplicationView> { }

    public partial class FrameworkApplicationView_Path : Cell<FrameworkApplicationView> { }

    public partial class FrameworkApplicationView_ApplicationTypeId : Cell<FrameworkApplicationView> { }

    public partial class FrameworkApplicationView_Type : Cell<FrameworkApplicationView> { }

    public partial class FrameworkApplicationView_IsExist : Cell<FrameworkApplicationView> { }

    public partial class FrameworkApplicationView_IsActive : Cell<FrameworkApplicationView> { }

    [SqlTable("dbo", "FrameworkColumn")]
    public partial class FrameworkColumn : Row
    {
        [SqlColumn("Id", typeof(FrameworkColumn_Id))]
        public int Id { get; set; }

        [SqlColumn("TableId", typeof(FrameworkColumn_TableId))]
        public int TableId { get; set; }

        [SqlColumn("FieldNameSql", typeof(FrameworkColumn_FieldNameSql))]
        public string FieldNameSql { get; set; }

        [SqlColumn("FieldNameCSharp", typeof(FrameworkColumn_FieldNameCSharp))]
        public string FieldNameCSharp { get; set; }

        [SqlColumn("IsExist", typeof(FrameworkColumn_IsExist))]
        public bool? IsExist { get; set; }
    }

    public partial class FrameworkColumn_Id : Cell<FrameworkColumn> { }

    public partial class FrameworkColumn_TableId : Cell<FrameworkColumn> { }

    public partial class FrameworkColumn_FieldNameSql : Cell<FrameworkColumn> { }

    public partial class FrameworkColumn_FieldNameCSharp : Cell<FrameworkColumn> { }

    public partial class FrameworkColumn_IsExist : Cell<FrameworkColumn> { }

    [SqlTable("dbo", "FrameworkConfigColumn")]
    public partial class FrameworkConfigColumn : Row
    {
        [SqlColumn("Id", typeof(FrameworkConfigColumn_Id))]
        public int Id { get; set; }

        [SqlColumn("ColumnId", typeof(FrameworkConfigColumn_ColumnId))]
        public int ColumnId { get; set; }

        [SqlColumn("Text", typeof(FrameworkConfigColumn_Text))]
        public string Text { get; set; }

        [SqlColumn("IsVisible", typeof(FrameworkConfigColumn_IsVisible))]
        public bool? IsVisible { get; set; }

        [SqlColumn("Sort", typeof(FrameworkConfigColumn_Sort))]
        public double? Sort { get; set; }

        [SqlColumn("WidthPercent", typeof(FrameworkConfigColumn_WidthPercent))]
        public double? WidthPercent { get; set; }
    }

    public partial class FrameworkConfigColumn_Id : Cell<FrameworkConfigColumn> { }

    public partial class FrameworkConfigColumn_ColumnId : Cell<FrameworkConfigColumn> { }

    public partial class FrameworkConfigColumn_Text : Cell<FrameworkConfigColumn> { }

    public partial class FrameworkConfigColumn_IsVisible : Cell<FrameworkConfigColumn> { }

    public partial class FrameworkConfigColumn_Sort : Cell<FrameworkConfigColumn> { }

    public partial class FrameworkConfigColumn_WidthPercent : Cell<FrameworkConfigColumn> { }

    [SqlTable("dbo", "FrameworkConfigColumnView")]
    public partial class FrameworkConfigColumnView : Row
    {
        [SqlColumn("TableId", typeof(FrameworkConfigColumnView_TableId))]
        public int? TableId { get; set; }

        [SqlColumn("TableName", typeof(FrameworkConfigColumnView_TableName))]
        public string TableName { get; set; }

        [SqlColumn("TableIsExist", typeof(FrameworkConfigColumnView_TableIsExist))]
        public bool? TableIsExist { get; set; }

        [SqlColumn("ColumnId", typeof(FrameworkConfigColumnView_ColumnId))]
        public int ColumnId { get; set; }

        [SqlColumn("FieldNameSql", typeof(FrameworkConfigColumnView_FieldNameSql))]
        public string FieldNameSql { get; set; }

        [SqlColumn("FieldNameCSharp", typeof(FrameworkConfigColumnView_FieldNameCSharp))]
        public string FieldNameCSharp { get; set; }

        [SqlColumn("ColumnIsExist", typeof(FrameworkConfigColumnView_ColumnIsExist))]
        public bool? ColumnIsExist { get; set; }

        [SqlColumn("ConfigId", typeof(FrameworkConfigColumnView_ConfigId))]
        public int? ConfigId { get; set; }

        [SqlColumn("Text", typeof(FrameworkConfigColumnView_Text))]
        public string Text { get; set; }

        [SqlColumn("IsVisible", typeof(FrameworkConfigColumnView_IsVisible))]
        public bool? IsVisible { get; set; }

        [SqlColumn("Sort", typeof(FrameworkConfigColumnView_Sort))]
        public double? Sort { get; set; }

        [SqlColumn("WidthPercent", typeof(FrameworkConfigColumnView_WidthPercent))]
        public double? WidthPercent { get; set; }
    }

    public partial class FrameworkConfigColumnView_TableId : Cell<FrameworkConfigColumnView> { }

    public partial class FrameworkConfigColumnView_TableName : Cell<FrameworkConfigColumnView> { }

    public partial class FrameworkConfigColumnView_TableIsExist : Cell<FrameworkConfigColumnView> { }

    public partial class FrameworkConfigColumnView_ColumnId : Cell<FrameworkConfigColumnView> { }

    public partial class FrameworkConfigColumnView_FieldNameSql : Cell<FrameworkConfigColumnView> { }

    public partial class FrameworkConfigColumnView_FieldNameCSharp : Cell<FrameworkConfigColumnView> { }

    public partial class FrameworkConfigColumnView_ColumnIsExist : Cell<FrameworkConfigColumnView> { }

    public partial class FrameworkConfigColumnView_ConfigId : Cell<FrameworkConfigColumnView> { }

    public partial class FrameworkConfigColumnView_Text : Cell<FrameworkConfigColumnView> { }

    public partial class FrameworkConfigColumnView_IsVisible : Cell<FrameworkConfigColumnView> { }

    public partial class FrameworkConfigColumnView_Sort : Cell<FrameworkConfigColumnView> { }

    public partial class FrameworkConfigColumnView_WidthPercent : Cell<FrameworkConfigColumnView> { }

    [SqlTable("dbo", "FrameworkFileStorage")]
    public partial class FrameworkFileStorage : Row
    {
        [SqlColumn("Id", typeof(FrameworkFileStorage_Id))]
        public int Id { get; set; }

        [SqlColumn("ApplicationId", typeof(FrameworkFileStorage_ApplicationId))]
        public int? ApplicationId { get; set; }

        [SqlColumn("Name", typeof(FrameworkFileStorage_Name))]
        public string Name { get; set; }

        [SqlColumn("FileNameUpload", typeof(FrameworkFileStorage_FileNameUpload))]
        public string FileNameUpload { get; set; }

        [SqlColumn("Data", typeof(FrameworkFileStorage_Data))]
        public byte[] Data { get; set; }

        [SqlColumn("IsDelete", typeof(FrameworkFileStorage_IsDelete))]
        public bool? IsDelete { get; set; }
    }

    public partial class FrameworkFileStorage_Id : Cell<FrameworkFileStorage> { }

    public partial class FrameworkFileStorage_ApplicationId : Cell<FrameworkFileStorage> { }

    public partial class FrameworkFileStorage_Name : Cell<FrameworkFileStorage> { }

    public partial class FrameworkFileStorage_FileNameUpload : Cell<FrameworkFileStorage> { }

    public partial class FrameworkFileStorage_Data : Cell<FrameworkFileStorage> { }

    public partial class FrameworkFileStorage_IsDelete : Cell<FrameworkFileStorage> { }

    [SqlTable("dbo", "FrameworkSession")]
    public partial class FrameworkSession : Row
    {
        [SqlColumn("Id", typeof(FrameworkSession_Id))]
        public int Id { get; set; }

        [SqlColumn("Name", typeof(FrameworkSession_Name))]
        public Guid Name { get; set; }

        [SqlColumn("ApplicationId", typeof(FrameworkSession_ApplicationId))]
        public int ApplicationId { get; set; }
    }

    public partial class FrameworkSession_Id : Cell<FrameworkSession> { }

    public partial class FrameworkSession_Name : Cell<FrameworkSession> { }

    public partial class FrameworkSession_ApplicationId : Cell<FrameworkSession> { }

    [SqlTable("dbo", "FrameworkTable")]
    public partial class FrameworkTable : Row
    {
        [SqlColumn("Id", typeof(FrameworkTable_Id))]
        public int Id { get; set; }

        [SqlColumn("Name", typeof(FrameworkTable_Name))]
        public string Name { get; set; }

        [SqlColumn("IsExist", typeof(FrameworkTable_IsExist))]
        public bool? IsExist { get; set; }
    }

    public partial class FrameworkTable_Id : Cell<FrameworkTable> { }

    public partial class FrameworkTable_Name : Cell<FrameworkTable> { }

    public partial class FrameworkTable_IsExist : Cell<FrameworkTable> { }

    [SqlTable("dbo", "FrameworkVersion")]
    public partial class FrameworkVersion : Row
    {
        [SqlColumn("Id", typeof(FrameworkVersion_Id))]
        public int Id { get; set; }

        [SqlColumn("Name", typeof(FrameworkVersion_Name))]
        public string Name { get; set; }

        [SqlColumn("Version", typeof(FrameworkVersion_Version))]
        public string Version { get; set; }
    }

    public partial class FrameworkVersion_Id : Cell<FrameworkVersion> { }

    public partial class FrameworkVersion_Name : Cell<FrameworkVersion> { }

    public partial class FrameworkVersion_Version : Cell<FrameworkVersion> { }
}
