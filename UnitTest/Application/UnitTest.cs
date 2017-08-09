namespace UnitTest.Application
{
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
    }
}