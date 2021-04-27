using System.ComponentModel;
using System.IO;

namespace Framework.Cli
{
    /// <summary>
    /// Read text or binary data from blob files.
    /// Used only by generated file Application.Cli/Database/DatabaseIntegrate.cs
    /// </summary>
    public static class UtilCliBlob
    {
        /// <summary>
        /// Returns text from blob.
        /// </summary>
        public static string ReadText(string fileName)
        {
            var fileNameFull = UtilFramework.FolderName + "Application.Cli/Database/Blob/" + fileName;
            return File.ReadAllText(fileNameFull);
        }

        /// <summary>
        /// Returns binary data from blob.
        /// </summary>
        public static byte[] ReadData(string fileName)
        {
            var fileNameFull = UtilFramework.FolderName + "Application.Cli/Database/Blob/" + fileName;
            return File.ReadAllBytes(fileNameFull);
        }
    }
}
