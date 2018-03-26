using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Framework.BuildTool")] // Not public but used by other Framework assembly.
[assembly: InternalsVisibleTo("Framework.UnitTest")] // Access internal methods by UnitTest.

namespace Framework
{
    using Database.dbo;
    using Framework.Application.Config;
    using Framework.DataAccessLayer;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore.Metadata;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Linq.Dynamic.Core;
    using System.Net.Http;
    using System.Reflection;
    using System.Runtime.Loader;
    using System.Threading;

    public static class UtilFramework
    {
        public static string VersionServer
        {
            get
            {
                // .NET Core 2.0
                return "v1.110 Server";
            }
        }

        /// <summary>
        /// Gets VersionClient. This is the expected client version. See also file util.ts
        /// </summary>
        public static string VersionClient
        {
            get
            {
                // node 8.9.2 LTS
                // npm 5.5.1
                return "v1.090 Client";
            }
        }

        /// <summary>
        /// First LinqDynamic query is slow. This is a warm up query for better performance.
        /// </summary>
        internal static void LinqDynamicBoot()
        {
            var list = new List<LinqDynamic>();
            list.Add(new LinqDynamic() { Text = "F" });
            var query = list.AsQueryable();
            var queryDynamic = query.OrderBy("Text"); // Takes ca. 360ms the first time.
            var result = queryDynamic.ToDynamicArray();
            if (result.Count() == 0)
            {
                UtilFramework.Assert(true);
            }
        }

        internal class LinqDynamic
        {
            public string Text;
        }

        /// <summary>
        /// Enable InMemory database for unit tests.
        /// </summary>
        /// <param name="typeInAssembly">Assembly to scan for Row classes.</param>
        /// <param name="init">Write for example FrameworkApplicationView records to InMemory database.</param>
        internal static void UnitTest(Type typeInAssembly, Action init)
        {
            UnitTestService.Instance.TypeInAssembly = typeInAssembly;
            if (UnitTestService.Instance.IsUnitTest == false)
            {
                UnitTestService.Instance.IsUnitTest = true;
                init();
            }
        }

        /// <summary>
        /// Enable InMemory database for unit tests.
        /// </summary>
        public static void UnitTest(Type typeApp)
        {
            Type typeInAssembly = typeApp;
            UnitTest(typeInAssembly, () => {
                UtilDataAccessLayer.Insert(new FrameworkApplicationDisplay() { TypeName = UtilFramework.TypeToName(typeApp), IsActive = true, IsExist = true });
                UtilDataAccessLayer.Insert(new FrameworkApplicationDisplay() { TypeName = UtilFramework.TypeToName(typeof(AppConfig)), IsActive = true, IsExist = true, Path = "config" });
            });
        }

        /// <summary>
        /// Dynamically loads assembly Framework.UnitTest.
        /// </summary>
        /// <returns></returns>
        public static Assembly AssemblyUnitTest()
        {
            Type typeUnitTest = Type.GetType("UnitTest.Application.UnitTestApplication, Framework.UnitTest");
            if (typeUnitTest != null)
            {
                return typeUnitTest.Assembly; // Assembly Framework.UnitTest is already loaded for example when project started by Framework.UnitTest. Load it again would result in exception.
            }
            else
            {
                string fileName = UtilFramework.FolderName + "/Submodule/Framework.UnitTest/bin/Debug/netcoreapp2.0/Framework.UnitTest.dll";
                Assembly assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(fileName);
                return assembly;
            }
        }

