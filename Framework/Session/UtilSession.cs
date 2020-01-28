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

            Console.WriteLine(string.Format("JsonSession.Length={0:n0}; JsonClient.Length={1:n0};", jsonSession.Length, jsonClient.Length));
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
        /// Returns true, if expected request has been sent by client.
        /// </summary>
        public static bool Request<T>(AppJson appJson, RequestCommand command, out RequestJson requestJson, out T componentJson) where T : ComponentJson
        {
            bool result = false;
            requestJson = appJson.RequestJson;
            componentJson = (T)null;
            if (command == requestJson.Command)
            {
                result = true;
                componentJson = (T)appJson.Root.RootComponentJsonList[requestJson.ComponentId];
            }
            return result;
        }
    }
}
