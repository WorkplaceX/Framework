namespace Framework.ComponentJson
{
    using System;
    using System.Collections.Generic;

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
            Constructor(owner);
        }

        internal void Constructor(ComponentJson owner)
        {
            this.Type = GetType().Name;
            if (owner != null)
            {
                if (owner.List == null)
                {
                    owner.List = new List<ComponentJson>();
                }
                int count = 0;
                foreach (var item in owner.List)
                {
                    if (item.TrackBy.StartsWith(this.Type + "-"))
                    {
                        count += 1;
                    }
                }
                this.TrackBy = this.Type + "-" + count.ToString();
                owner.List.Add(this);
            }
        }

        public string Type;

        public string TrackBy;

        public bool IsHide;

        /// <summary>
        /// Gets or sets custom html style classes for this component.
        /// </summary>
        public string CssClass;

        /// <summary>
        /// Gets json list.
        /// </summary>
        public List<ComponentJson> List = new List<ComponentJson>();

        private void ListAll(List<ComponentJson> result)
        {
            result.AddRange(List);
            foreach (var item in List)
            {
                item.ListAll(result);
            }
        }

        public List<ComponentJson> ListAll()
        {
            List<ComponentJson> result = new List<ComponentJson>();
            ListAll(result);
            return result;
        }

        private void Owner(ComponentJson componentTop, ComponentJson componentSearch, ref ComponentJson result)
        {
            if (componentTop.List.Contains(componentSearch))
            {
                result = componentTop; // Owner
            }
            if (result == null)
            {
                foreach (var item in componentTop.List)
                {
                    item.Owner(item, componentSearch, ref result);
                    if (result != null)
                    {
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Returns owner of this json component.
        /// </summary>
        /// <param name="componentTop">Component to start search from top to down.</param>
        public ComponentJson Owner(ComponentJson componentTop)
        {
            ComponentJson result = null;
            Owner(componentTop, this, ref result);
            return result;
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

    /// <summary>
    /// Json Button. Rendered as html button element.
    /// </summary>
    public class Button : ComponentJson
    {
        public Button() : this(null) { }

        public Button(ComponentJson owner)
            : base(owner)
        {

        }

        public string Text;

        public bool IsClick;
    }

}
