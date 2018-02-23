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
                GridNameType gridNameType = new GridNameType(typeof(MyRowCalc));
                UtilFramework.Assert(gridNameType.IsNameExclusive == false);
                gridNameType = new GridNameType(typeof(MyRowCalc), "Grid1");
                UtilFramework.Assert(gridNameType.IsNameExclusive == false);
                gridNameType = new GridNameType(typeof(MyRowCalc), "Grid1", true);
                UtilFramework.Assert(gridNameType.IsNameExclusive == true);
            }
            {
                GridName gridName = new GridName("Lookup");
                UtilFramework.Assert(gridName.IsNameExclusive == true);
                GridNameType gridNameType = new GridNameType(typeof(MyRowCalc), gridName);
                UtilFramework.Assert(gridNameType.IsNameExclusive == true);
            }
            {
                GridName gridName = new GridNameType(typeof(MyRowCalc), "Lookup");
                UtilFramework.Assert(gridName.IsNameExclusive == false);
                GridNameType gridNameType = new GridNameType(typeof(MyRowCalc), gridName);
                UtilFramework.Assert(gridNameType.IsNameExclusive == false);
            }
            {
                GridName gridName = new GridName("D");
                UtilFramework.Assert(gridName.Name == "D");
                //
                GridNameType gridNameType = new GridNameType(typeof(MyRowCalc), "S");
                UtilFramework.Assert(gridNameType.Name == "Calculated.MyRowCalc.S");
                UtilFramework.Assert(gridNameType.TypeRow == typeof(MyRowCalc));
                UtilFramework.Assert(gridNameType.IsNameExclusive == false);
                //
                gridNameType = new GridNameType(typeof(MyRowCalc), "S", true);
                UtilFramework.Assert(gridNameType.Name == "S");
                UtilFramework.Assert(gridNameType.IsNameExclusive == true);
            }
        }
    }
}

namespace Database.Calculated
{
    using Framework.DataAccessLayer;

    public class MyRowCalc : Row
    {
        public string Text { get; set; }
    }
}