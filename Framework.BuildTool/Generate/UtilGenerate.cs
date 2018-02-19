namespace Framework.BuildTool.DataAccessLayer
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using System.Text;
    
    /// <summary>
    /// Util functions for code generation.
    /// </summary>
    public class UtilGenerate
    {
        public static string FileLoad(string fileName)
        {
            return File.ReadAllText(fileName);
        }

        public static void FileSave(string fileName, string text)
        {
            File.WriteAllText(fileName, text);
        }

        /// <summary>
        /// Filter out special characters. Allow only characters and numbers.
        /// </summary>
        private static string NameCSharp(string name)
        {
            StringBuilder result = new StringBuilder();
            foreach (char item in name)
            {
                if (item >= '0' && item <= '9')
                {
                    result.Append(item);
                }
                char itemToUpper = char.ToUpper(item);
                if (itemToUpper >= 'A' && itemToUpper <= 'Z')
                {
                    result.Append(item);
                }
            }
            return result.ToString();
        }

        /// <summary>
        /// Return CSharp code compliant name.
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

        private static void SqlTypeToType(int sqlType, out Type type)
        {
            // See also: https://docs.microsoft.com/en-us/dotnet/framework/data/adonet/sql-server-data-type-mappings
            // See also: SELECT * FROM sys.types
            // See also: https://docs.microsoft.com/en-us/sql/t-sql/data-types/data-type-conversion-database-engine
            switch (sqlType)
            {
                case 56: // int
                    type = typeof(Int32);
                    break;
                case 52: // smallint
                    type = typeof(Int16);
                    break;
                case 48: // tinyint
                    type = typeof(byte);
                    break;
                case 127: // bigint
                    type = typeof(Int64);
                    break;
                case 36: // uniqueidentifier
                    type = typeof(Guid);
                    break;
                case 61: // datetime
                    type = typeof(DateTime);
                    break;
                case 42: // datetime2
                    type = typeof(DateTime);
                    break;
                case 40: // date
                    type = typeof(DateTime);
                    break;
                case 175: // char
                    type = typeof(string);
                    break;
                case 231: // nvarcahr
                    type = typeof(string);
                    break;
                case 167: // varchar
                    type = typeof(string);
                    break;
                case 35: // text // See also: https://stackoverflow.com/questions/564755/sql-server-text-type-vs-varchar-data-type
                    type = typeof(string);
                    break;
                case 99: // ntext
                    type = typeof(string);
                    break;
                case 104: // bit
                    type = typeof(bool);
                    break;
                case 60: // money
                    type = typeof(decimal);
                    break;
                case 106: // decimal
                    type = typeof(decimal);
                    break;
                case 59: // real
                    type = typeof(Single);
                    break;
                case 62: // float
                    type = typeof(double);
                    break;
                case 165: // varbinary
                    type = typeof(byte[]);
                    break;
                case 98: // sql_variant
                    type = typeof(object);
                    break;
                case 34: // image
                    type = typeof(byte[]);
                    break;
                case 108: // numeric
                    type = typeof(decimal);
                    break;
                default:
                    throw new Exception("Type unknown!");
            }
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
            Type type;
            SqlTypeToType(sqlType, out type);
            string result = TypeToCSharpType(type);
            if (type.GetTypeInfo().IsValueType)
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
