using Framework.Server.Application.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Framework.Server.Application
{
    public enum IndexEnum
    {
        None = 0,
        Index = 1,
        Filter = 2,
        New = 3,
        Total = 4
    }

    public static class Util
    {
        internal static PageJson PageJson(ApplicationJson applicationJson, string typeNamePageServer)
        {
            if (!applicationJson.PageJsonList.ContainsKey(typeNamePageServer))
            {
                applicationJson.PageJsonList[typeNamePageServer] = (PageJson)Framework.Util.TypeToObject(typeof(PageJson));
            }
            return (PageJson)applicationJson.PageJsonList[typeNamePageServer];
        }

        public static string IndexEnumToString(IndexEnum indexEnum)
        {
            return indexEnum.ToString();
        }

        public static IndexEnum IndexToIndexEnum(string index)
        {
            if (IndexEnumToString(IndexEnum.Filter) == index)
            {
                return IndexEnum.Filter;
            }
            if (IndexEnumToString(IndexEnum.New) == index)
            {
                return IndexEnum.New;
            }
            if (IndexEnumToString(IndexEnum.Total) == index)
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
    }
}
