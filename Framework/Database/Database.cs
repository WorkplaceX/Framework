using System;
using System.Collections.Generic;
using Framework.Application;
using Framework.DataAccessLayer;
using System.Linq;
using Framework.Component;
using Framework;
using Framework.Application.Config;

namespace Database.dbo
{
    public partial class FrameworkCmsNavigationView_Button : Cell<FrameworkCmsNavigationView>
    {
        protected internal override void ConfigCell(ConfigCell result, AppEventArg e)
        {
            result.CellEnum = GridCellEnum.Button;
        }

        protected internal override void RowValueToText(ref string result, AppEventArg e)
        {
            result = Row.Text;
        }

        protected internal override void ButtonIsClick(ref bool isReload, AppEventArg e)
        {
            Type type = UtilFramework.TypeFromName(Row.ComponentNameCSharp, e.App.TypeComponentInNamespaceList());
            Div divContent = e.App.AppJson.ListAll().OfType<CmsNavigation>().First().DivContent();
            if (type == null)
            {
                divContent.List.Clear();
            }
            if (UtilFramework.IsSubclassOf(type, typeof(Page)))
            {
                e.App.PageShow(divContent, type);
            }
            else
            {
                divContent.List.Clear();
                Component component = (Component)UtilFramework.TypeToObject(type);
                component.Constructor(divContent, null);
            }
        }
    }

    public partial class FrameworkCmsNavigationView
    {
        private void Reload()
        {
            var row = UtilDataAccessLayer.Query<FrameworkCmsNavigationView>().Where(item => item.Id == Id).Single();
            UtilDataAccessLayer.RowCopy(row, this);
        }

        protected internal override void Update(Row row, Row rowNew, AppEventArg e)
        {
            var rowNavigation = UtilDataAccessLayer.RowCopy<FrameworkCmsNavigation>(row);
            var rowNewNavigation = UtilDataAccessLayer.RowCopy<FrameworkCmsNavigation>(rowNew);
            UtilDataAccessLayer.Update(rowNavigation, rowNewNavigation); // Write to table. Not to view.
            //
            Reload();
        }

        protected internal override void Insert(Row rowNew, AppEventArg e)
        {
            var rowNewNavigation = UtilDataAccessLayer.RowCopy<FrameworkCmsNavigation>(rowNew);
            rowNewNavigation.ComponentId = ((FrameworkCmsNavigationView)rowNew).ComponentId;
            Id = UtilDataAccessLayer.Insert(rowNewNavigation).Id; // Write to table. Not to view.
            //
            Reload();
        }

        protected internal override IQueryable Query(App app, GridName gridName)
        {
            return UtilDataAccessLayer.Query<FrameworkCmsNavigationView>().OrderBy(item => item.Text);
        }

        [SqlColumn(null, typeof(FrameworkCmsNavigationView_Button))]
        public string Button { get; set; }
    }

    public partial class FrameworkCmsNavigationView_ComponentNameCSharp : Cell<FrameworkCmsNavigationView>
    {
        protected internal override void Lookup(out GridNameWithType gridName, out IQueryable query)
        {
            gridName = FrameworkComponent.GridNameLookup;
            if (Row.ComponentNameCSharp == null)
            {
                query = UtilDataAccessLayer.Query<FrameworkComponent>().OrderBy(item => item.ComponentNameCSharp);
            }
            else
            {
                query = UtilDataAccessLayer.Query<FrameworkComponent>().Where(item => item.ComponentNameCSharp.Contains(Row.ComponentNameCSharp)).OrderBy(item => item.ComponentNameCSharp);
            }
        }

        protected internal override void LookupIsClick(Row rowLookup, AppEventArg e)
        {
            Row.ComponentNameCSharp = ((FrameworkComponent)rowLookup).ComponentNameCSharp;
            Row.ComponentId = ((FrameworkComponent)rowLookup).Id;
        }

        protected internal override void TextParse(ref string text, bool isDeleteKey, AppEventArg e)
        {
            base.TextParse(ref text, isDeleteKey, e);
            //
            if (e.Index.Enum == IndexEnum.Filter)
            {
                return; // No further parsing ncessary like date.
            }
            if (text == null)
            {
                Row.ComponentId = null;
            }
            else
            {
                string textLocal = text;
                var query = UtilDataAccessLayer.Query<FrameworkComponent>().Where(item => item.ComponentNameCSharp.Contains(textLocal)).Take(2);
                var rowComponentList = query.ToArray();
                if (rowComponentList.Length == 1 && isDeleteKey == false)
                {
                    Row.ComponentId = rowComponentList[0].Id;
                    text = rowComponentList[0].ComponentNameCSharp;
                    base.TextParse(ref text, isDeleteKey, e);
                }
                else
                { 
                    throw new Exception("Component not found!");
                }
            }
        }
    }

    public partial class FrameworkComponent
    {
        public static GridName<FrameworkComponent> GridNameLookup = new GridName<FrameworkComponent>("Lookup");
    }

    public partial class FrameworkFileStorage
    {
        [SqlColumn(null, typeof(FrameworkFileStorage_Download))]
        public string Download { get; set; }
    }

    public partial class FrameworkFileStorage_Download : Cell<FrameworkFileStorage>
    {
        protected internal override void ConfigCell(ConfigCell result, AppEventArg e)
        {
            result.CellEnum = Framework.Component.GridCellEnum.Html;
        }

