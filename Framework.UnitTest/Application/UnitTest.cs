namespace UnitTest.Application
{
    using Database.Calculated;
    using Framework;
    using Framework.Application;

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

        public void GridName2()
        {
            GridName2 gridName = new GridName2("D");
            UtilFramework.Assert(gridName.Name == "D");
            //
            GridNameTypeRow2 gridNameTypeRow = new GridNameTypeRow2("S", typeof(MyRow));
            UtilFramework.Assert(gridNameTypeRow.Name == "S");
            UtilFramework.Assert(gridNameTypeRow.TypeRow == typeof(MyRow));
            UtilFramework.Assert(gridNameTypeRow.IsNameNotCombine == false);
            //
            gridNameTypeRow = new GridNameTypeRow2("S", typeof(MyRow), true);
            UtilFramework.Assert(gridNameTypeRow.Name == "S");
            UtilFramework.Assert(gridNameTypeRow.IsNameNotCombine == true);
        }
    }
}

namespace Database.Calculated
{
    using Framework.DataAccessLayer;

    public class MyRow : Row
    {
        public string Text { get; set; }
    }
}