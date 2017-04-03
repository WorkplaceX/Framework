
namespace Office
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    internal static class Util
    {
        /// <summary>
        /// Throws exception if TextOld is not found.
        /// </summary>
        public static string Replace(string text, string textOld, string textNew)
        {
            Util.Assert(text.Contains(textOld));
            return text.Replace(textOld, textNew);
        }

        public static void Assert(bool value)
        {
            if (!value)
            {
                throw new Exception("Assert!");
            }
        }

        /// <summary>
        /// Returns local FileName list.
        /// </summary>
        public static List<string> FileNameList()
        {
            List<string> result = new List<string>();
            foreach (string fileName in Directory.EnumerateFiles(ConnectionManager.ExcelLocalFolder, "*.*", SearchOption.AllDirectories))
            {
                result.Add(fileName);
            }
            result.Sort();
            return result;
        }
    }
}
