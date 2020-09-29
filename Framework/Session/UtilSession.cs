namespace Framework.Session
{
    using Framework.Json;
    using Framework.Server;
    using Microsoft.AspNetCore.Http;
    using System;

    internal static class UtilSession
    {
        /// <summary>
        /// Serialize session state.
        /// </summary>
        public static void Serialize(AppJson appJson, out string jsonClient)
        {
            appJson.RequestJson = null;

            UtilStopwatch.TimeStart("Serialize");
            UtilJson.Serialize(appJson, out string jsonSession, out jsonClient);
            UtilStopwatch.TimeStop("Serialize");
            UtilServer.Session.SetString("AppInternal", jsonSession);

            UtilStopwatch.Log(string.Format("JsonSession.Length={0:n0}; JsonClient.Length={1:n0};", jsonSession.Length, jsonClient.Length));
        }

        /// <summary>
        /// Deserialize session state.
        /// </summary>
        public static AppJson Deserialize()
        {
            AppJson result = null;
            string json = UtilServer.Session.GetString("AppInternal");

            if (!string.IsNullOrEmpty(json)) // Not session expired.
            {
                UtilStopwatch.TimeStart("Deserialize");
                result = (AppJson)UtilJson.Deserialize(json);
                UtilStopwatch.TimeStop("Deserialize");
            }
            return result;
        }

        /// <summary>
        /// Returns true, if expected command has been sent by client.
        /// </summary>
        public static bool Request<T>(AppJson appJson, RequestCommandEnum command, out CommandJson commandJson, out T componentJson) where T : ComponentJson
        {
            bool result = false;
            commandJson = appJson.RequestJson.CommandGet();
            componentJson = (T)null;
            if (command == commandJson.CommandEnum)
            {
                result = true;
                componentJson = (T)appJson.Root.RootComponentJsonList[commandJson.ComponentId];
            }
            return result;
        }
    }
}
