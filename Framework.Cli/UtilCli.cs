namespace Framework.Cli
{
    using Framework.Cli.Command;
    using Framework.Cli.Config;
    using Microsoft.Extensions.CommandLineUtils;
    using System;
    using System.CodeDom;
    using System.CodeDom.Compiler;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading.Tasks;

    internal static class UtilCli
    {
        /// <summary>
        /// Returns CommandOption flag.
        /// </summary>
        public static bool OptionGet(this CommandOption option)
        {
            return option?.Value() == "on" == true;
        }

        /// <summary>
        /// Sets CommandOption flag.
        /// </summary>
        public static void OptionSet(ref CommandOption option, bool value)
        {
            if (option == null)
            {
                option = new CommandOption("--null", CommandOptionType.NoValue); // For example if command calls command and options is not registered.
            }
            option.Values.Clear();
            if (value)
            {
                option.Values.Add("on");
            }
            UtilFramework.Assert(OptionGet(option) == value);
        }

        /// <summary>
        /// Run dotnet command.
        /// </summary>
        internal static void DotNet(string workingDirectory, string arguments, bool isWait = true)
        {
            Start(workingDirectory, "dotnet", arguments, isWait: isWait);
        }

        /// <summary>
        /// Run npm command.
        /// </summary>
        internal static void Npm(string workingDirectory, string arguments, bool isWait = true, bool isRedirectStdErr = false)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                UtilCli.Start(workingDirectory, "cmd", "/c npm.cmd " + arguments, isWait: isWait, isRedirectStdErr: isRedirectStdErr);
            }
            else
            {
                UtilCli.Start(workingDirectory, "npm", arguments, isWait: isWait, isRedirectStdErr: isRedirectStdErr);
            }
        }

        /// <summary>
        /// Start script.
        /// </summary>
        /// <param name="isRedirectStdErr">If true, do not write to stderr. Use this flag if shell command is known to write info (mistakenly) to stderr.</param>
        internal static void Start(string workingDirectory, string fileName, string arguments, bool isWait = true, bool isRedirectStdErr = false)
        {
            string time = UtilFramework.DateTimeToString(DateTime.Now);
            UtilCli.ConsoleWriteLinePassword(string.Format("### {4} Process Begin (FileName={1}; Arguments={2}; IsWait={3}; WorkingDirectory={0};)", workingDirectory, fileName, arguments, isWait, time), ConsoleColor.Green);

            ProcessStartInfo info = new ProcessStartInfo
            {
                WorkingDirectory = workingDirectory,
                FileName = fileName,
                Arguments = arguments
            };
            if (isRedirectStdErr)
            {
                info.RedirectStandardError = true; // Do not write to stderr.
            }
            // info.UseShellExecute = true;

            using (var process = Process.Start(info))
            {
                if (isWait)
                {
                    if (isRedirectStdErr)
                    {
                        // process.WaitForExit(); // Can hang. For example Angular 9.1.1 build:ssr (May be when std buffer is full)
                        string errorText = process.StandardError.ReadToEnd(); // Waits for process to exit.
                        process.WaitForExit(); // Used for Ubuntu. Otherwise HasExited is not (yet) true.
                        UtilFramework.Assert(process.HasExited);
                        if (!string.IsNullOrEmpty(errorText))
                        {
                            UtilCli.ConsoleWriteLinePassword(string.Format("### {4} Process StdErr (FileName={1}; Arguments={2}; IsWait={3}; WorkingDirectory={0};)", workingDirectory, fileName, arguments, isWait, time), ConsoleColor.DarkGreen); // Write stderr to stdout.
                            UtilCli.ConsoleWriteLinePassword(errorText, ConsoleColor.DarkGreen); // Log DarkGreen because it is not treated like an stderr output.
                        }
                    }
                    else
                    {
                        process.WaitForExit();
                        UtilFramework.Assert(process.HasExited);
                    }
                    if (process.ExitCode != 0)
                    {
                        throw new Exception("Script failed!");
                    }
                }
            }

            UtilCli.ConsoleWriteLinePassword(string.Format("### {4} Process End (FileName={1}; Arguments={2}; IsWait={3}; WorkingDirectory={0};)", workingDirectory, fileName, arguments, isWait, time), ConsoleColor.DarkGreen);
        }

        /// <summary>
        /// Returns stdout of command.
        /// </summary>
        internal static string StartStdout(string workingDirectory, string fileName, string arguments)
        {
            ProcessStartInfo info = new ProcessStartInfo
            {
                WorkingDirectory = workingDirectory,
                FileName = fileName,
                Arguments = arguments,
                RedirectStandardOutput = true // Do not write to stdout.
            };
            var process = Process.Start(info);
            process.WaitForExit();
            string result = process.StandardOutput.ReadToEnd();
            if (process.ExitCode != 0)
            {
                throw new Exception("Script failed!");
            }

            return result;
        }

        internal static void OpenWebBrowser(string url)
        {
            Start(null, "cmd", $"/c start {url}", isWait: false);
        }

        internal static void FolderNameDelete(string folderName)
        {
            if (Directory.Exists(folderName))
            {
                foreach (FileInfo fileInfo in new DirectoryInfo(folderName).GetFiles("*.*", SearchOption.AllDirectories))
                {
                    fileInfo.Attributes = FileAttributes.Normal; // See also: https://stackoverflow.com/questions/1701457/directory-delete-doesnt-work-access-denied-error-but-under-windows-explorer-it/30673648
                }
                Directory.Delete(folderName, true);
            }
        }

        /// <summary>
        /// Adjustment for: If sequence of arguments is passed different than defined in CommandLineApplication values are wrong.
        /// </summary>
        /// <param name="commandBase"></param>
        /// <param name="commandArgument"></param>
        /// <returns></returns>
        private static CommandArgument ArgumentValue(CommandBase command, CommandArgument commandArgument)
        {
            CommandArgument result = null;
            foreach (CommandArgument item in command.Configuration.Arguments)
            {
                string name = item.Value;
                if (name?.IndexOf("=") != -1)
                {
                    name = name?.Substring(0, name.IndexOf("="));
                }
                if (name?.ToLower() == commandArgument.Name.ToLower())
                {
                    result = item;
                    break;
                }
            }
            return result;
        }

        /// <summary>
        /// Returns true, if argument is used in command line.
        /// </summary>
        internal static bool ArgumentValueIsDelete(CommandBase command, CommandArgument commandArgument)
        {
            commandArgument = ArgumentValue(command, commandArgument); // Sequence of passed arguments might be wrong.

            bool result = commandArgument != null && commandArgument.Value != null;
            return result;
        }

        /// <summary>
        /// Returns true, if value has been set. (Use Argument=null to set a value to null).
        /// </summary>
        /// <param name="value">Returns value.</param>
        internal static bool ArgumentValue(CommandBase command, CommandArgument commandArgument, out string value)
        {
            string name = commandArgument.Name;
            commandArgument = ArgumentValue(command, commandArgument); // Sequence of passed arguments might be wrong.

            bool isValue = false;
            string result = commandArgument.Value;
            UtilFramework.Assert(name.ToLower() == result.Substring(0, name.Length).ToLower());
            if (result.ToUpper().StartsWith(name.ToUpper()))
            {
                result = result.Substring(name.Length);
            }
            if (result.StartsWith("="))
            {
                result = result.Substring(1);
            }
            result = UtilFramework.StringNull(result);
            if (result != null)
            {
                isValue = true;
            }
            if (result?.ToLower() == "null") // User sets value to null.
            {
                result = null;
            }
            value = result;
            return isValue;
        }

        /// <summary>
        /// Copy folder.
        /// </summary>
        /// <param name="searchPattern">For example: "*.*"</param>
        /// <param name="isAllDirectory">If true, includes subdirectories.</param>
        internal static void FolderCopy(string folderNameSource, string folderNameDest, string searchPattern, bool isAllDirectory)
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

        /// <summary>
        /// Create folder if it does not yet exist.
        /// </summary>
        internal static void FolderCreate(string fileName)
        {
            string folderName = new FileInfo(fileName).DirectoryName;
            if (!Directory.Exists(folderName))
            {
                Directory.CreateDirectory(folderName);
            }
        }

        internal static void FileCopy(string fileNameSource, string fileNameDest)
        {
            FolderCreate(fileNameDest);
            File.Copy(fileNameSource, fileNameDest, true);
        }

        internal static void FolderDelete(string folderName)
        {
            var count = 0;
            do
            {
                if (count > 0)
                {
                    Task.Delay(1000).Wait(); // Wait for next attempt.
                }
                if (UtilCli.FolderNameExist(folderName))
                {
                    foreach (FileInfo fileInfo in new DirectoryInfo(folderName).GetFiles("*.*", SearchOption.AllDirectories))
                    {
                        fileInfo.Attributes = FileAttributes.Normal; // See also: https://stackoverflow.com/questions/1701457/directory-delete-doesnt-work-access-denied-error-but-under-windows-explorer-it/30673648
                    }
                    try
                    {
                        Directory.Delete(folderName, true);
                    }
                    catch (IOException)
                    {
                        // Silent exception.
                        Console.WriteLine(string.Format("Can not delete folder! ({0})", folderName));
                    }
                }
                count += 1;
            } while (UtilCli.FolderNameExist(folderName) && count <= 3);
            UtilFramework.Assert(!UtilCli.FolderNameExist(folderName), string.Format("Can not delete folder! Make sure server.ts and node.exe is not running. ({0}", folderName));
        }

        internal static bool FolderNameExist(string folderName)
        {
            return Directory.Exists(folderName);
        }

        /// <summary>
        /// Returns git commit sha.
        /// </summary>
        internal static string GitCommit()
        {
            string result = "Commit";
            try
            {
                result = UtilCli.StartStdout(UtilFramework.FolderName, "git", "rev-parse --short HEAD");
                result = result.Replace("\n", "");
            }
            catch
            {
                // Silent exception
            }
            return result;
        }

        /// <summary>
        /// Tag version build.
        /// </summary>
        internal static void VersionBuild(Action build)
        {
            // Read UtilFramework.cs
            string fileNameServer = UtilFramework.FolderName + "Framework/Framework/UtilFramework.cs";
            string textServer = UtilFramework.FileLoad(fileNameServer);
            string fileNameClient = UtilFramework.FolderName + "Framework/Framework.Angular/application/src/app/data.service.ts";
            string textClient = UtilFramework.FileLoad(fileNameClient);

            string versionBuild = string.Format("Build (WorkplaceX={3}; Commit={0}; Pc={1}; Time={2} (UTC);)", UtilCli.GitCommit(), System.Environment.MachineName, UtilFramework.DateTimeToString(DateTime.Now.ToUniversalTime()), UtilFramework.Version);

            string findServer = "/* VersionBuild */"; // See also: method CommandBuild.BuildServer();
            string replaceServer = string.Format("                return \"{0}\"; /* VersionBuild */", versionBuild);
            string findClient = "/* VersionBuild */"; // See also: file data.service.ts
            string replaceClient = string.Format("  public VersionBuild: string = \"{0}\"; /* VersionBuild */", versionBuild);

            // Write UtilFramework.cs
            string textNewServer = UtilFramework.ReplaceLine(textServer, findServer, replaceServer);
            File.WriteAllText(fileNameServer, textNewServer);
            string textNewClient = UtilFramework.ReplaceLine(textClient, findClient, replaceClient);
            File.WriteAllText(fileNameClient, textNewClient);

            try
            {
                build();
            }
            finally
            {
                File.WriteAllText(fileNameServer, textServer); // Back to original text.
                File.WriteAllText(fileNameClient, textClient); // Back to original text.
            }
        }

        /// <summary>
        /// Returns password (ConnectionString or GitUrl) without sensitive data.
        /// </summary>
        /// <param name="password">For example ConnectionString or GitUrl.</param>
        private static string ConsoleWriteLinePasswordHide(string password)
        {
            return "[Password]"; // Remove password from ConnectionString or GitUrl.
        }

        /// <summary>
        /// Returns text without password. It replaces password with PasswordHide.
        /// </summary>
        private static string ConsoleWriteLinePasswordHide(string text, string password)
        {
            if (text != null && password?.Length > 0)
            {
                while (text.ToLower().IndexOf(password.ToLower()) >= 0)
                {
                    int indexStart = text.ToLower().IndexOf(password.ToLower());
                    int length = password.Length;
                    string passwordHide = ConsoleWriteLinePasswordHide(password);
                    text = text.Substring(0, indexStart) + passwordHide + text.Substring(indexStart + length);
                }
            }
            return text;
        }

        /// <summary>
        /// Write text which might contain sensitive data (ConnectionString and GitUrl) with this method to console.
        /// </summary>
        internal static void ConsoleWriteLinePassword(object value, ConsoleColor? color = null)
        {
            string text = string.Format("{0}", value);
            var configCli = ConfigCli.Load();
            var environment = configCli.EnvironmentGet();
            text = ConsoleWriteLinePasswordHide(text, environment.ConnectionStringFramework);
            text = ConsoleWriteLinePasswordHide(text, environment.ConnectionStringApplication);
            text = ConsoleWriteLinePasswordHide(text, environment.DeployAzureGitUrl);
            text = ConsoleWriteLinePasswordHide(text, configCli.ExternalGit);

            text = text.Replace("{", "{{").Replace("}", "}}"); // Console.Write("{", ConsoleColor.Green); throws exception "Input string was not in a correct format". // TODO Bug report

            Console.WriteLine(text, color);
        }

        /// <summary>
        /// Write to console in color.
        /// </summary>
        internal static void ConsoleWriteLineColor(object value, ConsoleColor? color)
        {
            if (color == null)
            {
                Console.WriteLine(value);
            }
            else
            {
                Console.ForegroundColor = color.Value;
                Console.WriteLine(value);
                Console.ResetColor();

                // ConsoleColor foregroundColor = Console.ForegroundColor;
                // Console.ForegroundColor = color.Value;
                // try
                // {
                //     Console.WriteLine(value);
                // }
                // finally
                // {
                //     Console.ForegroundColor = foregroundColor;
                // }
            }
        }

        /// <summary>
        /// Wait for user interaction.
        /// </summary>
        internal static bool ConsoleReadYesNo(string text)
        {
            string consoleReadLine;
            do
            {
                Console.Write(text + " [y/n] ");
                consoleReadLine = Console.ReadLine().ToUpper();
            } while (!(consoleReadLine == "Y" || consoleReadLine == "N"));
            return consoleReadLine == "Y";
        }

        /// <summary>
        /// Write to stderr.
        /// </summary>
        internal static void ConsoleWriteLineError(object value)
        {
            using TextWriter textWriter = Console.Error;
            textWriter.WriteLine(value);
        }

        /// <summary>
        /// Returns text escaped as CSharp code. Handles special characters.
        /// </summary>
        public static string EscapeCSharpString(string text)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("\"");
            var textList = UtilFramework.SplitChunk(text); // Because of line break after 80 characters!
            using (var writer = new StringWriter(CultureInfo.InvariantCulture))
            {
                using var provider = CodeDomProvider.CreateProvider("CSharp");
                foreach (var item in textList)
                {
                    provider.GenerateCodeFromExpression(new CodePrimitiveExpression(item), writer, null); // Does a line break after 80 characters by default!
                    string textCSharp = writer.ToString();
                    UtilFramework.Assert(textCSharp.StartsWith("\""));
                    UtilFramework.Assert(textCSharp.EndsWith("\""));
                    textCSharp = textCSharp[1..^1]; // Remove quotation marks.
                    stringBuilder.Append(textCSharp);
                    writer.GetStringBuilder().Clear(); // Reset writer for next chunk.
                }
            }
            stringBuilder.Append("\"");
            string result = stringBuilder.ToString();
            return result;
        }

        /// <summary>
        /// Create new text file.
        /// </summary>
        public static void FileCreate(string fileName, string text = null)
        {
            FolderCreate(fileName);
            File.WriteAllText(fileName, text);
        }

        /// <summary>
        /// Rename file.
        /// </summary>
        public static void FileRename(string fileNameSource, string fileNameDest)
        {
            if (fileNameSource != fileNameDest)
            {
                FileCopy(fileNameSource, fileNameDest);
                File.Delete(fileNameSource);
            }
        }
    }
}
