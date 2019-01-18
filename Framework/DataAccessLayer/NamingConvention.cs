namespace Framework.DataAccessLayer
{
    public class NamingConvention
    {
        public bool IsVisible(string fieldName)
        {
            if (fieldName.EndsWith("Id"))
            {
                return false;
            }
            return true;
        }
    }
}
