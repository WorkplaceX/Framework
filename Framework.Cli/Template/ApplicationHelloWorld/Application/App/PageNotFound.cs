namespace Application.Doc
{
    using Framework.Json;

    public class PageNotFound : Page
    {
        public PageNotFound(ComponentJson owner) : base(owner) 
        {
            new Html(this) { TextHtml = "<h1>Page not Found</h1>" };
        }
    }
}
