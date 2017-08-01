namespace Database.dbo
{
    using Framework.DataAccessLayer;
    using System;

    [SqlName("FrameworkApplication")]
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

    [SqlName("FrameworkApplicationType")]
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

    [SqlName("FrameworkApplicationView")]
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

    public partial class FrameworkApplicationView_Type : Cell<FrameworkApplicationView> { }

    public partial class FrameworkApplicationView_IsExist : Cell<FrameworkApplicationView> { }

    public partial class FrameworkApplicationView_IsActive : Cell<FrameworkApplicationView> { }

    [SqlName("FrameworkColumn")]
    public partial class FrameworkColumn : Row
    {
        [SqlName("Id")]
        [TypeCell(typeof(FrameworkColumn_Id))]
        public int Id { get; set; }

        [SqlName("TableNameSql")]
        [TypeCell(typeof(FrameworkColumn_TableNameSql))]
        public string TableNameSql { get; set; }

        [SqlName("FieldNameSql")]
        [TypeCell(typeof(FrameworkColumn_FieldNameSql))]
        public string FieldNameSql { get; set; }

        [SqlName("FieldNameCsharp")]
        [TypeCell(typeof(FrameworkColumn_FieldNameCsharp))]
        public string FieldNameCsharp { get; set; }

        [SqlName("IsExist")]
        [TypeCell(typeof(FrameworkColumn_IsExist))]
        public bool? IsExist { get; set; }
    }

    public partial class FrameworkColumn_Id : Cell<FrameworkColumn> { }

    public partial class FrameworkColumn_TableNameSql : Cell<FrameworkColumn> { }

    public partial class FrameworkColumn_FieldNameSql : Cell<FrameworkColumn> { }

    public partial class FrameworkColumn_FieldNameCsharp : Cell<FrameworkColumn> { }

    public partial class FrameworkColumn_IsExist : Cell<FrameworkColumn> { }

    [SqlName("FrameworkFileStorage")]
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

    [SqlName("FrameworkSession")]
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

    [SqlName("FrameworkVersion")]
    public partial class FrameworkVersion : Row
    {
        [SqlName("Id")]
        [TypeCell(typeof(FrameworkVersion_Id))]
        public int Id { get; set; }

        [SqlName("Version")]
        [TypeCell(typeof(FrameworkVersion_Version))]
        public string Version { get; set; }
    }

    public partial class FrameworkVersion_Id : Cell<FrameworkVersion> { }

    public partial class FrameworkVersion_Version : Cell<FrameworkVersion> { }
}
