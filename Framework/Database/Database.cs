using System;
using System.Collections.Generic;
using Framework.Application;
using Framework.DataAccessLayer;
using System.Linq;
using Framework.Component;
using Framework;
using Framework.Application.Config;
using Microsoft.EntityFrameworkCore;

namespace Database.dbo
{
    public partial class FrameworkLoginUserDisplay
    {
        /// <summary>
        /// AppConfig shows users from all applications.
        /// </summary>
        private static IQueryable<FrameworkLoginUserDisplay> Filter(IQueryable<FrameworkLoginUserDisplay> query, AppEventArg e)
        {
            var result = query;
            if (e.App.GetType() != typeof(AppConfig))
            {
                int applicationId = e.App.DbFrameworkApplication.Id;
                result = result.Where(item => item.UserApplicationId == applicationId);
            }
            return result;
        }

        protected override internal void Reload(AppEventArg e)
        {
            var query = UtilDataAccessLayer.Query<FrameworkLoginUserDisplay>().Where(item => item.UserId == UserId);
            query = Filter(query, e);
            FrameworkLoginUserDisplay rowReload = query.Single();
            UtilDataAccessLayer.RowCopy(rowReload, this);
        }

        protected internal override void Insert(Row rowNew, ref bool isReload, AppEventArg e)
        {
            FrameworkLoginUser user = UtilDataAccessLayer.RowCopy<FrameworkLoginUser>(this, "User");
            UtilDataAccessLayer.Insert(user);
            UserId = user.Id; // Read back new UserId for reload.
            isReload = true;
        }

        protected internal override void Update(Row row, Row rowNew, ref bool isReload, AppEventArg e)
        {
            FrameworkLoginUser user = UtilDataAccessLayer.RowCopy<FrameworkLoginUser>(row, "User");
            FrameworkLoginUser userNew = UtilDataAccessLayer.RowCopy<FrameworkLoginUser>(rowNew, "User");
            UtilDataAccessLayer.Update(user, userNew);
            isReload = true;
        }

        protected internal override IQueryable Query(GridName gridName, AppEventArg e)
        {
            var result = (IQueryable< FrameworkLoginUserDisplay>)base.Query(gridName, e);
            result = Filter(result, e);
            return result;
        }
    }

    public partial class FrameworkLoginUserDisplay_ApplicationText
    {
        protected internal override void Lookup(out GridNameType gridName, out IQueryable query, AppEventArg e)
        {
            gridName = FrameworkApplicationView.GridNameLookup;
            var result = UtilDataAccessLayer.Query<FrameworkApplicationView>();
            if (e.App.GetType() != typeof(AppConfig))
            {
                // Only AppConfig can add users of other applications.
                int applicationId = e.App.DbFrameworkApplication.Id;
                result = result.Where(item => item.Id == applicationId);
            }
            query = result;
        }

        protected internal override void LookupIsClick(Row rowLookup, AppEventArg e)
        {
            FrameworkApplicationView application = (FrameworkApplicationView)rowLookup;
            Row.UserApplicationId = application.Id;
            Row.ApplicationText = application.Text;
        }

        protected internal override void TextParse(ref string text, AppEventArg e)
        {
            base.TextParse(ref text, e);
            if (text != null)
            {
                string textLocal = text;
                FrameworkApplicationView application = UtilDataAccessLayer.Query<FrameworkApplicationView>().Where(item => item.Text == textLocal).FirstOrDefault();
                if (application != null)
                {
                    Row.UserApplicationId = application.Id;
                    Row.ApplicationText = application.Text;
                }
                else
                {
                    throw new Exception("Application not found!");
                }
            }
        }
    }

