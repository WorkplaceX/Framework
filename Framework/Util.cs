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
                return "v0.2 Server";
            }
        }

        /// <summary>
        /// Gets root FolderName.
        /// </summary>
        public static string FolderName
        {
            get
            {
                Uri uri = new Uri(typeof(Util).GetTypeInfo().Assembly.CodeBase);
                string result;
                if (uri.AbsolutePath.EndsWith("/Build/bin/Debug/netcoreapp1.1/Framework.dll")) // Running in Visual Studio
                {
                    result = new Uri(uri, "../../../../").AbsolutePath;
                }
                else
                {
                    if (uri.AbsolutePath.EndsWith("Submodule/Server/bin/Debug/netcoreapp1.1/Framework.dll"))
                    {
                        result = new Uri(uri, "../../../../../").AbsolutePath;
                    }
                    else
                    {
                        throw new Exception("FileName unknown!");
                    }
                }
                return result;
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
    }
}
