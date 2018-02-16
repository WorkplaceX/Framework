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
                GridNameWithType gridNameWithType = new GridNameWithType(typeof(MyRow));
                UtilFramework.Assert(gridNameWithType.IsNameExclusive == false);
                gridNameWithType = new GridNameWithType(typeof(MyRow), "Grid1");
                UtilFramework.Assert(gridNameWithType.IsNameExclusive == false);
                gridNameWithType = new GridNameWithType(typeof(MyRow), "Grid1", true);
                UtilFramework.Assert(gridNameWithType.IsNameExclusive == true);
            }
            {
                GridName gridName = new GridName("Lookup");
                UtilFramework.Assert(gridName.IsNameExclusive == true);
                GridNameWithType gridNameWithType = new GridNameWithType(typeof(MyRow), gridName);
                UtilFramework.Assert(gridNameWithType.IsNameExclusive == true);
            }
            {
                GridName gridName = new GridNameWithType(typeof(MyRow), "Lookup");
                UtilFramework.Assert(gridName.IsNameExclusive == false);
                GridNameWithType gridNameWithType = new GridNameWithType(typeof(MyRow), gridName);
                UtilFramework.Assert(gridNameWithType.IsNameExclusive == false);
            }
            {
                GridName gridName = new GridName("D");
                UtilFramework.Assert(gridName.Name == "D");
                //
                GridNameWithType gridNameWithType = new GridNameWithType(typeof(MyRow), "S");
                UtilFramework.Assert(gridNameWithType.Name == "Calculated.MyRow.S");
                UtilFramework.Assert(gridNameWithType.TypeRow == typeof(MyRow));
                UtilFramework.Assert(gridNameWithType.IsNameExclusive == false);
                //
                gridNameWithType = new GridNameWithType(typeof(MyRow), "S", true);
                UtilFramework.Assert(gridNameWithType.Name == "S");
                UtilFramework.Assert(gridNameWithType.IsNameExclusive == true);
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