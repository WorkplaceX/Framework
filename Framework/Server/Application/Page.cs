namespace Framework.Server.Application.Json
{
    using System;

    public class Page : Component
    {
        /// <summary>
        /// Create new page with method Application.PageShow();
        /// </summary>
        public Page()
        {

        }

        protected virtual internal void InitJson(App app)
        {

        }

        /// <summary>
        /// Show page. Create if it doesn't exist.
        /// </summary>
        /// <param name="isPageVisibleRemove">Remove currently visible page and it's state.</param>
        public Page PageShow(App app, Type typePage, bool isPageVisibleRemove = true)
        {
            return app.PageShow(this.Owner(app.AppJson), typePage, isPageVisibleRemove);
        }

        /// <summary>
        /// Show page. Create if it doesn't exist.
        /// </summary>
        /// <param name="isPageVisibleRemove">Remove currently visible page and it's state.</param>
        public TPage PageShow<TPage>(App app, bool isPageVisibleRemove = true) where TPage : Page
        {
            return (TPage)app.PageShow(this.Owner(app.AppJson), typeof(TPage), isPageVisibleRemove);
        }

        protected virtual internal void RunBegin(App app)
        {

        }

        protected virtual internal void RunEnd()
        {

        }
    }
}
