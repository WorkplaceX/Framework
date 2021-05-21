namespace Framework.Server
{
    using Database.dbo;
    using Framework.Config;
    using Framework.DataAccessLayer;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    internal class UtilServer
    {
        public static void Cors()
        {
            string origin = Context.Request.Headers["Origin"];
            if (origin != null)
            {
                Context.Response.Headers.Add("Access-Control-Allow-Origin", origin); // Prevent browser error: blocked by CORS policy.
                Context.Response.Headers.Add("Access-Control-Allow-Credentials", "true");
            }
        }

        /// <summary>
        /// Gets or sets ServiceProvider. Allows method ServiceGet(); to be called outside http context request. For example by background service.
        /// </summary>
        public static IServiceProvider ServiceProvider;

        /// <summary>
        /// Returns service. Can be called from a background service or within a http context request.
        /// </summary>
        public static T ServiceGet<T>()
        {
            var serviceProvider = ServiceProvider;
            if (serviceProvider == null && Context != null)
            {
                // Fallback
                serviceProvider = Context.RequestServices;
            }
            return (T)serviceProvider.GetService(typeof(T));
        }

        /// <summary>
        /// Returns logger. See also file appsettings.json.
        /// </summary>
        public static ILogger Logger(string categoryName)
        {
            var loggerFactory = UtilServer.ServiceGet<ILoggerFactory>();
            var result = loggerFactory.CreateLogger(categoryName);
            return (ILogger)result;
        }

        /// <summary>
        /// Gets Context of web request.
        /// </summary>
        public static HttpContext Context
        {
            get
            {
                return new HttpContextAccessor().HttpContext; // Not available during startup. See also: method ConfigureServices(); Not available for Cli.
            }
        }

        public static ISession Session
        {
            get
            {
                return Context.Session;
            }
        }

        /// <summary>
        /// Returns location of ASP.NET server wwwroot folder.
        /// </summary>
        public static string FolderNameContentRoot()
        {
            var webHostEnvironment = UtilServer.ServiceGet<IWebHostEnvironment>();
            return new Uri(webHostEnvironment.ContentRootPath).AbsolutePath + "/";
        }

        /// <summary>
        /// Returns client request url. For example: "http://localhost:5000/".
        /// </summary>
        public static string RequestUrlHost()
        {
            HttpContext context = Context;
            string result = string.Format("{0}://{1}/", context.Request.Scheme, context.Request.Host.Value);
            // result = string.Format("{0}://{1}{2}", context.Request.Scheme, context.Request.Host.Value, context.Request.Path); // Returns also path. For example: "http://localhost:5000/config/data.txt"
            return result;
        }

        /// <summary>
        /// Returns client request domain name. For example "localhost". Does not include http, https or port.
        /// </summary>
        public static string RequestDomainName()
        {
            return Context.Request.Host.Host;
        }

        /// <summary>
        /// Returns html content type.
        /// </summary>
        public static string ContentType(string fileName)
        {
            // ContentType
            string fileNameExtension = UtilFramework.FileNameExtension(fileName);
            string result; // https://www.sitepoint.com/web-foundations/mime-types-complete-list/
            switch (fileNameExtension)
            {
                case ".html": result = "text/html"; break;
                case ".css": result = "text/css"; break;
                case ".js": result = "text/javascript"; break;
                case ".map": result = "text/plain"; break;
                case ".png": result = "image/png"; break;
                case ".ico": result = "image/x-icon"; break;
                case ".jpg": result = "image/jpeg"; break;
                case ".pdf": result = "application/pdf"; break;
                case ".json": result = "application/json"; break;
                case ".xlsx": result = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"; break;
                case ".xml": result = "text/xml"; break;
                case ".csv": result = "text/csv"; break;
                default:
                    result = "text/plain"; break; // Type not found!
            }
            return result;
        }

        /// <summary>
        /// Returns true, if application runs on IIS server. Otherwise it runs from Visual Studio.
        /// </summary>
        public static bool IsIssServer
        {
            get
            {
                string folderName = UtilFramework.FolderNameGet() + "Application.Server/";
                bool result = Directory.Exists(folderName) == false;
                return result;
            }
        }

        /// <summary>
        /// Start one universal server for every website.
        /// </summary>
        public static void StartUniversalServerAngular()
        {
            UtilFramework.NodeClose();

            var configServer = ConfigServer.Load();
            bool isFirst = true; // See also class CommandBuild option --first
            foreach (var website in configServer.WebsiteList)
            {
                string folderNameAngular = UtilFramework.FolderNameParse(website.FolderNameAngular);
                if (folderNameAngular != null && !website.FolderNameAngularIsDuplicate)
                {
                    string fileNameServer = UtilFramework.FolderName + "Application.Server/Framework/Application.Website/" + website.FolderNameAngularWebsite + "server/main.js";
                    if (!File.Exists(fileNameServer))
                    {
                        if (isFirst == false)
                        {
                            break; // See also
                        }
                        throw new Exception(string.Format("File does not exis! Make sure cli command build --client did run. ({0})", fileNameServer));
                    }
                    isFirst = false;
                    
                    ProcessStartInfo info = new ProcessStartInfo
                    {
                        WorkingDirectory = UtilFramework.FolderName + "Application.Server/Framework/Application.Website/" + website.FolderNameAngularWebsite + "server/",
                        FileName = "node.exe"
                    };
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                    {
                        info.FileName = "node";
                    }
                    info.Arguments = fileNameServer;
                    info.UseShellExecute = true; // Open additional node window.
                    info.WindowStyle = ProcessWindowStyle.Minimized; // Show node window minimized.

                    Environment.SetEnvironmentVariable("PORT", (website.FolderNameAngularPort).ToString());

                    // Start node with Application.Server/Framework/Application.Website/Website01/server/main.js
                    Process.Start(info);
                }
            }
        }

        /// <summary>
        /// Used to get body of web post.
        /// </summary>
        public static async Task<string> StreamToString(Stream stream)
        {
            string result;
            using (var streamReader = new StreamReader(stream))
            {
                result = await streamReader.ReadToEndAsync();
            }
            if (result == "")
            {
                result = null;
            }
            return result;
        }

        /// <summary>
        /// Returns true, if request or response path is a FileName. Otherwise path is a FolderName.
        /// </summary>
        /// <param name="navigatePath">For example "/" or "/main.js"</param>
        /// <returns></returns>
        public static bool NavigatePathIsFileName(string navigatePath)
        {
            return !string.IsNullOrEmpty(Path.GetFileName(navigatePath));
        }

        /// <summary>
        /// Post to json url.
        /// </summary>
        public static async Task<string> WebPost(string url, string json)
        {
            using HttpClient client = new HttpClient();
            HttpResponseMessage response;
            try
            {
                response = await client.PostAsync(url, new StringContent(json, Encoding.Unicode, "application/json")); // Make sure Universal server is running.
                response.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException exception)
            {
                throw new Exception(string.Format("Http request failed! Make sure cli build command did run. Close node.exe ({0})", url), exception);
            }
            string result = await response.Content.ReadAsStringAsync();
            return result;
        }
    }

    /// <summary>
    /// Background service to write to file and database.
    /// </summary>
    internal class BackgroundFrameworkService : BackgroundService
    {
        public BackgroundFrameworkService(ILoggerFactory loggerFactory, IServiceProvider serviceProvider)
        {
            Logger = loggerFactory.CreateLogger(typeof(BackgroundFrameworkService));
            ServiceProvider = serviceProvider;
        }

        public readonly ILogger Logger;

        public readonly IServiceProvider ServiceProvider;

        /// <summary>
        /// Gets TimeHeartbeat. Last heartbeat of BackgroundService.
        /// </summary>
        public string TimeHeartbeat { get; private set; }

        /// <summary>
        /// Data loaded from database.
        /// </summary>
        private List<FrameworkTranslate> TranslateList = new List<FrameworkTranslate>();

        /// <summary>
        /// Data to be added to database.
        /// </summary>
        private List<FrameworkTranslate> TranslateUpsertList = new List<FrameworkTranslate>();

        /// <summary>
        /// (AppTypeName, Name, FrameworkTranslate).
        /// </summary>
        private ConcurrentDictionary<(string, string), FrameworkTranslate> TranslateNameList;

        /// <summary>
        /// Translate text into a different language.
        /// </summary>
        /// <param name="name">Dictionary key for text.</param>
        /// <param name="text">Default text.</param>
        /// <param name="languageId">Language into which to translate to.</param>
        /// <returns>Returns into languageId translated text. If no translation entry is found text is returned.</returns>
        public string Translate(string appTypeName, string name, string text, int languageId)
        {
            var result = text;
            try
            {
                string textLanguage = null;
                var translateRow = TranslateNameList.GetOrAdd((appTypeName, name), (key) =>
                {
                    var row = new FrameworkTranslate { AppTypeName = key.Item1, Name = key.Item2, Text = text };
                    TranslateList.Add(row);
                    TranslateUpsertList.Add(row);
                    return row;
                });

                switch (languageId)
                {
                    case 1:
                        textLanguage = translateRow.TextLanguage01;
                        break;
                    case 2:
                        textLanguage = translateRow.TextLanguage02;
                        break;
                    case 3:
                        textLanguage = translateRow.TextLanguage03;
                        break;
                    case 4:
                        textLanguage = translateRow.TextLanguage04;
                        break;
                    default:
                        break;
                }

                if (translateRow.Text != text)
                {
                    translateRow.Text = text; // Default text changed.
                    TranslateUpsertList.Add(translateRow);
                }

                if (textLanguage != null)
                {
                    result = textLanguage;
                }
            }
            catch (Exception exception)
            {
                Logger.LogError("{0} {1}", nameof(BackgroundFrameworkService), exception.ToString());
            }
            return result;
        }

        /// <summary>
        /// Gets LogTextList. Contains log entries.
        /// </summary>
        private StringBuilder LogTextList = new StringBuilder();

        /// <summary>
        /// Log to file log.csv
        /// </summary>
        public void LogText(string text)
        {
            LogTextList.AppendLine(text);
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            Logger.LogInformation("Service start");
            return base.StartAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                // Load language translate table.
                UtilServer.ServiceProvider = ServiceProvider; // Make sure method ServiceGet(); is available.
                TranslateList = (await Data.Query<FrameworkTranslate>().QueryExecuteAsync()).ToList();
                TranslateNameList = new ConcurrentDictionary<(string, string), FrameworkTranslate>(TranslateList.ToDictionary(item => (item.AppTypeName, item.Name)));

                while (!stoppingToken.IsCancellationRequested)
                {
                    TimeHeartbeat = DateTime.UtcNow.ToString("HH:mmm:ss");
                    await Task.Delay(1000);

                    // Translate
                    if (TranslateUpsertList.Count > 0)
                    {
                        Logger.LogInformation("Update sql table FrameworkTranslate. ({0} Rows)", TranslateUpsertList.Count);
                        UtilServer.ServiceProvider = ServiceProvider; // Make sure method ServiceGet(); is available.
                        await UtilDalUpsert.UpsertAsync(TranslateUpsertList, new string[] { nameof(FrameworkTranslate.AppTypeName), nameof(FrameworkTranslate.Name) });
                        TranslateUpsertList.Clear();
                    }

                    // Log
                    if (LogTextList.Length > 0)
                    {
                        string logText = LogTextList.ToString();
                        LogTextList.Clear();
                        UtilServer.ServiceProvider = ServiceProvider; // Make sure method ServiceGet(); is available.
                        File.AppendAllText(UtilFramework.FileNameLog, logText);
                    }
                }
            }
            catch (Exception exception)
            {
                Logger.LogError(exception.ToString());
            }
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            Logger.LogInformation("Service stop");
            return base.StopAsync(cancellationToken);
        }
    }

    /// <summary>
    /// Exception for example if client send request to modify a field which IsReadOnly.
    /// </summary>
    internal class ExceptionSecurity : Exception
    {
        public ExceptionSecurity(string message) : base(message)
        {

        }
    }
}
