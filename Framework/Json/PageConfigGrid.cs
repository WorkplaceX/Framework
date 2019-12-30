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

        protected internal override async Task InitAsync()
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
                var rowSelected = (FrameworkConfigGridDisplay)GridConfigGrid.GridRowSelected();
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
            if (grid == GridConfigGrid)
            {
                var rowDest = new FrameworkConfigGrid();
                rowDest.IsExist = true;
                Data.RowCopy(rowNew, rowDest);
                await Data.InsertAsync(rowDest);
                var rowReload = await GridConfigGridReload(rowNew);
                Data.RowCopy(rowReload, rowNew);
                return true;
            }
            if (grid == GridConfigField)
            {
                throw new Exception("Can not insert field config!");
            }
            return await base.GridInsertAsync(grid, rowNew, databaseEnum);
        }

        protected internal override async Task<bool> GridUpdateAsync(Grid grid, Row row, Row rowNew, DatabaseEnum databaseEnum)
        {
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
                    await Data.UpdateAsync(rowDest);
                }

                // Reload
                {
                    var rowDisplayReload = await GridConfigGridReload(rowNew);
                    Data.RowCopy(rowDisplayReload, rowDisplay);
                }
                return true;
            }
            if (grid == GridConfigField)
            {
                var rowDisplay = (FrameworkConfigFieldDisplay)rowNew;

                // ConfigGrid
                if (rowDisplay.ConfigGridId == null)
                {
                    var rowDisplayReload = await GridConfigFieldReload(row);
                    rowDisplay.ConfigGridId = rowDisplayReload.ConfigGridId;
                    if (rowDisplay.ConfigGridId == null)
                    {
                        var rowDest = new FrameworkConfigGrid();
                        Data.RowCopy(rowNew, rowDest, "ConfigGrid");
                        await Data.InsertAsync(rowDest);
                        rowDisplay.ConfigGridId = rowDest.Id;
                    }
                }

                // ConfigField
                if (rowDisplay.ConfigFieldId == null)
                {
                    var rowDest = new FrameworkConfigField();
                    Data.RowCopy(rowNew, rowDest, "ConfigField");
                    rowDest.ConfigGridId = rowDisplay.ConfigGridId.Value;
                    rowDest.FieldId = rowDisplay.FieldId;
                    await Data.InsertAsync(rowDest);
                    rowDisplay.ConfigFieldId = rowDest.Id;
                }
                else
                {
                    var rowDest = new FrameworkConfigField();
                    Data.RowCopy(rowNew, rowDest, "ConfigField");
                    await Data.UpdateAsync(rowDest);
                }

                // Reload
                {
                    var rowDisplayReload = await GridConfigFieldReload(rowNew);
                    Data.RowCopy(rowDisplayReload, rowDisplay);
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

        private async Task<FrameworkConfigFieldDisplay> GridConfigFieldReload(Row row)
        {
            var rowDisplay = (FrameworkConfigFieldDisplay)row;
            var result = (await Data.SelectAsync(Data.Query<FrameworkConfigFieldDisplay>().Where(
                item => item.ConfigGridTableId == rowDisplay.ConfigGridTableId &&
                item.ConfigGridConfigName == rowDisplay.ConfigGridConfigName &&
                item.FieldId == rowDisplay.FieldId))).Single();
            return result;
        }

        protected internal override async Task GridRowSelectedAsync(Grid grid)
        {
            if (grid == GridConfigGrid)
            {
                var configGrid = (FrameworkConfigGridDisplay)grid.GridRowSelected();
                await GridConfigField.LoadAsync();
            }
        }
    }
}
