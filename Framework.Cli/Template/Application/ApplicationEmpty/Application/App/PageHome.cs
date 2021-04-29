namespace Application.Doc
{
    using Framework.Json;

    public class PageHome : Page
    {
        public PageHome(ComponentJson owner) : base(owner) 
        {
            var content = new Div(this) { CssClass = "content" };
            new Html(content) { TextHtml = "<h1>Home</h1>" };
        }
    }
}
