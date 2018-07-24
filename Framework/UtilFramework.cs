using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Framework.Cli")] // Internal functions used by Framework.Cli assembly.

namespace Framework
{
    using System;

    public class UtilFramework
    {
        /// <summary>
        /// Gets VersionServer.
        /// </summary>
        public static string VersionServer
        {
            get
            {
                // dotnet --version
                // 2.1.201
                return "v2.0 Server";
            }
        }

        /// <summary>
        /// Gets VersionClient. This is the expected client version.
        /// </summary>
        public static string VersionClient
        {
            get
            {
                // node --version
                // v8.11.3

                // npm --version
                // 6.2.0

                // ng --version
                // Angular CLI: 6.0.8
                return "v2.0 Client";
            }
        }

        /// <summary>
        /// Gets FolderName. This is the root folder name.
        /// </summary>
        public static string FolderName
        {
            get
            {
                Uri result = new Uri(typeof(UtilFramework).Assembly.CodeBase);
                result = new Uri(result, "../../../../");
                return result.AbsolutePath;
            }
        }

        /// <summary>
        /// Write to console in color.
        /// </summary>
        internal static void ConsoleWriteLine(object value, ConsoleColor color)
        {
            ConsoleColor foregroundColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            try
            {
                Console.WriteLine(value);
            }
            finally
            {
                Console.ForegroundColor = foregroundColor;
            }
        }

        internal static void Assert(bool isAssert, string exceptionText)
        {
            if (!isAssert)
            {
                throw new Exception(exceptionText);
            }
        }

        internal static void Assert(bool isAssert)
        {
            Assert(isAssert, "Assert!");
        }
    }
}
