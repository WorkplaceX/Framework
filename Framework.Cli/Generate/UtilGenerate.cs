namespace Framework.Cli.Generate
{
    using Framework.DataAccessLayer;
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Text;

    /// <summary>
    /// Util functions for code generation.
    /// </summary>
    internal class UtilGenerate
    {
        /// <summary>
        /// Filter out special characters. Allow only characters and numbers.
        /// </summary>
        private static string NameCSharp(string name)
        {
            StringBuilder result = new StringBuilder();
            bool isFirst = true;
            foreach (char item in name)
            {
                if (item >= '0' && item <= '9')
                {
                    if (isFirst)
                    {
                        result.Append("_"); // If first char is a number 
                    }
                    result.Append(item);
                }
                char itemToUpper = char.ToUpper(item);
                if (itemToUpper >= 'A' && itemToUpper <= 'Z')
                {
                    result.Append(item);
                }
                isFirst = false;
            }
            return result.ToString();
        }

        /// <summary>
        /// Returns CSharp code compliant name.
        /// </summary>
        public static string NameCSharp(string name, List<string> nameExceptList)
        {
            var nameExceptListCopy = new List<string>(nameExceptList); // Do not modify list passed as parameter.
            for (int i = 0; i < nameExceptListCopy.Count; i++)
            {
                nameExceptListCopy[i] = NameCSharp(nameExceptListCopy[i]).ToUpper();
            }
            //
            name = NameCSharp(name);
            string result = name;
            int count = 1;
            while (nameExceptListCopy.Contains(result.ToUpper()))
            {
                count += 1;
                result = name + count;
            }
            nameExceptList.Add(name);
            return result;
        }

        /// <summary>
        /// Returns CSharp code.
        /// </summary>
        /// <param name="type">For example: "Int32"</param>
        /// <returns>Returns "int"</returns>
        private static string TypeToCSharpType(Type type)
        {
            if (type == typeof(Int32))
            {
                return "int";
            }
            if (type == typeof(String))
            {
                return "string";
            }
            if (type == typeof(Boolean))
            {
                return "bool";
            }
            if (type == typeof(Double))
            {
                return "double";
            }
            if (type == typeof(Byte[]))
            {
                return "byte[]";
            }
            return type.Name;
        }

        /// <summary>
        /// SqlType to CSharp code.
        /// </summary>
        public static string SqlTypeToCSharpType(int sqlType, bool isNullable)
        {
            Type type = UtilDalType.SqlTypeToType(sqlType);
            string result = TypeToCSharpType(type);
            if (type.IsValueType)
            {
                if (isNullable)
                {
                    result += "?";
                }
            }
            return result;
        }
    }
}
