namespace UnitTest.Application
{
    using Framework;
    using Framework.Application;
    using Framework.DataAccessLayer;

    public class UnitTest : UnitTestBase
    {
        public void FieldNameIsId()
        {
            UtilFramework.Assert(UtilApplication.ConfigFieldNameSqlIsId("") == false);
            UtilFramework.Assert(UtilApplication.ConfigFieldNameSqlIsId("Id") == true);
            UtilFramework.Assert(UtilApplication.ConfigFieldNameSqlIsId("IdX") == true);
            UtilFramework.Assert(UtilApplication.ConfigFieldNameSqlIsId("IdId") == true);
            UtilFramework.Assert(UtilApplication.ConfigFieldNameSqlIsId("Iden") == false);
            //
            UtilFramework.Assert(UtilApplication.ConfigFieldNameSqlIsId("xId") == true);
            UtilFramework.Assert(UtilApplication.ConfigFieldNameSqlIsId("xIdX") == true);
            UtilFramework.Assert(UtilApplication.ConfigFieldNameSqlIsId("xIdId") == true);
            UtilFramework.Assert(UtilApplication.ConfigFieldNameSqlIsId("xIden") == false);
            //
            UtilFramework.Assert(UtilApplication.ConfigFieldNameSqlIsId("Text") == false);
        }

        public void GridName()
        {
            {
                GridName gridName = new GridName("D");
                UtilFramework.Assert(gridName.IsNameExclusive == true);
                GridNameTypeRow gridNameTypeRow = new GridNameTypeRow(typeof(MyRow));
                UtilFramework.Assert(gridNameTypeRow.IsNameExclusive == false);
                gridNameTypeRow = new GridNameTypeRow(typeof(MyRow), "Grid1");
                UtilFramework.Assert(gridNameTypeRow.IsNameExclusive == false);
                gridNameTypeRow = new GridNameTypeRow(typeof(MyRow), "Grid1", true);
                UtilFramework.Assert(gridNameTypeRow.IsNameExclusive == true);
            }
            {
                GridName gridName = new GridName("Lookup");
                UtilFramework.Assert(gridName.IsNameExclusive == true);
                GridNameTypeRow gridNameTypeRow = new GridNameTypeRow(typeof(MyRow), gridName);
                UtilFramework.Assert(gridNameTypeRow.IsNameExclusive == true);
            }
            {
                GridName gridName = new GridNameTypeRow(typeof(MyRow), "Lookup");
                UtilFramework.Assert(gridName.IsNameExclusive == false);
                GridNameTypeRow gridNameTypeRow = new GridNameTypeRow(typeof(MyRow), gridName);
                UtilFramework.Assert(gridNameTypeRow.IsNameExclusive == false);
            }
        }
    }

    public class MyRow : Row
    {
        public string Text { get; set; }
    }
}