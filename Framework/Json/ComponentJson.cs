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

    public class AppJson : ComponentJson
    {
        public AppJson() { }

        public AppJson(ComponentJson owner) 
            : base(owner)
        {

        }

        public string Name { get; set; }

        public string Version { get; set; }

        public string VersionBuild { get; set; }

        public bool IsServerSideRendering { get; set; }
    }
}
