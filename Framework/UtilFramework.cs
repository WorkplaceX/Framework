using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Framework.Cli")] // Internal functions used by Framework.Cli assembly.

namespace Framework
{
    using Framework.Server;
    using Microsoft.ApplicationInsights.Extensibility;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Threading;

    internal class UtilFramework
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
        /// Returns root folder name. Does not throw an exception, if running on IIS server.
        /// </summary>
        /// <returns></returns>
        internal static string FolderNameGet()
        {
            Uri result = new Uri(typeof(UtilFramework).Assembly.CodeBase);
            result = new Uri(result, "../../../../");
            return result.AbsolutePath;
        }

        /// <summary>
        /// Gets FolderName. This is the root folder name. Throws exception if running on IIS server. See also method: UtilServer.FolderNameContentRoot();
        /// </summary>
        public static string FolderName
        {
            get
            {
                if (UtilServer.IsIssServer)
                {
                    throw new Exception("Running on ISS server! Use method UtilServer.FolderNameContentRoot();"); // Diferent folder structure! Use method: UtilServer.FolderNameContentRoot();
                }
                return FolderNameGet();
            }
        }

        public static string FolderNameParse(string folderName)
        {
            if (UtilFramework.StringNull(folderName) == null)
            {
                return null;
            }
            folderName = UtilFramework.StringEmpty(folderName);
            folderName = folderName.Replace(@"\", "/");
            if (folderName.StartsWith("/"))
            {
                folderName = folderName.Substring(1);
            }
            if (!folderName.EndsWith("/"))
            {
                folderName += "/";
            }
            return folderName;
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

        internal static string ExceptionToString(Exception exception)
        {
            string result = null;
            while (exception != null)
            {
                if (result != null)
                {
                    result += "; ";
                }
                result += exception.Message;
                exception = exception.InnerException;
            }
            return result;
        }

        /// <summary>
        /// Returns underlying tpye, if any. For example "type = typeof(int?)" returns "typeof(int)".
        /// </summary>
        internal static Type TypeUnderlying(Type type)
        {
            Type result = type;
            Type typeUnderlying = Nullable.GetUnderlyingType(type);
            if (typeUnderlying != null)
            {
                result = typeUnderlying;
            }
            return result;
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
            JsonSerializerSettings settings = new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Include };
            string json = JsonConvert.SerializeObject(config, isIndented? Formatting.Indented : Formatting.None, settings);
            return json;
        }

        internal static T ConfigLoad<T>(string fileName)
        {
            object result = null;
            string json = UtilFramework.FileLoad(fileName);
            result = ConfigFromJson<T>(json);
            return (T)result;
        }

        internal static void ConfigSave(object config, string fileName)
        {
            string json = ConfigToJson(config, isIndented: true);
            File.WriteAllText(fileName, json);

            Console.WriteLine(string.Format("Config saved to ({0})", fileName));
        }

        /// <summary>
        /// Returns null if value is empty string. Use for incoming and outgoing interfaces.
        /// </summary>
        internal static T StringNull<T>(T value)
        {
            T result = value;
            if (typeof(T) == typeof(string))
            {
                if (((string)(object)value) == "")
                {
                    result = (T)(object)null;
                }
            }
            return result;
        }

        /// <summary>
        /// Returns empty if value is null string. Use to get string length.
        /// </summary>
        internal static T StringEmpty<T>(T value)
        {
            T result = value;
            if (typeof(T) == typeof(string))
            {
                if (((string)(object)value) == null)
                {
                    result = (T)(object)"";
                }
            }
            return result;
        }

        internal static bool IsSubclassOf(Type type, Type typeBase)
        {
            if (type == null)
            {
                return false;
            }
            return type.GetTypeInfo().IsSubclassOf(typeBase) || type == typeBase;
        }

        /// <summary>
        /// Returns true for example for type "int?"
        /// </summary>
        internal static bool IsNullable(Type type)
        {
            if (type == typeof(string))
            {
                return true;
            }
            return Nullable.GetUnderlyingType(type) != null;
        }

        internal static string Replace(string text, string find, string replace)
        {
            UtilFramework.Assert(text.Contains(find));
            string result = text.Replace(find, replace);
            return result;
        }

        internal static string DateTimeToString(DateTime dateTime, bool isThousand = false)
        {
            string format = "yyyy-MM-dd HH:mm:ss";
            if (isThousand)
            {
                format += ".fff";
            }
            return dateTime.ToString(format);
        }

        /// <summary>
        /// See log in Visual Studio Output window.
        /// </summary>
        internal static void LogDebug(string text)
        {
            TelemetryConfiguration.Active.DisableTelemetry = true; // Disable "Application Insights Telemetry" logging
            Debug.WriteLine("### {0} {1}", UtilFramework.DateTimeToString(DateTime.Now, true), text);
        }

        /// <summary>
        /// Returns newly created instance of type with parameterless constructor.
        /// </summary>
        /// <param name="type">Type with parameterless constructor.</param>
        /// <returns>Returns instance of type.</returns>
        internal static object TypeToObject(Type type)
        {
            return Activator.CreateInstance(type);
        }

        internal static string FileLoad(string fileName)
        {
            return File.ReadAllText(fileName);
        }

        internal static void FileSave(string fileName, string text)
        {
            lock (typeof(object))
            {
                File.WriteAllText(fileName, text);
            }
        }

        internal static List<string> FileNameList(string folderName, string searchPattern)
        {
            return Directory.GetFiles(folderName, searchPattern, SearchOption.AllDirectories).ToList();
        }

        internal static List<string> FileNameList(string folderName)
        {
            return FileNameList(folderName, "*.*");
        }

        /// <summary>
        /// Returns for example: "Database.dbo.FrameworkScript"
        /// </summary>
        internal static string TypeToName(Type type)
        {
            return type.FullName;
        }

        internal static List<List<T>> Split<T>(List<T> list, int countMax)
        {
            List<List<T>> result = new List<List<T>>();
            for (int i = 0; i < list.Count; i++)
            {
                if (i % countMax == 0)
                {
                    result.Add(new List<T>());
                }
                result[result.Count - 1].Add(list[i]);
            }
            return result;
        }
    }
}
