namespace Framework.Component
{
    using System;

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

        /// <summary>
        /// Gets or sets RequestUrl. This value is set by the server. For example: http://localhost:49323/config/app.json
        /// </summary>
        public string RequestUrl;

        /// <summary>
        /// Gets or sets BrowserUrl. This value is set by the browser. It can be different from RequestUrl if application runs embeded in another webpage.
        /// For example:  http://localhost:49323/config/data.txt
        /// </summary>
        public string BrowserUrl;

        /// <summary>
        /// Returns BrowserUrl. This value is set by the browser. It can be different from RequestUrl if application runs embeded in another webpage.
        /// For example: http://localhost:4200/
        /// </summary>
        public string BrowserUrlServer()
        {
            Uri uri = new Uri(BrowserUrl);
            string result = string.Format("{0}://{1}/", uri.Scheme, uri.Authority);
            return result;
        }
    }
}
