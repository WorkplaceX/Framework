namespace Framework.Json
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Database.dbo;
    using Framework.DataAccessLayer;

    /// <summary>
    /// Page to configure data grid and columns.
    /// </summary>
    public class PageConfigGrid : BootstrapModal
    {
        public PageConfigGrid() : this(null) { }

        public PageConfigGrid(ComponentJson owner) : base(owner) { }

        public void Init(string tableNameCSharp, string configName, string fieldNameCSharp)
        {
            this.TableNameCSharp = tableNameCSharp;
            this.ConfigName = configName;
            this.FieldNameCSharp = fieldNameCSharp;
        }

        public string TableNameCSharp { get; set; }

        public string ConfigName { get; set; }

        public string FieldNameCSharp { get; set; }

        public override async Task InitAsync()
        {
            Init(true, false, isLarge: true);
            new Html(DivHeader) { TextHtml = "Config" };
            new Html(DivBody) { TextHtml = "<h1>Config Grid</h1>" };
            GridConfigGrid = new GridConfigGrid(DivBody);
            new Html(DivBody) { TextHtml = "<h1>Config Field</h1>" };
            GridConfigField = new GridConfigField(DivBody);

            await GridConfigGrid.LoadAsync();
        }

        public GridConfigGrid GridConfigGrid;

        /// <summary>
        /// Gets GridConfigGridSelected. Master row.
        /// </summary>
        public FrameworkConfigGridDisplay GridConfigGridRowSelected => (FrameworkConfigGridDisplay)GridConfigGrid.RowSelected;

        public GridConfigField GridConfigField;

        private async Task<FrameworkConfigGridDisplay> GridConfigGridReload(Row row)
        {
            var rowDisplay = (FrameworkConfigGridDisplay)row;
            var result = (await Data.SelectAsync(Data.Query<FrameworkConfigGridDisplay>().Where(
                item => item.TableId == rowDisplay.TableId && 
                item.ConfigName == rowDisplay.ConfigName))).Single();
            return result;
        }

        private async Task<FrameworkConfigFieldDisplay> GridConfigFieldReload(FrameworkConfigFieldDisplay row)
        {
            var result = (await Data.SelectAsync(Data.Query<FrameworkConfigFieldDisplay>().Where(
                item => item.ConfigGridTableId == row.ConfigGridTableId &&
                item.ConfigGridConfigName == row.ConfigGridConfigName &&
                item.FieldId == row.FieldId &&
                item.ConfigFieldInstanceName == row.ConfigFieldInstanceName ))).Single();
            return result;
        }
    }

    public class GridConfigGrid : Grid<FrameworkConfigGridDisplay>
    {
        public GridConfigGrid(ComponentJson owner) : base(owner) { }

        public string TableNameCSharp
        {
            get
            {
                return this.ComponentOwner<PageConfigGrid>().TableNameCSharp;
            }
        }

        private async Task<FrameworkConfigGridDisplay> Reload(FrameworkConfigGridDisplay row)
        {
            var result = (await Data.SelectAsync(Data.Query<FrameworkConfigGridDisplay>().Where(
                item => item.TableId == row.TableId &&
                item.ConfigName == row.ConfigName))).Single();
            return result;
        }

        protected override IQueryable<FrameworkConfigGridDisplay> Query()
        {
            var result = base.Query();
            if (TableNameCSharp != null)
            {
                result = result.Where(item => item.TableNameCSharp == TableNameCSharp && item.ConfigName == ConfigName);
            }
            return result;
        }

        /// <summary>
        /// Load detail grid (config field).
        /// </summary>
        protected internal override async Task RowSelectedAsync()
        {
            await this.ComponentOwner<PageConfigGrid>().GridConfigField.LoadAsync();
        }

        protected override async Task UpdateAsync(FrameworkConfigGridDisplay row, FrameworkConfigGridDisplay rowNew, DatabaseEnum databaseEnum, UpdateResult result)
        {
            // Insert
            bool isInsert = false;
            if (rowNew.Id == null)
            {
                var rowReload = await Reload(row);
                rowNew.Id = rowReload.Id;
                if (rowNew.Id == null)
                {
                    var rowDest = new FrameworkConfigGrid();
                    Data.RowCopy(rowNew, rowDest);
                    rowDest.IsExist = true;
                    await Data.InsertAsync(rowDest);
                    rowNew.Id = rowDest.Id;
                    isInsert = true;
                }
            }

            // Update
            if (isInsert == false)
            {
                var rowDest = new FrameworkConfigGrid();
                Data.RowCopy(rowNew, rowDest);
                rowDest.IsExist = true;
                await Data.UpdateAsync(rowDest);
            }

            // Reload
            {
                var rowDisplayReload = await Reload(rowNew);
                Data.RowCopy(rowDisplayReload, rowNew);
            }

            result.IsHandled = true;
        }

        protected override async Task InsertAsync(FrameworkConfigGridDisplay rowNew, DatabaseEnum databaseEnum, InsertResult result)
        {
            var rowDest = new FrameworkConfigGrid();
            Data.RowCopy(rowNew, rowDest);
            rowDest.IsExist = true;
            await Data.InsertAsync(rowDest);
            var rowReload = await Reload(rowNew);
            Data.RowCopy(rowReload, rowNew);

            result.IsHandled = true;
        }
    }

    public class GridConfigField : Grid<FrameworkConfigFieldDisplay>
    {
        public GridConfigField(ComponentJson owner) : base(owner) { }

        public string FieldNameCSharp
        {
            get
            {
                return this.ComponentOwner<PageConfigGrid>().FieldNameCSharp;
            }
        }

        /// <summary>
        /// Gets GridConfigGridSelected. Master row.
        /// </summary>
        public FrameworkConfigGridDisplay GridConfigGridRowSelected
        {
            get
            {
                return this.ComponentOwner<PageConfigGrid>().GridConfigGrid.RowSelected;
            }
        }

        private async Task<FrameworkConfigFieldDisplay> Reload(FrameworkConfigFieldDisplay row)
        {
            var result = (await Data.SelectAsync(Data.Query<FrameworkConfigFieldDisplay>().Where(
                item => item.ConfigGridTableId == row.ConfigGridTableId &&
                item.ConfigGridConfigName == row.ConfigGridConfigName &&
                item.FieldId == row.FieldId &&
                item.ConfigFieldInstanceName == row.ConfigFieldInstanceName))).Single();
            return result;
        }

        protected override IQueryable<FrameworkConfigFieldDisplay> Query()
        {
            var rowSelected = GridConfigGridRowSelected;
            var result = base.Query().Where(item => item.ConfigGridTableId == rowSelected.TableId && item.ConfigGridConfigName == rowSelected.ConfigName);
            if (FieldNameCSharp != null)
            {
                result = result.Where(item => item.FieldFieldNameCSharp == FieldNameCSharp);
            }
            return result;
        }

        protected override async Task UpdateAsync(FrameworkConfigFieldDisplay row, FrameworkConfigFieldDisplay rowNew, DatabaseEnum databaseEnum, UpdateResult result)
        {
            // ConfigGrid
            if (rowNew.ConfigGridId == null)
            {
                var rowDisplayReload = await Reload(row); // ConfigGrid row might have been added in the meantime, if multiple ConfigField rows are in the grid.
                rowNew.ConfigGridId = rowDisplayReload.ConfigGridId;
                if (rowNew.ConfigGridId == null)
                {
                    var rowDest = new FrameworkConfigGrid();
                    Data.RowCopy(rowNew, rowDest, "ConfigGrid");
                    rowDest.IsExist = true;
                    await Data.InsertAsync(rowDest);
                    rowNew.ConfigGridId = rowDest.Id;
                }
            }

            // ConfigField
            if (rowNew.ConfigFieldId == null)
            {
                var rowDest = new FrameworkConfigField();
                Data.RowCopy(rowNew, rowDest, "ConfigField");
                rowDest.ConfigGridId = rowNew.ConfigGridId.Value;
                rowDest.FieldId = rowNew.FieldId;
                rowDest.IsExist = true;
                await Data.InsertAsync(rowDest);
                rowNew.ConfigFieldId = rowDest.Id;
            }
            else
            {
                var rowDest = new FrameworkConfigField();
                Data.RowCopy(rowNew, rowDest, "ConfigField");
                await Data.UpdateAsync(rowDest);
            }

            // Reload
            {
                var rowDisplayReload = await Reload(rowNew);
                Data.RowCopy(rowDisplayReload, rowNew);
            }

            result.IsHandled = true;
        }

        protected override async Task InsertAsync(FrameworkConfigFieldDisplay rowNew, DatabaseEnum databaseEnum, InsertResult result)
        {
            rowNew.ConfigGridTableId = GridConfigGridRowSelected.TableId; // Master
            rowNew.ConfigGridConfigName = GridConfigGridRowSelected.ConfigName; // Master

            var rowDest = new FrameworkConfigField();
            Data.RowCopy(rowNew, rowDest, "ConfigField");
            if (GridConfigGridRowSelected.Id == null) // Master does not have FrameworkConfigGrid in database
            {
                var rowDestConfigGrid = new FrameworkConfigGrid();
                Data.RowCopy(GridConfigGridRowSelected, rowDestConfigGrid);
                rowDestConfigGrid.IsExist = true;
                await Data.InsertAsync(rowDestConfigGrid);
                GridConfigGridRowSelected.Id = rowDestConfigGrid.Id;
            }
            rowDest.ConfigGridId = GridConfigGridRowSelected.Id.Value; // Master

            // Lookup field
            string fieldNameCSharp = ((FrameworkConfigFieldDisplay)rowNew).FieldFieldNameCSharp; // Text entered by user.
            var fieldList = await Data.SelectAsync(Data.Query<FrameworkField>().Where(item => item.TableId == GridConfigGridRowSelected.TableId && item.FieldNameCSharp == fieldNameCSharp));
            if (fieldList.Count == 0)
            {
                throw new Exception("Field not found!");
            }
            int fieldId = fieldList.Single().Id;
            rowDest.FieldId = fieldId;
            rowNew.FieldId = fieldId;
            rowDest.IsExist = true;
            await Data.InsertAsync(rowDest);
            var rowReload = await Reload(rowNew);
            Data.RowCopy(rowReload, rowNew);

            result.IsHandled = true;
        }
    }
}
