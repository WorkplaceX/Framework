namespace Framework.DataAccessLayer
{
    using System;
    using System.Text;

    public class NamingConvention
    {
        /// <summary>
        /// Override this method for custom column IsVisible.
        /// </summary>
        public virtual bool ColumnIsVisible(Type typeRow, string fieldName, bool isVisibleDefault, bool? isVisibleConfig)
        {
            bool result = isVisibleDefault;
            if (isVisibleConfig != null)
            {
                result = isVisibleConfig.Value;
            }
            return result;
        }

        internal bool ColumnIsVisibleInternal(Type typeRow, string fieldName, bool? isVisibleConfig)
        {
            bool isVisibleDefault = !fieldName.EndsWith("Id");
            var result = ColumnIsVisible(typeRow, fieldName, isVisibleDefault, isVisibleConfig);
            return result;
        }

        /// <summary>
        /// Override this column for custom column text.
        /// </summary>
        public virtual string ColumnText(Type typeRow, string fieldName, string textDefault, string textConfig)
        {
            string result = textDefault;
            if (textConfig != null)
            {
                result = textConfig;
            }
            return result;
        }

        internal string ColumnTextInternal(Type typeRow, string fieldName, string textConfig)
        {
            // Default
            StringBuilder textDefault = new StringBuilder();
            bool isLower = false;
            foreach (var item in fieldName)
            {
                if (isLower && char.IsUpper(item))
                {
                    textDefault.Append(" ");
                }
                isLower = char.IsLower(item);
                textDefault.Append(item);
            }

            string result = ColumnText(typeRow, fieldName, textDefault.ToString(), textConfig);

            return result;
        }
    }
}
