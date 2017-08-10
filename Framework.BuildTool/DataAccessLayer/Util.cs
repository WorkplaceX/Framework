﻿namespace Framework.BuildTool.DataAccessLayer
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using System.Text;

    public class Util
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
            switch (sqlType)
            {
                case 56:
                    type = typeof(int);
                    break;
                case 36:
                    type = typeof(Guid);
                    break;
                case 61:
                    type = typeof(DateTime);
                    break;
                case 40:
                    type = typeof(DateTime);
                    break;
                case 231:
                    type = typeof(string);
                    break;
                case 104:
                    type = typeof(bool);
                    break;
                case 62:
                    type = typeof(double);
                    break;
                case 165:
                    type = typeof(byte[]);
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
