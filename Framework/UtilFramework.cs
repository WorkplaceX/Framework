using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Framework.Cli")] // Internal functions used by Framework.Cli assembly.

namespace Framework
{
    using Framework.Server;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Text.Json;

    internal class UtilFramework
    {
        /// <summary>
        /// Gets Version. This is the framework version.
        /// </summary>
        public static string Version
        {
            get
            {
                // dotnet --version
                // 3.1.100

                // node --version
                // v12.13.0

                // npm --version
                // 6.12.0

                // npm run ng -- --version (Framework/Framework.Angular/application/)
                // Angular CLI: 8.3.15

                return "v3.17.03";
            }
        }

        /// <summary>
        /// Gets time and pc name of Ci build. Value is set during build process.
        /// </summary>
        public static string VersionBuild
        {
            get
            {
                // See also: method CommandBuild.BuildServer();
                return "Build (local)"; /* VersionBuild */
            }
        }

        /// <summary>
        /// Returns root folder name. Does not throw an exception, if running on IIS server.
        /// </summary>
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

        /// <summary>
        /// Combines FolderName and path.
        /// </summary>
        /// <param name="folderName">For example "Default/"</param>
        /// <param name="path">For example "/index.html"</param>
        /// <returns>Returns for example "Default/index.html"</returns>
        public static string FolderNameParse(string folderName, string path)
        {
            string result = FolderNameParse(folderName);
            path = UtilFramework.StringEmpty(path);
            if (path.StartsWith("/") || path.StartsWith("\""))
            {
                path = path.Substring(1);
            }
            result = result + path;
            return result;
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

        internal static void Assert(bool isAssert, string exceptionText)
        {
            if (!isAssert)
            {
                throw new Exception(exceptionText);
            }
        }

        internal static void Assert(bool isAssert)
        {
            if (!isAssert)
            {
                throw new Exception("Assert!");
            }
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
                result = JsonSerializer.Deserialize<T>("{}");
            }
            else
            {
                result = JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions { AllowTrailingCommas = true });
            }
            return (T)result;
        }

        internal static string ConfigToJson(object config, bool isIndented)
        {
            string json = JsonSerializer.Serialize(config, config.GetType(), new JsonSerializerOptions { WriteIndented = isIndented });
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

        /// <summary>
        /// Search 'find' in every line. If found replace line with 'replace'.
        /// </summary>
        /// <param name="text">Text file.</param>
        /// <param name="find">Text to find in line.</param>
        /// <param name="replace">Text to replace line with.</param>
        /// <returns>Returns modified text file.</returns>
        internal static string ReplaceLine(string text, string find, string replace)
        {
            bool isFind = false;
            StringBuilder result = new StringBuilder();
            using (var reader = new StringReader(text))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (line.Contains(find))
                    {
                        isFind = true;
                        line = replace;
                    }
                    result.AppendLine(line);
                }
            }
            UtilFramework.Assert(isFind, string.Format("Text not found! ({0})", find));
            return result.ToString();
        }

        /// <summary>
        /// Returns count of how many time 'find' has been found in 'text'.
        /// </summary>
        internal static int FindCount(string text, string find)
        {
            int result = 0;
            int index = 0;
            do
            {
                index = text.IndexOf(find, index);
                if (index != -1)
                {
                    result += 1;
                    index += find.Length;
                }
            } while (index != -1);
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
            // TelemetryConfiguration.Active.DisableTelemetry = true; // Disable "Application Insights Telemetry" logging // .NET Core throws "Could not load file or assembly Microsoft.ApplicationInsights" error. Not needed anymore.
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

        private static readonly ConcurrentDictionary<Type, string> typeToNameListCache = new ConcurrentDictionary<Type, string>();

        /// <summary>
        /// Returns for example: "Database.dbo.FrameworkScript"
        /// </summary>
        /// <param name="isIncludeAssemblyName">If true, function returns for example: "Database.dbo.FrameworkScript, Framework". Use this option if used in connection with <see cref="TypeFromName(string)"/></param>
        internal static string TypeToName(Type type, bool isIncludeAssemblyName = false)
        {
            if (isIncludeAssemblyName == false)
            {
                return type.FullName;
            }
            else
            {
                string result = typeToNameListCache.GetOrAdd(type, (Type type) =>
                {
                    return type.FullName + ", " + type.Assembly.GetName().Name; // Slow
                });
                return result;
            }
        }

        /// <summary>
        /// (TypeName, Type). Cache.
        /// </summary>
        private static readonly Dictionary<string, Type> typeFromNameListCache = new Dictionary<string, Type>();

        /// <summary>
        /// Returns type of for example Application.AppMain" or better "Application.AppMain, Application".
        /// </summary>
        public static Type TypeFromName(string typeName)
        {
            if (!typeFromNameListCache.ContainsKey(typeName))
            {
                Type type = Type.GetType(typeName);
                typeFromNameListCache.TryAdd(typeName, type);
            }

            Type result = typeFromNameListCache[typeName];
            UtilFramework.Assert(result != null, "TypeName unknown!");
            return result;
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
