using System;
using System.Collections.Generic;
using Framework.Application;
using Framework.DataAccessLayer;
using System.Linq;
using Framework.Component;

namespace Database.dbo
{
    public partial class FrameworkFileStorage
    {
        [SqlColumn(null, typeof(FrameworkFileStorage_Download))]
        public string Download { get; set; }
    }

    public partial class FrameworkFileStorage_Download : Cell<FrameworkFileStorage>
    {
        protected internal override void DesignCell(App app, GridName gridName, Index index, DesignCell result)
        {
            result.CellEnum = Framework.Component.GridCellEnum.Html;
        }

        protected override internal void CellRowValueToText(App app, GridName gridName, Index index, ref string result)
        {
            result = null;
            if (index.Enum == IndexEnum.Index && Row.Name != null)
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
        protected internal override void Insert(App app, GridName gridName, Index index, Row row)
        {
            if (app.DbFrameworkApplication != null)
            {
                this.ApplicationId = app.DbFrameworkApplication.Id;
            }
            base.Insert(app, gridName, index, row);
        }
    }

    public partial class FrameworkFileStorage_Data : Cell<FrameworkFileStorage>
    {
        protected internal override void DesignCell(App app, GridName gridName, Index index, DesignCell result)
        {
            result.CellEnum = GridCellEnum.FileUpload;
        }

        protected override internal void CellRowValueToText(App app, GridName gridName, Index index, ref string result)
        {
            result = "File Upload";
        }
    }

    public partial class FrameworkApplicationView
    {
        protected internal override void Update(App app, GridName gridName, Index index, Row row, Row rowNew)
        {
            // Row
            var application = new FrameworkApplication();
            UtilDataAccessLayer.RowCopy(row, application);
            // RowNew
            var applicationNew = new FrameworkApplication();
            UtilDataAccessLayer.RowCopy(rowNew, applicationNew);
            //
            UtilDataAccessLayer.Update(application, applicationNew);
            UtilDataAccessLayer.RowCopy(UtilDataAccessLayer.Query<FrameworkApplicationView>().Where(item => item.Id == this.Id).First(), this);
        }

        protected internal override void Insert(App app, GridName gridName, Index index, Row row)
        {
            var application = new FrameworkApplication();
            UtilDataAccessLayer.RowCopy(this, application);
            UtilDataAccessLayer.Insert(application);
            UtilDataAccessLayer.RowCopy(UtilDataAccessLayer.Query<FrameworkApplicationView>().Where(item => item.Id == application.Id).First(), this);
        }
    }

    public partial class FrameworkApplicationView_Type
    {
        protected internal override void CellTextParse(App app, GridName gridName, Index index, string fieldName, string text)
        {
            var applicationType = UtilDataAccessLayer.Query<FrameworkApplicationType>().Where(item => item.Name == text).FirstOrDefault();
            if (applicationType == null)
            {
                throw new Exception(string.Format("Type unknown! ({0})", text));
            }
            Row.Type = applicationType.Name;
            Row.ApplicationTypeId = applicationType.Id;
        }

        protected internal override void CellLookup(App app, GridName gridName, Index index, string fieldName, out IQueryable query)
        {
            query = UtilDataAccessLayer.Query<FrameworkApplicationType>();
        }
    }

    public partial class FrameworkConfigColumnView
    {
        protected internal override void Update(App app, GridName gridName, Index index, Row row, Row rowNew)
        {
            FrameworkConfigColumn config = UtilDataAccessLayer.Query<FrameworkConfigColumn>().Where(item => item.Id == this.ConfigId).FirstOrDefault();
            if (config == null)
            {
                config = new FrameworkConfigColumn();
                UtilDataAccessLayer.RowCopy(rowNew, config);
                UtilDataAccessLayer.Insert(config);
            }
            else
            {
                FrameworkConfigColumn configNew = UtilDataAccessLayer.RowClone(config);
                UtilDataAccessLayer.RowCopy(rowNew, configNew);
                UtilDataAccessLayer.Update(config, configNew);
            }
            UtilDataAccessLayer.RowCopy(UtilDataAccessLayer.Query<FrameworkConfigColumnView>().Where(item => item.ColumnId == this.ColumnId).First(), this);
        }

        protected internal override void MasterIsClick(App app, GridName gridNameMaster, Row rowMaster, ref bool isReload)
        {
            FrameworkConfigTableView configTable = rowMaster as FrameworkConfigTableView;
            if (configTable != null)
            {
                isReload = true;
            }
        }

        protected internal override IQueryable Query(App app, GridName gridName)
        {
            var configTable = app.GridData.RowSelected(new GridName<FrameworkConfigTableView>());
            if (configTable != null)
            {
                return UtilDataAccessLayer.Query<FrameworkConfigColumnView>().Where(item => item.TableName == configTable.TableName);
            }
            else
            {
                return null;
            }
        }
    }

    public partial class FrameworkConfigTableView
    {
        protected internal override void Update(App app, GridName gridName, Index index, Row row, Row rowNew)
        {
            var config = UtilDataAccessLayer.Query<FrameworkConfigTable>().Where(item => item.Id == this.ConfigId).FirstOrDefault();
            if (config == null)
            {
                config = new FrameworkConfigTable();
                UtilDataAccessLayer.RowCopy(rowNew, config);
                UtilDataAccessLayer.Insert(config);
            }
            else
            {
                FrameworkConfigTable configNew = UtilDataAccessLayer.RowClone(config);
                UtilDataAccessLayer.RowCopy(rowNew, configNew);
                UtilDataAccessLayer.Update(config, configNew);
            }
            UtilDataAccessLayer.RowCopy(UtilDataAccessLayer.Query<FrameworkConfigTableView>().Where(item => item.TableId == this.TableId).First(), this);
        }
    }
}