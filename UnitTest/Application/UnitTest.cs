namespace UnitTest.Application
{
    using Framework;
    using Framework.Application;

    public class UnitTest : UnitTestBase
    {
        public void FieldNameIsId()
        {
            UtilFramework.Assert(UtilApplication.NamingConventionFieldNameSqlIsId("") == false);
            UtilFramework.Assert(UtilApplication.NamingConventionFieldNameSqlIsId("Id") == true);
            UtilFramework.Assert(UtilApplication.NamingConventionFieldNameSqlIsId("IdX") == true);
            UtilFramework.Assert(UtilApplication.NamingConventionFieldNameSqlIsId("IdId") == true);
            UtilFramework.Assert(UtilApplication.NamingConventionFieldNameSqlIsId("Iden") == false);
            //
            UtilFramework.Assert(UtilApplication.NamingConventionFieldNameSqlIsId("xId") == true);
            UtilFramework.Assert(UtilApplication.NamingConventionFieldNameSqlIsId("xIdX") == true);
            UtilFramework.Assert(UtilApplication.NamingConventionFieldNameSqlIsId("xIdId") == true);
            UtilFramework.Assert(UtilApplication.NamingConventionFieldNameSqlIsId("xIden") == false);
            //
            UtilFramework.Assert(UtilApplication.NamingConventionFieldNameSqlIsId("Text") == false);
        }
    }
}