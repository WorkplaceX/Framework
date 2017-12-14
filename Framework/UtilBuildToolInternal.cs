namespace Framework
{
    using Framework.DataAccessLayer;
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlClient;

    /// <summary>
    /// Util methods internally used by BuildTool.
    /// </summary>
    public static class UtilBuildToolInternal
    {
        public static class UtilDataAccessLayer
        {
            public static string Parameter(object value, SqlDbType dbType, List<SqlParameter> parameterList)
            {
                return DataAccessLayer.UtilDataAccessLayer.Parameter(value, dbType, parameterList);
            }

            public static string TypeRowToName(Type typeRow)
            {
                return DataAccessLayer.UtilDataAccessLayer.TypeRowToName(typeRow);
            }

            public static Type[] TypeRowList(Type typeRowInAssembly)
            {
                return DataAccessLayer.UtilDataAccessLayer.TypeRowList(typeRowInAssembly);
            }

            public static List<Cell> ColumnList(Type typeRow)
            {
                return DataAccessLayer.UtilDataAccessLayer.ColumnList(typeRow);
            }
        }
    }
}
