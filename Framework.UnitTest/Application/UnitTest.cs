namespace UnitTest.Application
{
    using Database.Calculated;
    using Framework;
    using Framework.Application;
    using System;

    public class UnitTest : UnitTestBase
    {
        public void ColumnNameIsId()
        {
            UtilFramework.Assert(UtilApplication.ConfigColumnNameSqlIsId("") == false);
            UtilFramework.Assert(UtilApplication.ConfigColumnNameSqlIsId("Id") == true);
            UtilFramework.Assert(UtilApplication.ConfigColumnNameSqlIsId("IdX") == true);
            UtilFramework.Assert(UtilApplication.ConfigColumnNameSqlIsId("IdId") == true);
            UtilFramework.Assert(UtilApplication.ConfigColumnNameSqlIsId("Iden") == false);
            //
            UtilFramework.Assert(UtilApplication.ConfigColumnNameSqlIsId("xId") == true);
            UtilFramework.Assert(UtilApplication.ConfigColumnNameSqlIsId("xIdX") == true);
            UtilFramework.Assert(UtilApplication.ConfigColumnNameSqlIsId("xIdId") == true);
            UtilFramework.Assert(UtilApplication.ConfigColumnNameSqlIsId("xIden") == false);
            //
            UtilFramework.Assert(UtilApplication.ConfigColumnNameSqlIsId("Text") == false);
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
            {
                GridName gridName = new GridName("D");
                UtilFramework.Assert(gridName.Name == "D");
                //
                GridNameTypeRow gridNameTypeRow = new GridNameTypeRow(typeof(MyRow), "S");
                UtilFramework.Assert(gridNameTypeRow.Name == "Calculated.MyRow.S");
                UtilFramework.Assert(gridNameTypeRow.TypeRow == typeof(MyRow));
                UtilFramework.Assert(gridNameTypeRow.IsNameExclusive == false);
                //
                gridNameTypeRow = new GridNameTypeRow(typeof(MyRow), "S", true);
                UtilFramework.Assert(gridNameTypeRow.Name == "S");
                UtilFramework.Assert(gridNameTypeRow.IsNameExclusive == true);
            }
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