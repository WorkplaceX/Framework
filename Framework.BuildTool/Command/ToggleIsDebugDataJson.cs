namespace Framework.BuildTool
{
    using Newtonsoft.Json;
    using System;

    public class CommandToggleIsDebugDataJson : Command
    {
        public CommandToggleIsDebugDataJson() 
            : base("debugJson", "Write every response into Application.json file.")
        {
            this.Get = OptionAdd("-g|--get", "Get IsDebugJson");
        }

        public readonly Option Get;

        public override void Run()
        {
            if (Get.IsOn)
            {
                Server.Config config = Server.Config.Instance;
                Console.WriteLine(string.Format("IsDebugJson={0}", config.IsDebugJson));
            }
            else
            {
                Server.Config config = Server.Config.Instance;
                config.IsDebugJson = !config.IsDebugJson;
                string json = JsonConvert.SerializeObject(config, Formatting.Indented);
                Util.FileWrite(Server.Config.JsonFileName, json);
                Util.Log(string.Format("File updated. ({0})", Server.Config.JsonFileName));
            }
        }
    }
}
