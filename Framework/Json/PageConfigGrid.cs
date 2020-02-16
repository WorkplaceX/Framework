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
            GridConfigGrid = new Grid(DivBody);
            new Html(DivBody) { TextHtml = "<h1>Config Field</h1>" };
            GridConfigField = new Grid(DivBody);

            await GridConfigGrid.LoadAsync();
        }

        public Grid GridConfigGrid;

        /// <summary>
        /// Gets GridConfigGridSelected. Master row.
        /// </summary>
        public FrameworkConfigGridDisplay GridConfigGridRowSelected => (FrameworkConfigGridDisplay)GridConfigGrid.RowSelected;

        public Grid GridConfigField;

        protected internal override IQueryable GridQuery(Grid grid)
        {
            if (grid == GridConfigGrid)
            {
                var result = Data.Query<FrameworkConfigGridDisplay>();
                if (TableNameCSharp != null)
                {
                    result = result.Where(item => item.TableNameCSharp == TableNameCSharp && item.ConfigName == ConfigName);
                }
                return result;
            }
            if (grid == GridConfigField)
            {
                var rowSelected = GridConfigGridRowSelected;
                var result = Data.Query<FrameworkConfigFieldDisplay>().Where(item => item.ConfigGridTableId == rowSelected.TableId && item.ConfigGridConfigName == rowSelected.ConfigName);
                if (FieldNameCSharp != null)
                {
                    result = result.Where(item => item.FieldFieldNameCSharp == FieldNameCSharp);
                }
                return result;
            }
            return base.GridQuery(grid);
        }

        protected internal override async Task<bool> GridInsertAsync(Grid grid, Row rowNew, DatabaseEnum databaseEnum)
        {
            // ConfigGrid
            if (grid == GridConfigGrid)
            {
                var rowDest = new FrameworkConfigGrid();
                Data.RowCopy(rowNew, rowDest);
                rowDest.IsExist = true;
                await Data.InsertAsync(rowDest);
                var rowReload = await GridConfigGridReload(rowNew);
                Data.RowCopy(rowReload, rowNew);
                return true;
            }

            // ConfigField
            if (grid == GridConfigField)
            {
                var rowNewDisplay = (FrameworkConfigFieldDisplay)rowNew;
                rowNewDisplay.ConfigGridTableId = GridConfigGridRowSelected.TableId; // Master
                rowNewDisplay.ConfigGridConfigName = GridConfigGridRowSelected.ConfigName; // Master

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
                rowNewDisplay.FieldId = fieldId;
                rowDest.IsExist = true;
                await Data.InsertAsync(rowDest);
                var rowReload = await GridConfigFieldReload(rowNewDisplay);
                Data.RowCopy(rowReload, rowNew);
                return true;
            }
            return await base.GridInsertAsync(grid, rowNew, databaseEnum);
        }

        protected internal override async Task<bool> GridUpdateAsync(Grid grid, Row row, Row rowNew, DatabaseEnum databaseEnum)
        {
            // ConfigGrid
            if (grid == GridConfigGrid)
            {
                // Insert
                bool isInsert = false;
                var rowDisplay = (FrameworkConfigGridDisplay)rowNew;
                if (rowDisplay.Id == null)
                {
                    var rowDisplayReload = await GridConfigGridReload(row);
                    rowDisplay.Id = rowDisplayReload.Id;
                    if (rowDisplay.Id == null)
                    {
                        var rowDest = new FrameworkConfigGrid();
                        Data.RowCopy(rowNew, rowDest);
                        rowDest.IsExist = true;
                        await Data.InsertAsync(rowDest);
                        rowDisplay.Id = rowDest.Id;
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
                    var rowDisplayReload = await GridConfigGridReload(rowNew);
                    Data.RowCopy(rowDisplayReload, rowDisplay);
                }
                return true;
            }

            // ConfigField
            if (grid == GridConfigField)
            {
                var rowDisplay = (FrameworkConfigFieldDisplay)row;
                var rowNewDisplay = (FrameworkConfigFieldDisplay)rowNew;

                // ConfigGrid
                if (rowNewDisplay.ConfigGridId == null)
                {
                    var rowDisplayReload = await GridConfigFieldReload(rowDisplay); // ConfigGrid row might have been added in the meantime, if multiple ConfigField rows are in the grid.
                    rowNewDisplay.ConfigGridId = rowDisplayReload.ConfigGridId;
                    if (rowNewDisplay.ConfigGridId == null)
                    {
                        var rowDest = new FrameworkConfigGrid();
                        Data.RowCopy(rowNew, rowDest, "ConfigGrid");
                        rowDest.IsExist = true;
                        await Data.InsertAsync(rowDest);
                        rowNewDisplay.ConfigGridId = rowDest.Id;
                    }
                }

                // ConfigField
                if (rowNewDisplay.ConfigFieldId == null)
                {
                    var rowDest = new FrameworkConfigField();
                    Data.RowCopy(rowNew, rowDest, "ConfigField");
                    rowDest.ConfigGridId = rowNewDisplay.ConfigGridId.Value;
                    rowDest.FieldId = rowNewDisplay.FieldId;
                    rowDest.IsExist = true;
                    await Data.InsertAsync(rowDest);
                    rowNewDisplay.ConfigFieldId = rowDest.Id;
                }
                else
                {
                    var rowDest = new FrameworkConfigField();
                    Data.RowCopy(rowNew, rowDest, "ConfigField");
                    await Data.UpdateAsync(rowDest);
                }

                // Reload
                {
                    var rowDisplayReload = await GridConfigFieldReload(rowNewDisplay);
                    Data.RowCopy(rowDisplayReload, rowNewDisplay);
                }

                return true;
            }
            return await base.GridUpdateAsync(grid, row, rowNew, databaseEnum);
        }

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

        protected internal override async Task GridRowSelectedAsync(Grid grid)
        {
            if (grid == GridConfigGrid)
            {
                await GridConfigField.LoadAsync();
            }
        }
    }
}