        /// <summary>
        /// Enable InMemory database for unit tests. Load dynamically Framework.UnitTest.
        /// </summary>
        public static void UnitTest()
        {
            string typeAppString = "MyApp";
            Assembly assembly = AssemblyUnitTest();
            var typeList = assembly.GetTypes();
            Type typeApp = typeList.Where(item => item.Name == typeAppString).Single();
            UtilFramework.UnitTest(typeApp, () => {
                UtilDataAccessLayer.Insert(new FrameworkApplicationDisplay() { TypeName = UtilFramework.TypeToName(typeApp), IsActive = true, IsExist = true });
            });
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

        private static void FolderNamePrivate(out string folderName, out bool isIss)
        {
            Uri uri = new Uri(typeof(UtilFramework).GetTypeInfo().Assembly.CodeBase);
            isIss = false;
            if (uri.AbsolutePath.EndsWith("/Build/bin/Debug/netcoreapp2.0/Framework.dll") || uri.AbsolutePath.EndsWith("/BuildTool/bin/Debug/netcoreapp2.0/Framework.dll")) // Running in Visual Studio
            {
                folderName = new Uri(uri, "../../../../").AbsolutePath;
                return;
            }
            if (uri.AbsolutePath.EndsWith("Server/bin/Debug/netcoreapp2.0/Framework.dll")) // Running in Visual Studio
            {
                folderName = new Uri(uri, "../../../../").AbsolutePath;
                return;
            }
            if (uri.AbsolutePath.EndsWith("Submodule/Framework.UnitTest/bin/Debug/netcoreapp2.0/Framework.dll")) // Framework.UnitTest running in Visual Studio
            {
                folderName = new Uri(uri, "../../../../../").AbsolutePath;
                return;
            }
            if (uri.AbsolutePath.EndsWith("Framework.dll")) // On IIS
            {
                folderName = new Uri(uri, "./").AbsolutePath;
                isIss = true;
                return;
            }
            throw new Exception("FileName unknown!");
        }

        /// <summary>
        /// Gets root FolderName.
        /// </summary>
        internal static string FolderName
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
        internal static bool IsLinux
        {
            get
            {
                return FolderName.StartsWith("/");
            }
        }

        /// <summary>
        /// Gets FolderNameIsIss. True, if running on ISS server.
        /// </summary>
        internal static bool FolderNameIsIss
        {
            get
            {
                string folderName;
                bool isIss;
                FolderNamePrivate(out folderName, out isIss);
                return isIss;
            }
        }

        internal static string FileRead(string fileName)
        {
            return File.ReadAllText(fileName);
        }

        internal static void FileWrite(string fileName, string value)
        {
            lock (typeof(object))
            {
                File.WriteAllText(fileName, value);
            }
        }

        internal static string[] FileNameList(string folderName, string searchPattern)
        {
            var list = Directory.GetFiles(folderName, searchPattern, SearchOption.AllDirectories);
            var result = list.Select(item => item.Replace(@"\", "/")).ToArray();
            return result;
        }

        internal static string[] FileNameList(string folderName)
        {
            return FileNameList(folderName, "*.*");
        }

        /// <summary>
        /// Returns external ip address.
        /// </summary>
        internal static string Ip()
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
        internal static string ExceptionToText(Exception exception)
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
        /// Returns all types of type with base class typeBase in typeInAssembly. Searches also Framework assembly.
        /// </summary>
        internal static List<Type> TypeList(Type typeInAssembly, Type typeBase)
        {
            List<Type> result = new List<Type>();
            List<Assembly> assemblyList = new List<Assembly>();
            assemblyList.Add(typeInAssembly.Assembly);
            if (typeof(UtilFramework).Assembly != typeInAssembly.Assembly)
            {
                assemblyList.Add(typeof(UtilFramework).Assembly);
            }
            foreach (Assembly assembly in assemblyList)
            {
                foreach (Type type in assembly.GetTypes())
                {
                    if (UtilFramework.IsSubclassOf(type, typeBase))
                    {
                        result.Add(type);
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Returns list of assemblies. Including Framework assembly.
        /// </summary>
        internal static Type[] TypeInAssemblyList(Type typeInAssembly)
        {
            List<Type> result = new List<Type>();
            //
            result.Add(typeof(UtilFramework)); // Add Framework assembly.
            if (UnitTestService.Instance.IsUnitTest)
            {
                result.Add(AssemblyUnitTest().GetTypes().First()); // Add Framework.UnitTest assembly.
            }
            if (result.Where(item => item.Assembly == typeInAssembly.Assembly).Count() == 0)
            {
                result.Add(typeInAssembly); // Add assembly.
            }
            //
            return result.ToArray();
        }

        /// <summary>
        /// Returns for example type of: "Framework.Application.App"
        /// </summary>
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

        internal static bool IsSubclassOf(Type type, Type typeBase)
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
        internal static void Log(string text)
        {
            Console.WriteLine(text);
        }

        internal enum LogDebugOutput { None = 0, File = 1, Console = 2  };

        private static DateTime logDebugDateTime = DateTime.Now;

        /// <summary>
        /// Write Debug.csv file.
        /// </summary>
        internal static void LogDebug(string text, bool isDebug = false)
        {
            if (isDebug == false)
            {
                return;
            }
            //
            LogDebugOutput logDebugOutput = LogDebugOutput.Console; // Switch debug output manually.
            if (logDebugOutput != LogDebugOutput.None)
            {
                StackTrace stackTrace = new StackTrace();
                string caller1 = stackTrace.GetFrame(1)?.GetMethod().DeclaringType.Name + "." + stackTrace.GetFrame(1)?.GetMethod().Name + "();";
                string caller2 = stackTrace.GetFrame(2)?.GetMethod().DeclaringType.Name + "." + stackTrace.GetFrame(2)?.GetMethod().Name + "();";
                string caller3 = stackTrace.GetFrame(3)?.GetMethod().DeclaringType.Name + "." + stackTrace.GetFrame(3)?.GetMethod().Name + "();";
                //
                int threadId = Thread.CurrentThread.ManagedThreadId;
                text = text.Replace(",", ";");
                text = text.Replace("\r", "");
                text = text.Replace("\n", "");
                text = text.Replace("\"", "");
                string time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                string result = string.Format("=\"{0}\",{1:000},{2},({3}, {4}, {5})", time, threadId, text, caller1, caller2, caller3);
                TimeSpan timeSpan = DateTime.Now - logDebugDateTime;
                if (timeSpan.TotalSeconds > 3)
                {
                    logDebugDateTime = DateTime.Now;
                    result = "-----------------------------\r\n\r\n" + result; // Seperator
                }
                //
                if (logDebugOutput == LogDebugOutput.File)
                {
                    string fileName = FolderName + "Submodule/Framework/Debug.csv";
                    if (!File.Exists(fileName))
                    {
                        File.AppendAllText(fileName, "Time,ThreadId,Text,Caller" + "\r\n");
                    }
                    File.AppendAllText(fileName, result);
                }
                if (logDebugOutput == LogDebugOutput.Console)
                {
                    Debug.WriteLine("");
                    Debug.WriteLine(result);
                    Debug.WriteLine("");
                }
            }
        }

        [ThreadStatic]
        private static ConsoleColor? colorDefault;

        /// <summary>
        /// Change font color.
        /// </summary>
        internal static void LogColor(ConsoleColor color)
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
        internal static void LogColorDefault()
        {
            if (colorDefault != null)
            {
                Console.ForegroundColor = colorDefault.Value;
            }
        }

        /// <summary>
        /// Write to stderr.
        /// </summary>
        internal static void LogError(string text)
        {
            Console.Error.WriteLine(text);
        }

        /// <summary>
        /// Returns underlying tpye, if any.
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
    }

    /// <summary>
    /// Enable InMemory database for unit tests.
    /// </summary>
    internal class UnitTestService
    {
        /// <summary>
        /// Gets or sets IsUnitTest. Run application in UnitTest mode.
        /// </summary>
        public bool IsUnitTest { get; set; }

        /// <summary>
        /// Gets or sets App. App of current request.
        /// </summary>
        public Type TypeInAssembly { get; set; }

        public IMutableModel Model { get; set; }

        /// <summary>
        /// Field used, if running as Framework.UnitTest.
        /// </summary>
        // [ThreadStatic] // Multiple threads will access. See also: method ConfigInternal.LoadDatabaseConfig(); command Task.WhenAll();
        private static UnitTestService instance;

        /// <summary>
        /// Gets Instance. Singelton.
        /// </summary>
        public static UnitTestService Instance
        {
            get
            {
                HttpContext httpContext = new HttpContextAccessor().HttpContext;
                if (httpContext == null) // Running as Framework.UnitTest.
                {
                    if (instance == null)
                    {
                        instance = new UnitTestService();
                    }
                    return instance;
                }
                else
                {
                    return (UnitTestService)httpContext.RequestServices.GetService(typeof(UnitTestService));
                }
            }
        }
    }
}
