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
