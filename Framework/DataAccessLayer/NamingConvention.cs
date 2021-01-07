namespace Framework.DataAccessLayer
{
    using System;
    using System.Text;
    using static Framework.DataAccessLayer.UtilDalType;

    internal class NamingConvention
    {
        /// <summary>
        /// Override this method for custom column IsVisible.
        /// </summary>
        protected virtual bool ColumnIsVisible(Type typeRow, string fieldName, bool isVisibleDefault, bool? isVisibleConfig)
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
            var fieldNameCamelCase = new UtilFramework.CamelCase(fieldName);
            bool isVisibleDefault = !fieldNameCamelCase.EndsWith("Id") && !fieldNameCamelCase.EndsWith("IdName");
            var result = ColumnIsVisible(typeRow, fieldName, isVisibleDefault, isVisibleConfig);
            return result;
        }

        /// <summary>
        /// Override this method for custom column IsReadOnly.
        /// </summary>
        protected virtual bool ColumnIsReadOnly(Type typeRow, string fieldName, bool isReadOnlyDefault, bool? isReadOnlyConfig)
        {
            bool result = isReadOnlyDefault;
            if (isReadOnlyConfig != null)
            {
                result = isReadOnlyConfig.Value;
            }
            return result;
        }

        internal bool ColumnIsReadOnlyInternal(Type typeRow, string fieldName, bool? isReadOnlyConfig)
        {
            bool isReadOnlyDefault = isReadOnlyConfig.GetValueOrDefault(false);
            var result = ColumnIsReadOnly(typeRow, fieldName, isReadOnlyDefault, isReadOnlyConfig);
            return result;
        }

        /// <summary>
        /// Override this column for custom column text.
        /// </summary>
        protected virtual string ColumnText(Type typeRow, string fieldName, string textDefault, string textConfig)
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

        protected virtual double ColumnSort(Type typeRow, string fieldName, int sortDefault, double? sortConfig)
        {
            double result = sortDefault;
            if (sortConfig != null)
            {
                result = sortConfig.Value;
            }
            return result;
        }

        internal double ColumnSortInternal(Type typeRow, string fieldName, Field field, double? sortConfig)
        {
            int sortDefault = field.Sort;
            double result = ColumnSort(typeRow, fieldName, sortDefault, sortConfig);
            return result;
        }
    }
}
