namespace Framework.BuildTool
{
    using Framework.Server;
    using Newtonsoft.Json;
    using System;

    public class CommandConnectionString : Command
    {
        public CommandConnectionString()
            : base("connection", "Set database connection string (also --get and --check)")
        {
            this.ConnectionString = ArgumentAdd("connectionString", "Set ConnectionString");
            this.OptionGet = OptionAdd("-g|--get", "Get ConnectionString");
            this.OptionCheck = OptionAdd("-c|--check", "Test ConnectionString");
        }

        public readonly Argument ConnectionString;

        public readonly Option OptionGet;

        public readonly Option OptionCheck;

        private void ConnectionStringGet()
        {
            ConnectionManagerCheck.JsonFileCreateIfNotExists();
            //
            string connectionStringApplication = ConnectionManagerServer.ConnectionString(false);
            string connectionStringFramework = ConnectionManagerServer.ConnectionString(true);
            UtilFramework.Log(string.Format("ConnectionStringApplication={0};", connectionStringApplication));
            UtilFramework.Log(string.Format("ConnectionStringFramework={0};", connectionStringFramework));
        }

        private void ConnectionStringSet(string connectionString)
        {
            ConnectionManagerCheck.JsonFileCreateIfNotExists();
            //
            ConfigServer configServer = ConfigServer.Instance;
            configServer.ConnectionStringSet(connectionString, false);
            configServer.ConnectionStringSet(connectionString, true);
            string json = JsonConvert.SerializeObject(configServer, Formatting.Indented);
            UtilFramework.FileWrite(ConfigServer.JsonFileName, json);
            UtilFramework.Log(string.Format("File updated. ({0})", ConfigServer.JsonFileName));
        }

        private void ConnectionStringCheck()
        {
            ConnectionManagerCheck.ConnectionStringCheck();
        }

        public override void Run()
        {
            if (OptionGet.IsOn)
            {
                ConnectionStringGet();
                return;
            }
            if (OptionCheck.IsOn)
            {
                ConnectionStringCheck();
                return;
            }
            if (ConnectionString.Value != null)
            {
                ConnectionStringSet(ConnectionString.Value);
                return;
            }
        }
    }
}
