namespace Database.dbo
{
    using Framework.DataAccessLayer;
    using System;

    [SqlTable("dbo", "FrameworkApplication")]
    public partial class FrameworkApplication : Row
    {
        [SqlName("Id")]
        [TypeCell(typeof(FrameworkApplication_Id))]
        public int Id { get; set; }

        [SqlName("Name")]
        [TypeCell(typeof(FrameworkApplication_Name))]
        public string Name { get; set; }

        [SqlName("Path")]
        [TypeCell(typeof(FrameworkApplication_Path))]
        public string Path { get; set; }

        [SqlName("ApplicationTypeId")]
        [TypeCell(typeof(FrameworkApplication_ApplicationTypeId))]
        public int ApplicationTypeId { get; set; }

        [SqlName("IsActive")]
        [TypeCell(typeof(FrameworkApplication_IsActive))]
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
        [SqlName("Id")]
        [TypeCell(typeof(FrameworkApplicationType_Id))]
        public int Id { get; set; }

        [SqlName("Name")]
        [TypeCell(typeof(FrameworkApplicationType_Name))]
        public string Name { get; set; }

        [SqlName("IsExist")]
        [TypeCell(typeof(FrameworkApplicationType_IsExist))]
        public bool? IsExist { get; set; }
    }

    public partial class FrameworkApplicationType_Id : Cell<FrameworkApplicationType> { }

    public partial class FrameworkApplicationType_Name : Cell<FrameworkApplicationType> { }

    public partial class FrameworkApplicationType_IsExist : Cell<FrameworkApplicationType> { }

    [SqlTable("dbo", "FrameworkApplicationView")]
    public partial class FrameworkApplicationView : Row
    {
        [SqlName("Id")]
        [TypeCell(typeof(FrameworkApplicationView_Id))]
        public int Id { get; set; }

        [SqlName("Name")]
        [TypeCell(typeof(FrameworkApplicationView_Name))]
        public string Name { get; set; }

        [SqlName("Path")]
        [TypeCell(typeof(FrameworkApplicationView_Path))]
        public string Path { get; set; }

        [SqlName("ApplicationTypeId")]
        [TypeCell(typeof(FrameworkApplicationView_ApplicationTypeId))]
        public int ApplicationTypeId { get; set; }

        [SqlName("Type")]
        [TypeCell(typeof(FrameworkApplicationView_Type))]
        public string Type { get; set; }

        [SqlName("IsExist")]
        [TypeCell(typeof(FrameworkApplicationView_IsExist))]
        public bool? IsExist { get; set; }

        [SqlName("IsActive")]
        [TypeCell(typeof(FrameworkApplicationView_IsActive))]
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
        [SqlName("Id")]
        [TypeCell(typeof(FrameworkColumn_Id))]
        public int Id { get; set; }

        [SqlName("TableId")]
        [TypeCell(typeof(FrameworkColumn_TableId))]
        public int TableId { get; set; }

        [SqlName("FieldNameSql")]
        [TypeCell(typeof(FrameworkColumn_FieldNameSql))]
        public string FieldNameSql { get; set; }

        [SqlName("FieldNameCSharp")]
        [TypeCell(typeof(FrameworkColumn_FieldNameCSharp))]
        public string FieldNameCSharp { get; set; }

        [SqlName("IsExist")]
        [TypeCell(typeof(FrameworkColumn_IsExist))]
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
        [SqlName("Id")]
        [TypeCell(typeof(FrameworkConfigColumn_Id))]
        public int Id { get; set; }

        [SqlName("ColumnId")]
        [TypeCell(typeof(FrameworkConfigColumn_ColumnId))]
        public int ColumnId { get; set; }

        [SqlName("Text")]
        [TypeCell(typeof(FrameworkConfigColumn_Text))]
        public string Text { get; set; }

        [SqlName("IsVisible")]
        [TypeCell(typeof(FrameworkConfigColumn_IsVisible))]
        public bool? IsVisible { get; set; }

        [SqlName("Sort")]
        [TypeCell(typeof(FrameworkConfigColumn_Sort))]
        public double? Sort { get; set; }

        [SqlName("WidthPercent")]
        [TypeCell(typeof(FrameworkConfigColumn_WidthPercent))]
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
        [SqlName("TableId")]
        [TypeCell(typeof(FrameworkConfigColumnView_TableId))]
        public int? TableId { get; set; }

        [SqlName("TableName")]
        [TypeCell(typeof(FrameworkConfigColumnView_TableName))]
        public string TableName { get; set; }

