using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Framework.BuildTool")] // Not public but used by other Framework assembly.
[assembly: InternalsVisibleTo("Framework.UnitTest")] // Access internal methods by UnitTest.

namespace Framework
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Reflection;
    using System.Threading;

    public static class UtilFramework
    {
        public static string VersionServer
        {
            get
            {
                // .NET Core 2.0
                // node 8.9.2 LTS
                // npm 5.5.1
                return "v1.047 Server";
            }
        }

        public static void Assert(bool isAssert, string exceptionText)
        {
            if (!isAssert)
            {
                throw new Exception(exceptionText);
            }
        }

        public static void Assert(bool isAssert)
        {
            Assert(isAssert, "Assert!");
        }

        private static void FolderNamePrivate(out string folderName, out bool isIss)
        {
            Uri uri = new Uri(typeof(UtilFramework).GetTypeInfo().Assembly.CodeBase);
            string result;
            isIss = false;
            if (uri.AbsolutePath.EndsWith("/Build/bin/Debug/netcoreapp2.0/Framework.dll") || uri.AbsolutePath.EndsWith("/BuildTool/bin/Debug/netcoreapp2.0/Framework.dll")) // Running in Visual Studio
            {
                result = new Uri(uri, "../../../../").AbsolutePath;
            }
            else
            {
                if (uri.AbsolutePath.EndsWith("Server/bin/Debug/netcoreapp2.0/Framework.dll")) // Running in Visual Studio
                {
                    result = new Uri(uri, "../../../../").AbsolutePath;
                }
                else
                {
                    if (uri.AbsolutePath.EndsWith("Framework.dll")) // On IIS
                    {
                        result = new Uri(uri, "./").AbsolutePath;
                        isIss = true;
                    }
                    else
                    {
                        throw new Exception("FileName unknown!");
                    }
                }
            }
            folderName = result;
        }

        /// <summary>
        /// Gets root FolderName.
        /// </summary>
        public static string FolderName
        {
            get
            {
                string folderName;
                bool isIss;
                FolderNamePrivate(out folderName, out isIss);
                return folderName;
            }
        }

        /// <summary>
        /// Gets IsLinux. True, if running for example on Ubuntu.
        /// </summary>
        public static bool IsLinux
        {
            get
            {
                return FolderName.StartsWith("/");
            }
        }

        /// <summary>
        /// Gets FolderNameIsIss. True, if running on ISS server.
        /// </summary>
        public static bool FolderNameIsIss
        {
            get
            {
                string folderName;
                bool isIss;
                FolderNamePrivate(out folderName, out isIss);
                return isIss;
            }
        }

        public static string FileRead(string fileName)
        {
            return File.ReadAllText(fileName);
        }

        public static void FileWrite(string fileName, string value)
        {
            lock (typeof(object))
            {
                File.WriteAllText(fileName, value);
            }
        }

        public static string[] FileNameList(string folderName, string searchPattern)
        {
            var list = Directory.GetFiles(folderName, searchPattern, SearchOption.AllDirectories);
            var result = list.Select(item => item.Replace(@"\", "/")).ToArray();
            return result;
        }

        public static string[] FileNameList(string folderName)
        {
            return FileNameList(folderName, "*.*");
        }

        /// <summary>
        /// Returns external ip address.
        /// </summary>
        public static string Ip()
        {
            string result = null;
            try
            {
                HttpClient client = new HttpClient();
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://bot.whatismyipaddress.com/");
                var taskSend = client.SendAsync(request);
                taskSend.Wait();
                var taskRead = taskSend.Result.Content.ReadAsStringAsync();
                taskRead.Wait();
                result = taskRead.Result;
            }
            catch (Exception exception)
            {
                result = exception.Message;
            }
            return result;
        }

        /// <summary>
        /// Returns Exception as text including InnerException.
        /// </summary>
        public static string ExceptionToText(Exception exception)
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
        /// Returns for example: "Framework.Application.App"
        /// </summary>
        public static string TypeToName(Type type)
        {
            string result = null;
            if (type != null)
            {
                result = type.FullName;
            }
            return result;
        }

        /// <summary>
        /// Returns list of assemblies. Including Framework assembly.
        /// </summary>
        public static Type[] TypeInAssemblyList(Type typeInAssembly)
        {
            List<Type> result = new List<Type>();
            result.Add(typeof(UtilFramework));
            if (result.First().GetTypeInfo().Assembly != typeInAssembly.GetTypeInfo().Assembly)
            {
                result.Add(typeInAssembly);
            }
            return result.ToArray();
        }

        public static Type TypeFromName(string name, params Type[] typeInAssemblyList)
        {
            List<Type> resultList = new List<Type>();
            foreach (var type in typeInAssemblyList)
            {
                Type resultType = type.GetTypeInfo().Assembly.GetType(name);
                if (resultType != null)
                {
                    if (!resultList.Contains(resultType))
                    {
                        resultList.Add(resultType);
                    }
                }
            }
            Type result = resultList.FirstOrDefault();
            if (result == null)
            {
                throw new Exception(string.Format("Url path points to an App class which does not exist. Run BuildTool SqlCreate command to update database table FrameworkApplicationType. ({0})", name));
            }
            return resultList.Single(); // See also database table FrameworkApplication.
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

        public static bool IsSubclassOf(Type type, Type typeBase)
        {
            if (type == null)
            {
                return false;
            }
            return type.GetTypeInfo().IsSubclassOf(typeBase) || type == typeBase;
        }

        /// <summary>
        /// Write to stdout.
        /// </summary>
        public static void Log(string text)
        {
            Console.WriteLine(text);
        }

        /// <summary>
        /// Write Debug.csv file.
        /// </summary>
        /// <param name="text"></param>
        internal static void LogDebug(string text)
        {
            string fileName = FolderName + "Submodule/Framework/Debug.csv";
            int threadId = Thread.CurrentThread.ManagedThreadId;
            text = text.Replace(",", ";");
            text = text.Replace("\r", "");
            text = text.Replace("\n", "");
            text = text.Replace("\"", "");
            string time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            string result = string.Format("=\"{0}\",{1:000},{2}" + "\r\n", time, threadId, text);
            // if (!File.Exists(fileName))
            // {
            //     File.AppendAllText(fileName, "Time,ThreadId,Text" + "\r\n");
            // }
            // File.AppendAllText(fileName, result);
        }

        [ThreadStatic]
        private static ConsoleColor? colorDefault;

        /// <summary>
        /// Change font color.
        /// </summary>
        public static void LogColor(ConsoleColor color)
        {
            if (colorDefault == null)
            {
                colorDefault = Console.ForegroundColor;
            }
            Console.ForegroundColor = color;
        }

        /// <summary>
        /// Change font color back to default color.
        /// </summary>
        public static void LogColorDefault()
        {
            if (colorDefault != null)
            {
                Console.ForegroundColor = colorDefault.Value;
            }
        }

        /// <summary>
        /// Write to stderr.
        /// </summary>
        public static void LogError(string text)
        {
            Console.Error.WriteLine(text);
        }

        /// <summary>
        /// Returns underlying tpye, if any.
        /// </summary>
        public static Type TypeUnderlying(Type type)
        {
            Type result = type;
            Type typeUnderlying = Nullable.GetUnderlyingType(type);
            if (typeUnderlying != null)
            {
                result = typeUnderlying;
            }
            return result;
        }
    }
}
