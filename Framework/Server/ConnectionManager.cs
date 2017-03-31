using Newtonsoft.Json;
using System;

namespace Framework.Server
{
    /// <summary>
    /// Server config json.
    /// </summary>
    public class Config
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
                return Util.FolderName + "Framework/Server/ConnectionManager.json"; // See also .gitignore
            }
        }

        public static string JsonTxtFileName
        {
            get
            {
                return Util.FolderName + "Framework/Server/ConnectionManager.json.txt"; // See also .gitignore
            }
        }

        public static Config Instance
        {
            get
            {
                string json = Util.FileRead(JsonFileName);
                var result = JsonConvert.DeserializeObject<Config>(json);
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

        public bool IsDebugJson;
    }

    public static class ConnectionManager
    {
        public static string ConnectionString
        {
            get
            {
                return Config.Instance.ConnectionStringGet();
            }
        }
    }
}
