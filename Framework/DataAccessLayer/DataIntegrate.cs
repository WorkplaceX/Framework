namespace Framework.DataAccessLayer.Integrate
{
    using System;
    using System.Linq;

    /// <summary>
    /// Mapping from CSharp enum to value in sql table.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)] // Enum entry
    public class IdNameEnumAttribute : Attribute
    {
        public IdNameEnumAttribute(string idName)
        {
            this.IdName = idName;
        }

        /// <summary>
        /// Gets IdName. Value in sql table.
        /// </summary>
        public readonly string IdName;

        /// <summary>
        /// Returns IdName from enum.
        /// </summary>
        public static string IdNameFromEnum(Enum idEnum)
        {
            var enumType = idEnum.GetType();
            var enumName = Enum.GetName(enumType, idEnum);
            var enumNameAttribute = enumType.GetField(enumName).GetCustomAttributes(false).OfType<IdNameEnumAttribute>().Single();
            return enumNameAttribute.IdName;
        }

        /// <summary>
        /// Returns enum from IdName.
        /// </summary>
        public static TEnum IdNameToEnum<TEnum>(string idName) where TEnum : Enum
        {
            TEnum result = default(TEnum);
            foreach (var item in Enum.GetValues(typeof(TEnum)))
            {
                string idNameLocal = IdNameFromEnum((TEnum)item);
                if (idNameLocal == idName)
                {
                    result = (TEnum)item;
                    break;
                }
            }
            return result;
        }
    }
}
