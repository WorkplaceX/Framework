using System;
using System.Collections.Generic;
using Framework.Application;
using Framework.DataAccessLayer;
using System.Linq;

namespace Database.dbo
{
    public partial class FrameworkFileStorage
    {
        [SqlColumn(null, typeof(FrameworkFileStorage_Download))]
        public string Download { get; set; }
    }

    public partial class FrameworkFileStorage_Download : Cell<FrameworkFileStorage>
    {
        protected override internal void CellIsHtml(App app, string gridName, string index, ref bool result)
        {
            result = true;
        }

        protected override internal void CellValueToText(App app, string gridName, string index, ref string result)
        {
            result = null;
            if (UtilApplication.IndexEnumFromText(index) == IndexEnum.Index && Row.Name != null)
            {
                string fileNameOnly = Row.Name;
                if (Row.Name.Contains("/"))
                {
                    fileNameOnly = Row.Name.Substring(Row.Name.LastIndexOf("/") + 1);
                    if (fileNameOnly.Length == 0)
                    {
                        fileNameOnly = Row.Name;
                    }
                }
                result = string.Format("<a href={0} target='blank'>{1}</a>", Row.Name, fileNameOnly);
            }
        }
    }

    public partial class FrameworkFileStorage
    {
        protected internal override void Insert(App app)
        {
            if (app.DbFrameworkApplication != null)
            {
                this.ApplicationId = app.DbFrameworkApplication.Id;
            }
            base.Insert(app);
        }
    }

    public partial class FrameworkFileStorage_Data : Cell<FrameworkFileStorage>
    {
        protected override internal void CellIsFileUpload(App app, string gridName, string index, ref bool result)
        {
            result = true;
        }

        protected override internal void CellValueToText(App app, string gridName, string index, ref string result)
        {
            result = "File Upload";
        }
    }

    public partial class FrameworkApplicationView
    {
        protected internal override void Update(App app, Row row)
        {
            // Old row
            var application = new FrameworkApplication();
            UtilDataAccessLayer.RowCopy(row, application);
            // New row
            var applicationNew = new FrameworkApplication();
            UtilDataAccessLayer.RowCopy(this, applicationNew);
            //
            UtilDataAccessLayer.Update(application, applicationNew);
        }

        protected internal override void Insert(App app)
        {
            var application = new FrameworkApplication();
            UtilDataAccessLayer.RowCopy(this, application);
            UtilDataAccessLayer.Insert(application);
            this.Id = application.Id;
        }

        protected internal override void Select()
        {
            UtilDataAccessLayer.RowCopy(UtilDataAccessLayer.Select<FrameworkApplicationView>().Where(item => item.Id == this.Id).First(), this);
        }
    }

    public partial class FrameworkApplicationView_Type
    {
        protected internal override void CellTextParse(App app, string gridName, string index, ref string result)
        {
            string text = result;
            var applicationType = UtilDataAccessLayer.Select<FrameworkApplicationType>().Where(item => item.Name == text).FirstOrDefault();
            if (applicationType == null)
            {
                throw new Exception(string.Format("Type unknown! ({0})", result));
            }
            Row.ApplicationTypeId = applicationType.Id;
        }
    }

    public partial class FrameworkApplicationView_Type
    {
        protected internal override void CellLookUp(out Type typeRow, out List<Row> rowList)
        {
            typeRow = typeof(FrameworkApplicationType);
            rowList = UtilDataAccessLayer.Select(typeRow, null, null, false, 0, 5);
        }

        protected internal override void CellLookUpIsClick(Row row, ref string result)
        {
            result = ((FrameworkApplicationType)row).Name;
        }
    }

    public partial class FrameworkConfigColumnView
    {
        protected internal override void Update(App app, Row row)
        {
            FrameworkConfigColumn configColumn = UtilDataAccessLayer.Select<FrameworkConfigColumn>().Where(item => item.Id == this.ConfigId).FirstOrDefault();
            if (configColumn == null)
            {
                configColumn = new FrameworkConfigColumn();
                UtilDataAccessLayer.RowCopy(this, configColumn);
                UtilDataAccessLayer.Insert(configColumn);
            }
            else
            {
                UtilDataAccessLayer.RowCopy(this, configColumn);
                UtilDataAccessLayer.Update(configColumn, configColumn);
            }
        }
    }
}