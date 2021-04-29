namespace Application
{
    using Database.dbo;
    using Framework.Json;
    using System.Threading.Tasks;

    public class AppMain : AppJson
    {
        public override async Task InitAsync()
        {
            CssFrameworkEnum = CssFrameworkEnum.Bootstrap;

            await new GridHelloWorld(this).LoadAsync();
        }
    }

    public class GridHelloWorld : Grid<HelloWorld>
    {
        public GridHelloWorld(ComponentJson owner) 
            : base(owner)
        {

        }
    }
}