        protected internal override void RowValueToText(ref string result, AppEventArg e)
        {
            result = null;
            if (e.Index.Enum == IndexEnum.Index && Row.Name != null)
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
        protected internal override void Insert(Row rowNew, AppEventArg e)
        {
            if (e.App.DbFrameworkApplication != null)
            {
                this.ApplicationId = e.App.DbFrameworkApplication.Id;
            }
            base.Insert(rowNew, e);
        }
    }

    public partial class FrameworkFileStorage_Data : Cell<FrameworkFileStorage>
    {
        protected internal override void ConfigCell(ConfigCell result, AppEventArg e)
        {
            result.CellEnum = GridCellEnum.FileUpload;
        }

        protected internal override void RowValueToText(ref string result, AppEventArg e)
        {
            result = "File Upload";
        }
    }

    public partial class FrameworkApplicationView
    {
        protected internal override void Update(Row row, Row rowNew, AppEventArg e)
        {
            // Row
            var application = new FrameworkApplication();
            UtilDataAccessLayer.RowCopy(row, application);
            // RowNew
            var applicationNew = new FrameworkApplication();
            UtilDataAccessLayer.RowCopy(rowNew, applicationNew);
            //
            UtilDataAccessLayer.Update(application, applicationNew);
            UtilDataAccessLayer.RowCopy(UtilDataAccessLayer.Query<FrameworkApplicationView>().Where(item => item.Id == this.Id).Single(), this);
        }

        protected internal override void Insert(Row rowNew, AppEventArg e)
        {
            var application = new FrameworkApplication();
            UtilDataAccessLayer.RowCopy(this, application);
            UtilDataAccessLayer.Insert(application);
            UtilDataAccessLayer.RowCopy(UtilDataAccessLayer.Query<FrameworkApplicationView>().Where(item => item.Id == application.Id).Single(), this);
        }
    }

    public partial class FrameworkApplicationType : Row
    {
        public static GridNameWithType Lookup { get { return new GridName<FrameworkApplicationType>("Lookup"); } }
    }

    public partial class FrameworkApplicationView_Type
    {
        protected internal override void TextParse(ref string text, bool isDeleteKey, AppEventArg e)
        {
            string textLocal = text;
            var applicationType = UtilDataAccessLayer.Query<FrameworkApplicationType>().Where(item => item.Name == textLocal).FirstOrDefault();
            if (applicationType == null)
            {
                throw new Exception(string.Format("Type unknown! ({0})", text));
            }
            Row.Type = applicationType.Name;
            Row.ApplicationTypeId = applicationType.Id;
        }

        protected internal override void Lookup(out GridNameWithType gridName, out IQueryable query)
        {
            gridName = FrameworkApplicationType.Lookup;
            query = UtilDataAccessLayer.Query<FrameworkApplicationType>();
        }

        protected internal override void LookupIsClick(Row rowLookup, AppEventArg e)
        {
            Row.ApplicationTypeId = ((FrameworkApplicationType)rowLookup).Id;
        }
    }

    public partial class FrameworkConfigColumnView
    {
        protected internal override void Update(Row row, Row rowNew, AppEventArg e)
        {
            FrameworkConfigColumn config = UtilDataAccessLayer.Query<FrameworkConfigColumn>().Where(item => item.Id == this.ConfigId).SingleOrDefault();
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
            UtilDataAccessLayer.RowCopy(UtilDataAccessLayer.Query<FrameworkConfigColumnView>().Where(item => item.GridId == this.GridId && item.ColumnId == this.ColumnId).Single(), this); // Read back whole row to update ConfigId, if inserted.
        }

        protected internal override void MasterIsClick(GridName gridNameMaster, Row rowMaster, ref bool isReload, AppEventArg e)
        {
            FrameworkConfigGridView configTable = rowMaster as FrameworkConfigGridView;
            if (configTable != null)
            {
                isReload = true;
            }
        }

        protected internal override IQueryable Query(App app, GridName gridName)
        {
            var configTable = app.GridData.RowSelected(new GridName<FrameworkConfigGridView>());
            if (configTable != null)
            {
                return UtilDataAccessLayer.Query<FrameworkConfigColumnView>().Where(item => item.GridId == configTable.GridId && item.TableNameCSharp == configTable.TableNameCSharp);
            }
            else
            {
                return null;
            }
        }
    }

    public partial class FrameworkConfigGridView
    {
        protected internal override void Update(Row row, Row rowNew, AppEventArg e)
        {
            var config = UtilDataAccessLayer.Query<FrameworkConfigGrid>().Where(item => item.Id == this.ConfigId).SingleOrDefault();
            if (config == null)
            {
                config = new FrameworkConfigGrid();
                UtilDataAccessLayer.RowCopy(rowNew, config);
                UtilDataAccessLayer.Insert(config);
            }
            else
            {
                FrameworkConfigGrid configNew = UtilDataAccessLayer.RowClone(config);
                UtilDataAccessLayer.RowCopy(rowNew, configNew);
                UtilDataAccessLayer.Update(config, configNew);
            }
            UtilDataAccessLayer.RowCopy(UtilDataAccessLayer.Query<FrameworkConfigGridView>().Where(item => item.GridId == this.GridId && item.TableId == this.TableId).SingleOrDefault(), this);
        }
    }
}