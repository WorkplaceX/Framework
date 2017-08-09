namespace Framework.Application
{
    using System;

    public enum IndexEnum
    {
        None = 0,
        Index = 1,
        Filter = 2,
        New = 3,
        Total = 4
    }

    public static class UtilApplication
    {
        /// <summary>
        /// Returns TypeRowInAssembly. This is a type in an assembly. In this assembly search for Row classes when deserializing json. (For example: "Database.dbo.Airport")
        /// </summary>
        public static Type TypeRowInAssembly(App app)
        {
            return app.GetType();
        }

        public static string IndexEnumToText(IndexEnum indexEnum)
        {
            return indexEnum.ToString();
        }

        public static IndexEnum IndexEnumFromText(string index)
        {
            if (IndexEnumToText(IndexEnum.Filter) == index)
            {
                return IndexEnum.Filter;
            }
            if (IndexEnumToText(IndexEnum.New) == index)
            {
                return IndexEnum.New;
            }
            if (IndexEnumToText(IndexEnum.Total) == index)
            {
                return IndexEnum.Total;
            }
            int indexInt;
            if (int.TryParse(index, out indexInt))
            {
                return IndexEnum.Index;
            }
            return IndexEnum.None;
        }

        public static bool NamingConventionFieldNameSqlIsId(string fieldNameSql)
        {
            bool result = false;
            if (fieldNameSql != null)
            {
                int index = 0;
                while (index != -1)
                {
                    index = fieldNameSql.IndexOf("Id", index);
                    if (index != -1)
                    {
                        index += "Id".Length;
                        if (index < fieldNameSql.Length)
                        {
                            string text = fieldNameSql.Substring(index, 1);
                            if (text.ToUpper() == text)
                            {
                                result = true;
                                break;
                            }
                        }
                        else
                        {
                            result = true;
                            break;
                        }
                    }
                }
            }
            return result;
        }
    }
}
