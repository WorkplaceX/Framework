namespace Framework.Component
{
    public class ComponentJson
    {
        /// <summary>
        /// Deserialization constructor.
        /// </summary>
        public ComponentJson() { }

        /// <summary>
        /// Programmatically constructor.
        /// </summary>
        public ComponentJson(ComponentJson owner)
        {

        }
    }

    public class App : ComponentJson
    {
        public App() { }

        public App(ComponentJson owner) 
            : base(owner)
        {
            this.Name = "Application";
            this.Version = UtilFramework.Version;
            this.VersionBuild = UtilFramework.VersionBuild;
        }

        public string Name { get; set; }

        public string Version { get; set; }

        public string VersionBuild { get; set; }

        public bool IsServerSideRendering { get; set; }
    }
}
