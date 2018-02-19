namespace Framework.Component
{
    using Framework.Application;
    using System;

    /// <summary>
    /// Application page.
    /// </summary>
    public class Page : Div
    {
        public Page() { }

        /// <summary>
        /// Constructor. Does not call method InitJson(); See also method App.PageShow();
        /// </summary>
        public Page(Component owner) 
            : base(owner)
        {
            
        }

        /// <summary>
        /// Called only once, when page being created.
        /// </summary>
        protected virtual internal void InitJson(App app)
        {

        }
    }
}
