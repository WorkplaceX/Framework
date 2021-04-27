using System;
using System.ComponentModel;
using System.IO;

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
        /// Returns text from blob.
        /// Used only by generated file Application.Cli/Database/DatabaseIntegrate.cs
        /// </summary>
        public static string BlobReadText(string fileName)
        {
            var fileNameFull = UtilFramework.FolderName + "Application.Cli/Database/Blob/" + fileName;
            return File.ReadAllText(fileNameFull);
        }

        /// <summary>
        /// Returns binary data from blob.
        /// Used only by generated file Application.Cli/Database/DatabaseIntegrate.cs
        /// </summary>
        public static byte[] BlobReadData(string fileName)
        {
            var fileNameFull = UtilFramework.FolderName + "Application.Cli/Database/Blob/" + fileName;
            return File.ReadAllBytes(fileNameFull);
        }
    }
}
