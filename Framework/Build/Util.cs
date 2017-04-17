namespace Framework.Build
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.InteropServices;

    public class DescriptionAttribute : Attribute
    {
        public DescriptionAttribute(string text, double orderBy)
        {
            this.Text = text;
            this.OrderBy = orderBy;
        }

        public readonly string Text;

        public readonly double OrderBy;
    }

    public static class Util
    {
        public static void OpenBrowser(string url)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Process.Start(new ProcessStartInfo("cmd", $"/c start {url}")); // Works ok on windows
            }
        }

        public static void OpenVisualStudioCode(string folderName)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                ProcessStartInfo info = new ProcessStartInfo(Framework.Build.ConnectionManager.VisualStudioCodeFileName, folderName);
                info.CreateNoWindow = true;
                Process.Start(info);
            }
        }

        public class Method
        {
            public MethodInfo MethodInfo { get; set; }

            public DescriptionAttribute Description { get; set; }
        }

        public static Method[] MethodList(ScriptBase script)
        {
            List<Method> result = new List<Method>();
            foreach (var methodInfo in script.GetType().GetTypeInfo().GetMethods(BindingFlags.Public | BindingFlags.Instance))
            {
                DescriptionAttribute description = methodInfo.GetCustomAttribute(typeof(DescriptionAttribute)) as DescriptionAttribute;
                if (methodInfo.DeclaringType != typeof(object))
                {
                    if (methodInfo.GetParameters().Length == 0)
                    {
                        result.Add(new Method() { MethodInfo = methodInfo, Description = description });
                    }
                }
            }
            return result.OrderBy(item => item.Description.OrderBy).ToArray();
        }

        public static void MethodExecute(ScriptBase script)
        {
            int number = 0;
            foreach (Method method in Util.MethodList(script))
            {
                number += 1;
                string text = string.Format("{0:00}", number) + "=" + method.MethodInfo.Name;
                DescriptionAttribute description = method.Description;
                if (description != null)
                {
                    text += " " + "(" + description.Text + ")";
                }
                Util.Log(text);
            }
            Framework.Build.ConnectionManagerCheck.Run();
            Console.Write(">");
            string numberText = script.ArgGet(0);
            if (numberText == null)
            {
                numberText = Console.ReadLine();
            }
            try
            {
                int numberInt = int.Parse(numberText);
                Util.MethodList(script)[numberInt - 1].MethodInfo.Invoke(script, new object[] { });
            }
            catch (Exception exception)
            {
                string message = exception.Message;
                if (exception.InnerException != null)
                {
                    message = exception.InnerException.Message;
                }
                Util.Log(message);
            }
        }

        public static void Start(string workingDirectory, string fileName, string arguments, bool isThrowException = true, bool isWait = true, KeyValuePair<string, string>? environment = null)
        {
            ProcessStartInfo info = new ProcessStartInfo();
            if (environment != null)
            {
                info.Environment.Add(environment.Value.Key, environment.Value.Value);
            }
            info.WorkingDirectory = workingDirectory;
            info.FileName = fileName;
            info.Arguments = arguments;
            if (Framework.Util.IsLinux)
            {
                // info.UseShellExecute = true;
            }
            var process = Process.Start(info);
            if (isWait == true)
            {
                process.WaitForExit();
                if (isThrowException && process.ExitCode != 0)
                {
                    throw new Exception("Script failed!");
                }
            }
        }

        private static void Process_Exited(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        public static void NpmInstall(string workingDirectory, bool isThrowException = true)
        {
            string fileName = Framework.Build.ConnectionManager.NpmFileName;
            Start(workingDirectory, fileName, "install --loglevel error", isThrowException); // Do not show npm warnings.
        }

        public static void NpmRun(string workingDirectory, string script)
        {
            string fileName = Framework.Build.ConnectionManager.NpmFileName;
            Start(workingDirectory, fileName, "run " + script);
        }

        public static void DirectoryDelete(string folderName)
        {
            if (Directory.Exists(folderName))
            {
                Directory.Delete(folderName, true);
            }
        }

        public static void DirectoryCopy(string folderNameSource, string folderNameDest, string searchPattern, bool isAllDirectory)
        {
            var source = new DirectoryInfo(folderNameSource);
            var dest = new DirectoryInfo(folderNameDest);
            SearchOption searchOption = SearchOption.TopDirectoryOnly;
            if (isAllDirectory)
            {
                searchOption = SearchOption.AllDirectories;
            }
            foreach (FileInfo file in source.GetFiles(searchPattern, searchOption))
            {
                string fileNameSource = file.FullName;
                string fileNameDest = Path.Combine(dest.FullName, file.FullName.Substring(source.FullName.Length));
                FileCopy(fileNameSource, fileNameDest);
            }
        }

        public static void FileCopy(string fileNameSource, string fileNameDest)
        {
            string folderNameDest = new FileInfo(fileNameDest).DirectoryName;
            if (!Directory.Exists(folderNameDest))
            {
                Directory.CreateDirectory(folderNameDest);
            }
            File.Copy(fileNameSource, fileNameDest, true);
        }

        public static void Node(string workingDirectory, string fileName, bool isWait = true)
        {
            string nodeFileName = Framework.Build.ConnectionManager.NodeFileName;
            Start(workingDirectory, nodeFileName, fileName, false, isWait, new KeyValuePair<string, string>("PORT", "1337"));
        }

        public static void DotNetRestore(string workingDirectory)
        {
            string fileName = Framework.Build.ConnectionManager.DotNetFileName;
            Start(workingDirectory, fileName, "restore");
        }

        public static void MSBuild(string fileNameCsproj)
        {
            string fileName = Framework.Build.ConnectionManager.MSBuildFileName;
            string workingDirectory = Framework.Util.FolderName;
            Start(workingDirectory, fileName, fileNameCsproj);
        }

        public static void DotNetBuild(string workingDirectory)
        {
            string fileName = Framework.Build.ConnectionManager.DotNetFileName;
            Start(workingDirectory, fileName, "build");
        }

        public static void DotNetRun(string workingDirectory, bool isWait = true)
        {
            string fileName = Framework.Build.ConnectionManager.DotNetFileName;
            Start(workingDirectory, fileName, "run", false, isWait);
        }

        private static bool LogColorStart(string text)
        {
            string textStart = "Start";
            if (text.StartsWith(textStart))
            {
                Console.BackgroundColor = ConsoleColor.DarkGreen;
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write(textStart);
                Console.BackgroundColor = ConsoleColor.Black;
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine(text.Substring(textStart.Length));
                return true;

            }
            return false;
        }

        private static bool LogColor(string text, string textStartsWith, ConsoleColor color)
        {
            if (text.StartsWith(textStartsWith))
            {
                Console.BackgroundColor = color;
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write(textStartsWith);
                Console.BackgroundColor = ConsoleColor.Black;
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine(text.Substring(textStartsWith.Length));
                return true;
            }
            return false;
        }

        private static bool LogColor(string text)
        {
            if (text == "Build Command")
            {
                Console.BackgroundColor = ConsoleColor.Blue;
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine(text);
                return true;
            }
            if (LogColor(text, "Error:", ConsoleColor.Red)) { return true; };
            if (LogColor(text, "Warning:", ConsoleColor.DarkYellow)) { return true; };
            string[] textList = text.Split(new string[] { "=" }, StringSplitOptions.None);
            if (textList.Count() == 2 && (textList[0].FirstOrDefault() >= '0' && textList[0].FirstOrDefault() <= '9'))
            {
                Console.BackgroundColor = ConsoleColor.DarkYellow;
                Console.Write("[" + textList[0] + "]");
                Console.BackgroundColor = ConsoleColor.Black;
                Console.Write(" = ");
                if (!LogColorStart(textList[1]))
                {
                    Console.WriteLine(textList[1]);
                }
                return true;
            }
            return false;
        }

        public static void Log(string text)
        {
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.Gray;
            if (!LogColor(text))
            {
                Console.WriteLine(text);
            }
        }

        public static void LogClear()
        {
            Console.Clear();
        }
    }
}
