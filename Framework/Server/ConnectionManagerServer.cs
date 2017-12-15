using Newtonsoft.Json;
using System;
using System.Reflection;

namespace Framework.Server
{
    /// <summary>
    /// Server config json. Read and write values from file "Server\ConnectionManagerServer.json". See also class ConnectionManagerServer.
    /// </summary>
    public class ConfigServer
    {
        /// <summary>
        /// Gets or sets ConnectionString for Application database.
        /// </summary>
        public string ConnectionStringApplication;

        /// <summary>
        /// Gets or sets ConnectionString for Framework database.
        /// </summary>
        public string ConnectionStringFramework;

        public static string JsonFileName
        {
            get
            {
                if (UtilFramework.FolderNameIsIss == false)
                {
                    return UtilFramework.FolderName + "Server/ConnectionManagerServer.json"; // See also .gitignore
                }
                else
                {
                    return UtilFramework.FolderName + "ConnectionManagerServer.json"; // See also .gitignore
                }
            }
        }

        /// <summary>
        /// Gets JsonTxtFileName. Used as template, if file ConnectionManagerServer.json does not exist.
        /// </summary>
        public static string JsonTxtFileName
        {
            get
            {
                return UtilFramework.FolderName + "Submodule/Framework/Server/ConnectionManagerServer.json.txt";
            }
        }

        public static ConfigServer Instance
        {
            get
            {
                string json = UtilFramework.FileRead(JsonFileName);
                var result = JsonConvert.DeserializeObject<ConfigServer>(json);
                return result;
            }
        }

        private string ConnectionStringGetSet(bool isSet, string value, bool isFrameworkDb)
        {
            // Set
            if (isSet)
            {
                if (isFrameworkDb == false)
                {
                    ConnectionStringApplication = value;
                }
                else
                {
                    ConnectionStringFramework = value;
                }
            }
            // Get
            if (isFrameworkDb == false)
            {
                return ConnectionStringApplication;
            }
            else
            {
                return ConnectionStringFramework;
            }
        }

        /// <summary>
        /// Returns ConnectionString.
        /// </summary>
        public string ConnectionStringGet(bool isFrameworkDb)
        {
            return ConnectionStringGetSet(false, null, isFrameworkDb);
        }

        /// <summary>
        /// Sets ConnectionString.
        /// </summary>
        public void ConnectionStringSet(string value, bool isFrameworkDb)
        {
            ConnectionStringGetSet(true, value, isFrameworkDb);
        }
    }

    /// <summary>
    /// Gets values from file "Server/ConnectionManagerServer.json"
    /// </summary>
    public static class ConnectionManagerServer
    {
        /// <summary>
        /// Returns ConnectionString for Application or Framework database.
        /// </summary>
        /// <param name="isFrameworkDb">If true, Framework database (ConnectionStringFramework) otherwise Application database (ConnectionString) is returned. </param>
        public static string ConnectionString(bool isFrameworkDb)
        {
            return ConfigServer.Instance.ConnectionStringGet(isFrameworkDb);
        }

        /// <summary>
        /// Returns ConnectionString for Application or Framework database.
        /// </summary>
        /// <param name="typeRow">Application or Framework data row.</param>
        public static string ConnectionString(Type typeRow)
        {
            bool isFrameworkDb = typeRow.GetTypeInfo().Assembly == typeof(ConnectionManagerServer).Assembly; // Type is declared in Framework assembly.
            return ConnectionString(isFrameworkDb);
        }
    }
}
