using Newtonsoft.Json;
using System;

namespace Framework.Server
{
    /// <summary>
    /// Server config json.
    /// </summary>
    public class ConfigServer
    {
        public string ConnectionStringDev;

        public string ConnectionStringTest;

        public string ConnectionStringProd;

        /// <summary>
        /// Current ConnectionString (DEV, TEST, PROD).
        /// </summary>
        public string ConnectionStringSwitch;

        public static string JsonFileName
        {
            get
            {
                if (UtilFramework.FolderNameIsIss == false)
                {
                    return UtilFramework.FolderName + "Submodule/Framework/Server/ConnectionManagerServer.json"; // See also .gitignore
                }
                else
                {
                    return UtilFramework.FolderName + "Server/ConnectionManagerServer.json"; // See also .gitignore
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
                return UtilFramework.FolderName + "Submodule/Framework/Server/ConnectionManagerServer.json.txt"; // See also .gitignore
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

        private string ConnectionStringGetSet(bool isSet, string value)
        {
            string result = null;
            switch (ConnectionStringSwitch)
            {
                case "ConnectionStringDev":
                    if (isSet == false)
                    {
                        result = ConnectionStringDev;
                    }
                    else
                    {
                        ConnectionStringDev = value;
                    }
                    break;
                case "ConnectionStringTest":
                    if (isSet == false)
                    {
                        result = ConnectionStringTest;
                    }
                    else
                    {
                        ConnectionStringTest = value;
                    }
                    break;
                case "ConnectionStringProd":
                    if (isSet == false)
                    {
                        result = ConnectionStringProd;
                    }
                    else
                    {
                        ConnectionStringProd = value;
                    }
                    break;
                default:
                    throw new Exception("ConnectionStringSwitch unknown!");
            }
            if (result == "")
            {
                result = null;
            }
            return result;
        }

        /// <summary>
        /// Returns ConnectionString for DEV, TEST, PROD.
        /// </summary>
        /// <returns></returns>
        public string ConnectionStringGet()
        {
            return ConnectionStringGetSet(false, null);
        }

        /// <summary>
        /// Sets ConnectionString for DEV, TEST, PROD.
        /// </summary>
        /// <param name="value"></param>
        public void ConnectionStringSet(string value)
        {
            ConnectionStringGetSet(true, value);
        }
    }

    public static class ConnectionManagerServer
    {
        public static string ConnectionString
        {
            get
            {
                return ConfigServer.Instance.ConnectionStringGet();
            }
        }
    }
}
