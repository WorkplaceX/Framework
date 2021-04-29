using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;

namespace Framework.Cli
{
    /// <summary>
    /// Cli relevant functions.
    /// </summary>
    public static class UtilCli
    {
        /// <summary>
        /// Returns for example "Application.Doc.AppMain, Application" which can be used in file ConfigCli.json
        /// </summary>
        public static string AppTypeName(Type appType)
        {
            return appType.FullName + ", " + appType.Assembly.GetName().Name;
        }

        /// <summary>
        /// Gets FolderName. This is the root folder where file Application.sln is located.
        /// </summary>
        public static string FolderName
        {
            get
            {
                return UtilFramework.FolderName;
            }
        }

        /// <summary>
        /// Returns FileNameFull for blob file. Searches also in ExternalGit.
        /// </summary>
        private static string BlobReadFileNameFull(string fileName)
        {
            string result = null;

            var list = new List<string>();
            list.Add(UtilFramework.FolderName + "Application.Cli/Database/Blob/" + fileName);
            foreach (var item in Config.ConfigCli.Load().ExternalGitList)
            {
                list.Add(UtilFramework.FolderName + "Application.Cli/Database/ExternalGit/" + item.ExternalProjectName + "/Blob/" + fileName);
            }
            int count = 0;
            foreach (var item in list)
            {
                if (File.Exists(item))
                {
                    result = item;
                    count += 1;
                }
            }

            // Assert found once
            UtilFramework.Assert(count == 1, string.Format("File not found or multiple times! ({0})", fileName));

            return result;
        }
    

        /// <summary>
        /// Returns text from blob.
        /// Used only by generated file Application.Cli/Database/DatabaseIntegrate.cs
        /// </summary>
        public static string BlobReadText(string fileName)
        {
            var fileNameFull = BlobReadFileNameFull(fileName);
            return File.ReadAllText(fileNameFull);
        }

        /// <summary>
        /// Returns binary data from blob.
        /// Used only by generated file Application.Cli/Database/DatabaseIntegrate.cs
        /// </summary>
        public static byte[] BlobReadData(string fileName)
        {
            var fileNameFull = BlobReadFileNameFull(fileName);
            return File.ReadAllBytes(fileNameFull);
        }
    }
}
