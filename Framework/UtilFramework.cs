namespace Framework
{
    public class UtilFramework
    {
        /// <summary>
        /// Gets VersionServer.
        /// </summary>
        public static string VersionServer
        {
            get
            {
                // dotnet --version
                // 2.1.201
                return "v2.0 Server";
            }
        }

        /// <summary>
        /// Gets VersionClient. This is the expected client version.
        /// </summary>
        public static string VersionClient
        {
            get
            {
                // node --version
                // v8.11.3

                // npm --version
                // 6.2.0

                // ng --version
                // Angular CLI: 6.0.8
                return "v2.0 Client";
            }
        }
    }
}
