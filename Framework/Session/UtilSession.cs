namespace Framework.Session
{
    using Framework.Application;
    using Framework.Session;
    using Framework.Server;
    using Microsoft.AspNetCore.Http;
    using Newtonsoft.Json;
    using System.Collections.Generic;

    internal static class UtilSession
    {
        /// <summary>
        /// Serialize session state.
        /// </summary>
        public static void Serialize(App app)
        {
            string json = JsonConvert.SerializeObject(app.AppSession, new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.All });
            UtilServer.Session.SetString("AppSession", json);
        }

        /// <summary>
        /// Deserialize session state.
        /// </summary>
        public static void Deserialize(App app)
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
            app.AppSession = appSession;
        }
    }
}