        [SqlName("TableIsExist")]
        [TypeCell(typeof(FrameworkConfigColumnView_TableIsExist))]
        public bool? TableIsExist { get; set; }

        [SqlName("ColumnId")]
        [TypeCell(typeof(FrameworkConfigColumnView_ColumnId))]
        public int ColumnId { get; set; }

        [SqlName("FieldNameSql")]
        [TypeCell(typeof(FrameworkConfigColumnView_FieldNameSql))]
        public string FieldNameSql { get; set; }

        [SqlName("FieldNameCSharp")]
        [TypeCell(typeof(FrameworkConfigColumnView_FieldNameCSharp))]
        public string FieldNameCSharp { get; set; }

        [SqlName("ColumnIsExist")]
        [TypeCell(typeof(FrameworkConfigColumnView_ColumnIsExist))]
        public bool? ColumnIsExist { get; set; }

        [SqlName("ConfigId")]
        [TypeCell(typeof(FrameworkConfigColumnView_ConfigId))]
        public int? ConfigId { get; set; }

        [SqlName("Text")]
        [TypeCell(typeof(FrameworkConfigColumnView_Text))]
        public string Text { get; set; }

        [SqlName("IsVisible")]
        [TypeCell(typeof(FrameworkConfigColumnView_IsVisible))]
        public bool? IsVisible { get; set; }

        [SqlName("Sort")]
        [TypeCell(typeof(FrameworkConfigColumnView_Sort))]
        public double? Sort { get; set; }

        [SqlName("WidthPercent")]
        [TypeCell(typeof(FrameworkConfigColumnView_WidthPercent))]
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
        [SqlName("Id")]
        [TypeCell(typeof(FrameworkFileStorage_Id))]
        public int Id { get; set; }

        [SqlName("ApplicationId")]
        [TypeCell(typeof(FrameworkFileStorage_ApplicationId))]
        public int? ApplicationId { get; set; }

        [SqlName("Name")]
        [TypeCell(typeof(FrameworkFileStorage_Name))]
        public string Name { get; set; }

        [SqlName("FileNameUpload")]
        [TypeCell(typeof(FrameworkFileStorage_FileNameUpload))]
        public string FileNameUpload { get; set; }

        [SqlName("Data")]
        [TypeCell(typeof(FrameworkFileStorage_Data))]
        public byte[] Data { get; set; }

        [SqlName("IsDelete")]
        [TypeCell(typeof(FrameworkFileStorage_IsDelete))]
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
        [SqlName("Id")]
        [TypeCell(typeof(FrameworkSession_Id))]
        public int Id { get; set; }

        [SqlName("Name")]
        [TypeCell(typeof(FrameworkSession_Name))]
        public Guid Name { get; set; }

        [SqlName("ApplicationId")]
        [TypeCell(typeof(FrameworkSession_ApplicationId))]
        public int ApplicationId { get; set; }
    }

    public partial class FrameworkSession_Id : Cell<FrameworkSession> { }

    public partial class FrameworkSession_Name : Cell<FrameworkSession> { }

    public partial class FrameworkSession_ApplicationId : Cell<FrameworkSession> { }

    [SqlTable("dbo", "FrameworkTable")]
    public partial class FrameworkTable : Row
    {
        [SqlName("Id")]
        [TypeCell(typeof(FrameworkTable_Id))]
        public int Id { get; set; }

        [SqlName("Name")]
        [TypeCell(typeof(FrameworkTable_Name))]
        public string Name { get; set; }

        [SqlName("IsExist")]
        [TypeCell(typeof(FrameworkTable_IsExist))]
        public bool? IsExist { get; set; }
    }

    public partial class FrameworkTable_Id : Cell<FrameworkTable> { }

    public partial class FrameworkTable_Name : Cell<FrameworkTable> { }

    public partial class FrameworkTable_IsExist : Cell<FrameworkTable> { }

    [SqlTable("dbo", "FrameworkVersion")]
    public partial class FrameworkVersion : Row
    {
        [SqlName("Id")]
        [TypeCell(typeof(FrameworkVersion_Id))]
        public int Id { get; set; }

        [SqlName("Name")]
        [TypeCell(typeof(FrameworkVersion_Name))]
        public string Name { get; set; }

        [SqlName("Version")]
        [TypeCell(typeof(FrameworkVersion_Version))]
        public string Version { get; set; }
    }

    public partial class FrameworkVersion_Id : Cell<FrameworkVersion> { }

    public partial class FrameworkVersion_Name : Cell<FrameworkVersion> { }

    public partial class FrameworkVersion_Version : Cell<FrameworkVersion> { }
}
