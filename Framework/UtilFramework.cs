using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Framework.Cli")] // Internal functions used by Framework.Cli assembly.

namespace Framework
{
    using Newtonsoft.Json;
    using System;
    using System.IO;

    public class UtilFramework
    {
        public static string Version
        {
            get
            {
                // dotnet --version
                // 2.1.201

                // node --version
                // v8.11.3

                // npm --version
                // 6.2.0

                // ng --version
                // Angular CLI: 6.0.8

                return "v2.011";
            }
        }

        /// <summary>
        /// Gets time and pc name of Ci build.
        /// </summary>
        public static string VersionBuild
        {
            get
            {
                return "Build (local)"; // See also: method CommandBuild.BuildServer();
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
        internal static void ConsoleWriteLineColor(object value, ConsoleColor color)
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

        /// <summary>
        /// Write to stderr.
        /// </summary>
        /// <param name="value"></param>
        internal static void ConsoleWriteLineError(object value)
        {
            using (TextWriter textWriter = Console.Error)
            {
                textWriter.WriteLine(value);
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

        internal static T ConfigFromJson<T>(string json)
        {
            object result = null;
            if (json == null)
            {
                result = JsonConvert.DeserializeObject<T>("{}");
            }
            else
            {
                result = JsonConvert.DeserializeObject<T>(json);
            }
            return (T)result;
        }

        internal static string ConfigToJson(object config, bool isIndented)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore };
            string json = JsonConvert.SerializeObject(config, isIndented? Formatting.Indented : Formatting.None, settings);
            return json;
        }

        internal static T ConfigLoad<T>(string fileName, string fileNameDefault)
        {
            object result = null;
            if (!File.Exists(fileName))
            {
                string json = File.ReadAllText(fileNameDefault);
                result = ConfigFromJson<T>(json);
            }
            else
            {
                string json = File.ReadAllText(fileName);
                result = ConfigFromJson<T>(json);
            }
            return (T)result;
        }

        internal static void ConfigSave(object config, string fileName)
        {
            string json = ConfigToJson(config, isIndented: true);
            File.WriteAllText(fileName, json);

            Console.WriteLine(string.Format("Config saved to ({0})", fileName));
        }

        internal static string StringNull(string value)
        {
            if (value == "")
            {
                value = null;
            }
            return value;
        }

        internal static string Replace(string text, string find, string replace)
        {
            UtilFramework.Assert(text.Contains(find));
            string result = text.Replace(find, replace);
            return result;
        }
    }
}
