namespace Framework.Session
{
    using Framework.Dal;
    using System.Collections.Generic;

    internal class AppSession
    {
        public List<GridSession> GirdList;
    }

    internal class GridSession
    {
        public List<Row> RowList;
    }
}
