namespace Framework.BuildTool
{
    using Newtonsoft.Json;
    using System;

    public class CommandConnectionString : Command
    {
        public CommandConnectionString()
            : base("connection", "Database connection string")
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
            string connectionStringSwitch = Server.Config.Instance.ConnectionStringSwitch;
            string connectionString = Server.ConnectionManager.ConnectionString;
            UtilFramework.Log(string.Format("{0}={1}", connectionStringSwitch, connectionString));
        }

        private void ConnectionStringSet(string connectionString)
        {
            Server.Config config = Server.Config.Instance;
            config.ConnectionStringSet(connectionString);
            string json = JsonConvert.SerializeObject(config, Formatting.Indented);
            UtilFramework.FileWrite(Server.Config.JsonFileName, json);
            UtilFramework.Log(string.Format("File updated. ({0})", Server.Config.JsonFileName));
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
