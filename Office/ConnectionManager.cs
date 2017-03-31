namespace Office
{
    using System;
    using System.Collections.Generic;
    using System.Data.SqlClient;
    using System.IO;
    using System.Reflection;

    /// <summary>
    /// ConnectionManager manages connection to database and local paths.
    /// </summary>
    internal static class ConnectionManager
    {
        private static string excelLocalFolder;

        /// <summary>
        /// Gets local folder which is scaned for (*.xlsx) files.
        /// </summary>
        public static string ExcelLocalFolder
        {
            get
            {
                if (excelLocalFolder == null)
                {
                    excelLocalFolder = @"C:\Temp\";
                }
                return excelLocalFolder;
            }
            set
            {
                excelLocalFolder = value;
            }
        }

        /// <summary>
        /// Truncate excel cell with longer text.
        /// </summary>
        public static readonly int ExcelVaueTextLengthMax = 256;

        private static Dictionary<string, string> connectionStringList;

        /// <summary>
        /// Gets list of defined ConnectionString. (ConnectionKey, ConnectionString).
        /// </summary>
        public static Dictionary<string, string> ConnectionStringList
        {
            get
            {
                if (connectionStringList == null)
                {
                    connectionStringList = new Dictionary<string, string>();
                    connectionStringList.Add("Default", @"Data Source=.\SQLEXPRESS;Initial Catalog=Main;Integrated Security=True");
                    connectionStringList.Add("SqlExpress (Local, Main)", @"Data Source=.\SQLEXPRESS;Initial Catalog=Main;Integrated Security=True");
                }
                return connectionStringList;
            }
            set
            {
                connectionStringList = value;
            }
        }

        private static string connectionKey;

        public static string ConnectionKey
        {
            get
            {
                return connectionKey;
            }
            set
            {
                // Util.Assert(connection == null); // Can not change ConnectionKey once Connection it is initialized.
                connection = null; // See also SqlConnection.ClearAllPools();
                connectionKey = value;
            }
        }

        private static SqlConnection connection;

        /// <summary>
        /// Gets connection to database.
        /// </summary>
        public static SqlConnection Connection
        {
            get
            {
                if (connection == null)
                {
                    connection = new SqlConnection(ConnectionStringList[ConnectionKey]);
                    connection.Open();
                }
                return connection;
            }
        }

        /// <summary>
        /// Returns content of file which has been embedded with "Build Action Resource".
        /// </summary>
        private static string Resource(string fileName)
        {
            string result = null;
            // Implementation for console application.
            {
                string assemblyName = typeof(ConnectionManager).Assembly.GetName().Name;
                string fileNameResource = assemblyName + "." + fileName.Replace("/", ".");
                using (Stream stream = typeof(ConnectionManager).Assembly.GetManifestResourceStream(fileNameResource))
                {
                    using (StreamReader streamReader = new StreamReader(stream))
                    {
                        result = streamReader.ReadToEnd();
                    }
                }
            }
            // Implementation for application.
            {
                // string assemblyName = typeof(ConnectionManager).Assembly.GetName().Name;
                // StreamResourceInfo info = Application.GetResourceStream(new Uri(assemblyName + ";component/" + fileName, UriKind.Relative));
                // string result = new StreamReader(info.Stream).ReadToEnd();
            }
            //
            return result;
        }

        /// <summary>
        /// Returns content of sql file which has been embedded with "Build Action Resource". And replaces prefix.
        /// </summary>
        public static string SqlResource(string fileName)
        {
            fileName = "SqlExcelToSql/" + fileName;
            string result = Resource(fileName);
            result = ConnectionManager.SqlPrefix(result);
            return result;
        }

        /// <summary>
        /// Returns sql statement with prefix.
        /// </summary>
        public static string SqlPrefix(string value)
        {
            return Util.Replace(value, "Temp", ConnectionManager.Prefix);
        }

        public delegate void LogEventHandler(string text);

        public static event LogEventHandler Log;

        public static void OnLog(string text, bool isError)
        {
            if (isError)
            {
                text = string.Format("Error: {0}", text);
            }
            if (Log != null)
            {
                Log(text);
            }
        }

        /// <summary>
        /// Write message to log.
        /// </summary>
        public static void OnLog(string text)
        {
            OnLog(text, false);
        }

        private static string prefix;

        /// <summary>
        /// Gets Prefix for sql tables and views.
        /// </summary>
        public static string Prefix
        {
            get
            {
                if (prefix == null)
                {
                    prefix = "Temp";
                }
                return prefix;
            }
            set
            {
                prefix = value;
            }
        }

        /// <summary>
        /// Use for initialization only. Do not call while script is running!
        /// </summary>
        public static void PrefixSet(string value)
        {
            prefix = value;
        }

        /// <summary>
        /// Write all properties of ConnectionManager to Log.
        /// </summary>
        public static void LogThis()
        {
            OnLog("ConnectionManager");
            SortedDictionary<string, string> valueList = new SortedDictionary<string, string>();
            foreach (PropertyInfo propertyInfo in typeof(ConnectionManager).GetProperties())
            {
                if (propertyInfo.PropertyType == typeof(string))
                {
                    string value = (string)propertyInfo.GetValue(null, null);
                    valueList.Add(propertyInfo.Name, value);
                }
            }
            //
            foreach (string key in valueList.Keys)
            {
                OnLog(string.Format("-{0}={1};", key, valueList[key]));
            }
        }
    }
}
