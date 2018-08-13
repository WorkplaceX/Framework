namespace Framework.Session
{
    using Framework.Application;
    using Framework.Server;
    using Microsoft.AspNetCore.Http;
    using Newtonsoft.Json;

    internal static class UtilSession
    {
        /// <summary>
        /// Serialize session state.
        /// </summary>
        public static void Serialize(AppInternal appInternal)
        {
            string json = JsonConvert.SerializeObject(appInternal.AppSession, new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.All });
            UtilServer.Session.SetString("AppSession", json);
        }

        /// <summary>
        /// Deserialize session state.
        /// </summary>
        public static void Deserialize(AppInternal appInternal)
        {
            string json = UtilServer.Session.GetString("AppSession");
            AppSession appSession;
            if (string.IsNullOrEmpty(json))
            {
                appSession = new AppSession();
            }
            else
            {
                appSession = JsonConvert.DeserializeObject<AppSession>(json, new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.All });
            }
            appInternal.AppSession = appSession;
        }
    }
}
