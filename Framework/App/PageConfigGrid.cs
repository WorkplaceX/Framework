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
    public class PageConfigGrid : PageModal
    {
        public PageConfigGrid(ComponentJson owner, string tableNameCSharp, string fieldNameCSharp) 
            : base(owner) 
        {
            TableNameCSharp = tableNameCSharp;
            FieldNameCSharp = fieldNameCSharp;
        }

        public string TableNameCSharp { get; set; }

        public string FieldNameCSharp { get; set; }

        public override async Task InitAsync()
        {
            new Html(DivHeader) { TextHtml = "<h1>Config</h1>" };
            new Html(DivBody) { TextHtml = "<h2>Config Grid</h2>" };
            GridConfigGrid = new GridConfigGrid(DivBody);
            new Html(DivBody) { TextHtml = "<h2>Config Field</h2>" };
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
            var result = (await Data.Query<FrameworkConfigGridDisplay>().Where(
                item => item.TableId == rowDisplay.TableId && 
                item.ConfigName == rowDisplay.ConfigName).QueryExecuteAsync()).Single();
            return result;
        }

        private async Task<FrameworkConfigFieldDisplay> GridConfigFieldReload(FrameworkConfigFieldDisplay row)
        {
            var result = (await Data.Query<FrameworkConfigFieldDisplay>().Where(
                item => item.ConfigGridTableId == row.ConfigGridTableId &&
                item.ConfigGridConfigName == row.ConfigGridConfigName &&
                item.FieldId == row.FieldId &&
                item.ConfigFieldInstanceName == row.ConfigFieldInstanceName).QueryExecuteAsync()).Single();
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
            var result = (await Data.Query<FrameworkConfigGridDisplay>().Where(
                item => item.TableId == row.TableId &&
                item.ConfigName == row.ConfigName).QueryExecuteAsync()).Single();
            return result;
        }

        protected override void Query(QueryArgs args, QueryResult result)
        {
            if (TableNameCSharp != null)
            {
                result.Query = args.Query.Where(item => item.TableNameCSharp == TableNameCSharp);
            }
        }

        /// <summary>
        /// Load detail grid (config field).
        /// </summary>
        protected internal override async Task RowSelectedAsync()
        {
            await this.ComponentOwner<PageConfigGrid>().GridConfigField.LoadAsync();
        }

        protected override async Task UpdateAsync(UpdateArgs args, UpdateResult result)
        {
            // Insert
            bool isInsert = false;
            if (args.Row.Id == null)
            {
                var rowReload = await Reload(args.RowOld);
                args.Row.Id = rowReload.Id;
                if (args.Row.Id == null)
                {
                    var rowDest = new FrameworkConfigGrid();
                    Data.RowCopy(args.Row, rowDest);
                    rowDest.IsExist = true;
                    await Data.InsertAsync(rowDest);
                    args.Row.Id = rowDest.Id;
                    isInsert = true;
                }
            }

            // Update
            if (isInsert == false)
            {
                var rowDest = new FrameworkConfigGrid();
                Data.RowCopy(args.Row, rowDest);
                rowDest.IsExist = true;
                await Data.UpdateAsync(rowDest);
            }

            // Reload
            {
                var rowDisplayReload = await Reload(args.Row);
                Data.RowCopy(rowDisplayReload, args.Row);
            }

            result.IsHandled = true;
        }

        protected override async Task InsertAsync(InsertArgs args, InsertResult result)
        {
            var rowDest = new FrameworkConfigGrid();
            Data.RowCopy(args.Row, rowDest);
            int tableId = (await Data.Query<FrameworkTable>().Where(item => item.TableNameCSharp == TableNameCSharp).QueryExecuteAsync()).Single().Id;
            rowDest.TableId = tableId;
            rowDest.IsExist = true;
            await Data.InsertAsync(rowDest);
            Data.RowCopy(rowDest, result.Row);
            var rowReload = await Reload(result.Row);
            Data.RowCopy(rowReload, result.Row);

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
            var result = (await Data.Query<FrameworkConfigFieldDisplay>().Where(
                item => item.ConfigGridTableId == row.ConfigGridTableId &&
                item.ConfigGridConfigName == row.ConfigGridConfigName &&
                item.FieldId == row.FieldId &&
                item.ConfigFieldInstanceName == row.ConfigFieldInstanceName).QueryExecuteAsync()).Single();
            return result;
        }

        protected override void Query(QueryArgs args, QueryResult result)
        {
            var rowSelected = GridConfigGridRowSelected;
            result.Query = args.Query.Where(item => item.ConfigGridTableId == rowSelected.TableId && item.ConfigGridConfigName == rowSelected.ConfigName);
            if (FieldNameCSharp != null)
            {
                result.Query = result.Query.Where(item => item.FieldFieldNameCSharp == FieldNameCSharp);
            }
            result.Query = result.Query.OrderBy(item => item.FieldFieldSort);
        }

        protected override async Task UpdateAsync(UpdateArgs args, UpdateResult result)
        {
            // ConfigGrid
            if (args.Row.ConfigGridId == null)
            {
                var rowDisplayReload = await Reload(args.RowOld); // ConfigGrid row might have been added in the meantime, if multiple ConfigField rows are in the grid.
                args.Row.ConfigGridId = rowDisplayReload.ConfigGridId;
                if (args.Row.ConfigGridId == null)
                {
                    var rowDest = new FrameworkConfigGrid();
                    Data.RowCopy(args.Row, rowDest, "ConfigGrid");
                    rowDest.IsExist = true;
                    await Data.InsertAsync(rowDest);
                    args.Row.ConfigGridId = rowDest.Id;
                }
            }

            // ConfigField
            if (args.Row.ConfigFieldId == null)
            {
                var rowDest = new FrameworkConfigField();
                Data.RowCopy(args.Row, rowDest, "ConfigField");
                rowDest.ConfigGridId = args.Row.ConfigGridId.Value;
                rowDest.FieldId = args.Row.FieldId;
                rowDest.IsExist = true;
                await Data.InsertAsync(rowDest);
                args.Row.ConfigFieldId = rowDest.Id;
            }
            else
            {
                var rowDest = new FrameworkConfigField();
                Data.RowCopy(args.Row, rowDest, "ConfigField");
                await Data.UpdateAsync(rowDest);
            }

            // Reload
            {
                var rowDisplayReload = await Reload(args.Row);
                Data.RowCopy(rowDisplayReload, args.Row);
            }

            result.IsHandled = true;
        }

        protected override async Task InsertAsync(InsertArgs args, InsertResult result)
        {
            args.Row.ConfigGridTableId = GridConfigGridRowSelected.TableId; // Master
            args.Row.ConfigGridConfigName = GridConfigGridRowSelected.ConfigName; // Master

            var rowDest = new FrameworkConfigField();
            Data.RowCopy(args.Row, rowDest, "ConfigField");
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
            string fieldNameCSharp = ((FrameworkConfigFieldDisplay)args.Row).FieldFieldNameCSharp; // Text entered by user.
            var fieldList = await Data.Query<FrameworkField>().Where(item => item.TableId == GridConfigGridRowSelected.TableId && item.FieldNameCSharp == fieldNameCSharp).QueryExecuteAsync();
            if (fieldList.Count == 0)
            {
                throw new Exception("Field not found!");
            }
            int fieldId = fieldList.Single().Id;
            rowDest.FieldId = fieldId;
            args.Row.FieldId = fieldId;
            rowDest.IsExist = true;
            await Data.InsertAsync(rowDest);
            var rowReload = await Reload(args.Row);
            Data.RowCopy(rowReload, args.Row);

            result.IsHandled = true;
        }
    }
}
