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
            var result = default(T);

            var serviceProvider = ServiceProvider;
            if (serviceProvider == null && Context != null)
            {
                // Fallback
                serviceProvider = Context.RequestServices;
            }

            if (serviceProvider != null) // Otherwise running as cli
            {
                result = (T)serviceProvider.GetService(typeof(T));
            }

            return result;
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

        /// <summary>
        /// Returns true, if http request method is GET or HEAD.
        /// </summary>
        /// <returns></returns>
        public static bool RequestMethodIsGet()
        {
            var context = Context;
            return context.Request.Method == "GET" || context.Request.Method == "HEAD";
        }

        /// <summary>
        /// Returns true, if http request method is POST.
        /// </summary>
        /// <returns></returns>
        public static bool RequestMethodIsPost()
        {
            return Context.Request.Method == "POST";
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
            string result;
            try
            {
                using var client = new HttpClient();
                using var stringContent = new StringContent(json, Encoding.Unicode, "application/json");
                using var response = await client.PostAsync(url, stringContent); // Make sure Universal server is running.
                response.EnsureSuccessStatusCode();
                result = await response.Content.ReadAsStringAsync();
            }
            catch (HttpRequestException exception)
            {
                throw new Exception(string.Format("Http POST request failed! Make sure cli build command did run. Close node.exe ({0})", url), exception);
            }
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
        /// Data to be added to database by service.
        /// </summary>
        private List<FrameworkLanguageItem> LanguageItemUpsertList = new List<FrameworkLanguageItem>();

        /// <summary>
        /// (AppTypeName, LanguageName, ItemName)
        /// </summary>
        private ConcurrentDictionary<(string, string, string), FrameworkLanguageApp> LanguageAppList = new ConcurrentDictionary<(string, string, string), FrameworkLanguageApp>();

        /// <summary>
        /// Gets or sets LanguageAppIsLoad. If true, LanguageAppList gets loaded or reloaded.
        /// </summary>
        public bool LanguageAppIsLoad = true;

        /// <summary>
        /// Translate text into a different language.
        /// </summary>
        /// <param name="appTypeName">Translation belongs to this app.</param>
        /// <param name="languageName">Destination language. Can be null for no selected language.</param>
        /// <param name="itemName">Dixtionary key for this text.</param>
        /// <param name="textDefault">Default text.</param>
        /// <returns>Returns into languageName translated text. If no translation entry is found text is returned.</returns>
        public string Language(string appTypeName, string languageName, string itemName, string textDefault)
        {
            var result = textDefault;
            try
            {
                var row = LanguageAppList.GetOrAdd((appTypeName, languageName, itemName), (key) =>
                {
                    var rowNew = new FrameworkLanguageApp { LanguageAppTypeName = appTypeName, LanguageName = languageName, ItemName = itemName, ItemTextDefault = textDefault };
                    var rowApp = new FrameworkLanguageItem { AppTypeName = appTypeName, Name = itemName, TextDefault = textDefault };
                    LanguageItemUpsertList.Add(rowApp);
                    return rowNew;
                });

                // TextDefault changed
                if (textDefault != row.ItemTextDefault)
                {
                    var rowApp = new FrameworkLanguageItem { AppTypeName = appTypeName, Name = itemName, TextDefault = textDefault };
                    LanguageItemUpsertList.Add(rowApp);
                    var find = (appTypeName, itemName);
                    foreach (var item in LanguageAppList)
                    {
                        if ((item.Key.Item1, item.Key.Item3) == find) // (AppTypeName, ItemName), no LanguageName
                        {
                            item.Value.ItemTextDefault = textDefault;
                        }
                    }
                }

                if (row.TextText != null)
                {
                    result = row.TextText;
                }
            }
            catch (Exception exception)
            {
                var errorText = string.Format("{0} {1}", nameof(BackgroundFrameworkService), exception.ToString());
                Logger.LogError(errorText);
                LogText(errorText, isRequestContext: false);
            }
            return result;
        }

        /// <summary>
        /// Update language translate in memory dictionary.
        /// </summary>
        public void LanguageUpdate(string appTypeName, string languageName, string itemName, string textDefault, string text)
        {
            LanguageAppList[(appTypeName, languageName, itemName)] = new FrameworkLanguageApp { LanguageAppTypeName = appTypeName, LanguageName = languageName, ItemName = itemName, ItemTextDefault = textDefault, TextText = text };
        }

        /// <summary>
        /// Gets LogTextList. Contains log entries.
        /// </summary>
        private StringBuilder LogTextList = new StringBuilder();

        /// <summary>
        /// Log to file log.csv
        /// </summary>
        public void LogText(string text, bool isRequestContext = true)
        {
            if (isRequestContext)
            {
                LogTextList.AppendLine(text);
            }
            else
            {
                var logTime = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mmm:ss.fff");
                LogTextList.AppendLine($"{ UtilFramework.Version },=\"{ logTime }\",{ text }");
            }
        }

        /// <summary>
        /// Write to file log.csv
        /// </summary>
        public void LogTextFlush()
        {
            if (LogTextList.Length > 0)
            {
                string logText = LogTextList.ToString();
                LogTextList.Clear();
                UtilServer.ServiceProvider = ServiceProvider; // Make sure method ServiceGet(); is available.
                File.AppendAllText(UtilFramework.FileNameLog, logText);
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Logger.LogInformation("Service start");
            LogText("Service start", isRequestContext: false);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    TimeHeartbeat = DateTime.UtcNow.ToString("HH:mmm:ss");
                    // Logger.LogInformation(TimeHeartbeat);

                    // Load Language
                    if (LanguageAppIsLoad)
                    {
                        LanguageAppIsLoad = false;
                        // Load sql table FrameworkLanguageApp.
                        UtilServer.ServiceProvider = ServiceProvider; // Make sure method ServiceGet(); is available.
                        var languageAppList = (await Data.Query<FrameworkLanguageApp>().QueryExecuteAsync()).ToList();
                        LanguageAppList = new ConcurrentDictionary<(string, string, string), FrameworkLanguageApp>(languageAppList.ToDictionary(item => (item.LanguageAppTypeName, item.LanguageName, item.ItemName)));
                    }

                    await Task.Delay(1000);

                    // Update Language
                    if (LanguageItemUpsertList.Count > 0)
                    {
                        Logger.LogInformation("Update sql table FrameworkLanguageItem. ({0} Rows)", LanguageItemUpsertList.Count);
                        UtilServer.ServiceProvider = ServiceProvider; // Make sure method ServiceGet(); is available.
                        await UtilDalUpsert.UpsertAsync(LanguageItemUpsertList, new string[] { nameof(FrameworkLanguageItem.AppTypeName), nameof(FrameworkLanguageItem.Name) });
                        LanguageItemUpsertList.Clear();
                    }

                    // Update Log
                    LogTextFlush();
                }
                catch (Exception exception)
                {
                    var logText = exception.ToString();
                    Logger.LogError(logText);
                    LogText(logText, isRequestContext: false);
                }
            }
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            var logText = "Service stop";
            Logger.LogInformation(logText);
            LogText(logText, isRequestContext: false);
            LogTextFlush();
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
