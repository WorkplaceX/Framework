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
        /// Returns TypeRowInAssembly. This is a type in an assembly. Search for row class in this assembly when deserializing json. (For example: "dbo.Airport")
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

        /// <summary>
        /// Returns true, if column name contains "Id" according default naming convention.
        /// </summary>
        public static bool ConfigFieldNameSqlIsId(string fieldNameSql)
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