    public partial class FrameworkNavigationView_Button : Cell<FrameworkNavigationView>
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
            foreach (Navigation navigation in e.App.AppJson.ListAll().OfType<Navigation>())
            {
                navigation.ButtonIsClick(e);
            }
        }
    }

    public partial class FrameworkNavigationView
    {
        public static GridNameType GridNameConfig = new GridName<FrameworkNavigationView>("Config");

        protected override internal void Reload(AppEventArg e)
        {
            var row = UtilDataAccessLayer.Query<FrameworkNavigationView>().Where(item => item.Id == Id).Single();
            UtilDataAccessLayer.RowCopy(row, this);
        }

        protected internal override void Update(Row row, Row rowNew, ref bool isReload, AppEventArg e)
        {
            var rowNavigation = UtilDataAccessLayer.RowCopy<FrameworkNavigation>(row);
            var rowNewNavigation = UtilDataAccessLayer.RowCopy<FrameworkNavigation>(rowNew);
            UtilDataAccessLayer.Update(rowNavigation, rowNewNavigation); // Write to table. Not to view.
            //
            isReload = true;
        }

        protected internal override void Insert(Row rowNew, ref bool isReload, AppEventArg e)
        {
            var rowNewNavigation = UtilDataAccessLayer.RowCopy<FrameworkNavigation>(rowNew);
            rowNewNavigation.ComponentId = ((FrameworkNavigationView)rowNew).ComponentId;
            Id = UtilDataAccessLayer.Insert(rowNewNavigation).Id; // Write to table. Not to view.
            //
            isReload = true;
        }

        protected internal override IQueryable Query(GridName gridName, AppEventArg e)
        {
            return UtilDataAccessLayer.Query<FrameworkNavigationView>().OrderBy(item => item.Text);
        }

        [SqlColumn(null, typeof(FrameworkNavigationView_Button))]
        public string Button { get; set; }
    }

    public partial class FrameworkNavigationView_ComponentNameCSharp : Cell<FrameworkNavigationView>
    {
        protected internal override void Lookup(out GridNameType gridName, out IQueryable query, AppEventArg e)
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

        protected internal override void TextParse(ref string text, AppEventArg e)
        {
            base.TextParse(ref text, e);
            //
            if (e.Index.Enum == IndexEnum.Filter)
            {
                return; // No further parsing necessary like date.
            }
            if (text == null)
            {
                Row.ComponentId = null;
            }
            else
            {
                string textLocal = text;
                FrameworkComponent component = UtilDataAccessLayer.Query<FrameworkComponent>().Where(item => item.ComponentNameCSharp == textLocal).SingleOrDefault();
                if (component != null)
                {
                    Row.ComponentId = component.Id;
                    text = component.ComponentNameCSharp;
                    base.TextParse(ref text, e);
                }
                else
                {
                    throw new Exception("Component not found!");
                }
            }
        }

        protected internal override void TextParseAuto(ref string text, AppEventArg e)
        {
            base.TextParse(ref text, e);
            //
            if (e.Index.Enum == IndexEnum.Filter)
            {
                return; // No further parsing necessary like date.
            }
            if (text == null)
            {
                Row.ComponentId = null;
            }
            else
            {
                string textLocal = text;
                var componentList = UtilDataAccessLayer.Query<FrameworkComponent>().Where(item => EF.Functions.Like(item.ComponentNameCSharp, '%' + textLocal + '%')).Take(2).ToArray();
                if (componentList.Count() == 1)
                {
                    Row.ComponentId = componentList[0].Id;
                    text = componentList[0].ComponentNameCSharp;
                    base.TextParse(ref text, e);
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
        protected internal override void Insert(Row rowNew, ref bool isReload, AppEventArg e)
        {
            if (e.App.DbFrameworkApplication != null)
            {
                this.ApplicationId = e.App.DbFrameworkApplication.Id;
            }
            base.Insert(rowNew, ref isReload, e);
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
        public static GridNameType GridNameLookup = new GridName<FrameworkApplicationView>("Lookup");

        protected internal override void Update(Row row, Row rowNew, ref bool isReload, AppEventArg e)
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

        protected internal override void Insert(Row rowNew, ref bool isReload, AppEventArg e)
        {
            var application = new FrameworkApplication();
            UtilDataAccessLayer.RowCopy(this, application);
            UtilDataAccessLayer.Insert(application);
            UtilDataAccessLayer.RowCopy(UtilDataAccessLayer.Query<FrameworkApplicationView>().Where(item => item.Id == application.Id).Single(), this);
        }
    }

    public partial class FrameworkApplicationType : Row
    {
        public static GridNameType GridNameLookup { get { return new GridName<FrameworkApplicationType>("Lookup"); } }
    }

    public partial class FrameworkApplicationView_TypeName
    {
        protected internal override void TextParse(ref string text, AppEventArg e)
        {
            string textLocal = text;
            var applicationType = UtilDataAccessLayer.Query<FrameworkApplicationType>().Where(item => item.TypeName == textLocal).FirstOrDefault();
            if (applicationType == null)
            {
                throw new Exception(string.Format("Type unknown! ({0})", text));
            }
            Row.TypeName = applicationType.TypeName;
            Row.ApplicationTypeId = applicationType.Id;
        }

        protected internal override void Lookup(out GridNameType gridName, out IQueryable query, AppEventArg e)
        {
            gridName = FrameworkApplicationType.GridNameLookup;
            query = UtilDataAccessLayer.Query<FrameworkApplicationType>();
        }

        protected internal override void LookupIsClick(Row rowLookup, AppEventArg e)
        {
            Row.ApplicationTypeId = ((FrameworkApplicationType)rowLookup).Id;
        }
    }

    public partial class FrameworkConfigColumnView
    {
        protected internal override void Update(Row row, Row rowNew, ref bool isReload, AppEventArg e)
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

        protected internal override IQueryable Query(GridName gridName, AppEventArg e)
        {
            var configTable = e.App.GridData.RowSelected(new GridName<FrameworkConfigGridView>());
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
        protected internal override void Update(Row row, Row rowNew, ref bool isReload, AppEventArg e)
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