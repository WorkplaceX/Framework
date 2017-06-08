namespace Framework
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Reflection;

    public static class Util
    {
        public static string VersionServer
        {
            get
            {
                return "v0.236 Server";
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
            Uri uri = new Uri(typeof(Util).GetTypeInfo().Assembly.CodeBase);
            string result;
            isIss = false;
            if (uri.AbsolutePath.EndsWith("/Build/bin/Debug/netcoreapp1.1/Framework.dll")) // Running in Visual Studio
            {
                result = new Uri(uri, "../../../../").AbsolutePath;
            }
            else
            {
                if (uri.AbsolutePath.EndsWith("Server/bin/Debug/netcoreapp1.1/Framework.dll")) // Running in Visual Studio
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

        public static string[] FileNameList(string folderName)
        {
            var result = Directory.GetFiles(folderName, "*.*", SearchOption.AllDirectories).OrderBy(item => item).ToArray();
            return result;
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
    }
}
