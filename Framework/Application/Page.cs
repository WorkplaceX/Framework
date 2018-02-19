namespace Framework.Component
{
    using Framework.Application;
    using System;

    /// <summary>
    /// Application page.
    /// </summary>
    public class Page : Div
    {
        /// <summary>
        /// Create new page with method App.PageShow();
        /// </summary>
        public Page()
        {

        }

        /// <summary>
        /// Called only once, when page being created.
        /// </summary>
        protected virtual internal void InitJson(App app)
        {

        }

        /// <summary>
        /// Show page. Create if it doesn't exist.
        /// </summary>
        /// <param name="isPageVisibleRemove">If true, remove currently visible page and it's state.</param>
        public Page PageShow(App app, Type typePage, bool isPageVisibleRemove = true)
        {
            return app.PageShow(this.Owner(app.AppJson), typePage, isPageVisibleRemove);
        }

        /// <summary>
        /// Show page. Create if it doesn't exist.
        /// </summary>
        /// <param name="isPageVisibleRemove">If true, remove currently visible page and it's state.</param>
        public TPage PageShow<TPage>(App app, bool isPageVisibleRemove = true) where TPage : Page
        {
            return (TPage)app.PageShow(this.Owner(app.AppJson), typeof(TPage), isPageVisibleRemove);
        }

        protected virtual internal void RunBegin(App app)
        {

        }

        protected virtual internal void RunEnd(App app)
        {

        }
    }
}
