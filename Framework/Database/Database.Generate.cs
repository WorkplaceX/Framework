namespace Database.dbo
{
    using Framework.DataAccessLayer;

    [SqlName("FrameworkFileStorage")]
    public partial class FrameworkFileStorage : Row
    {
        [SqlName("Id")]
        [TypeCell(typeof(FrameworkFileStorage_Id))]
        public int Id { get; set; }

        [SqlName("FileName")]
        [TypeCell(typeof(FrameworkFileStorage_FileName))]
        public string FileName { get; set; }

        [SqlName("Data")]
        [TypeCell(typeof(FrameworkFileStorage_Data))]
        public byte[] Data { get; set; }
    }

    public partial class FrameworkFileStorage_Id : Cell<FrameworkFileStorage> { }

    public partial class FrameworkFileStorage_FileName : Cell<FrameworkFileStorage> { }

    public partial class FrameworkFileStorage_Data : Cell<FrameworkFileStorage> { }
}
