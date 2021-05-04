namespace Framework.Json
{
    using Database.dbo;
    using DatabaseIntegrate.dbo;
    using Framework.DataAccessLayer;
    using Framework.Session;
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    /// <summary>
    /// Page to configure data grid and columns.
    /// </summary>
    internal class PageConfigGrid : PageModal
    {
        public PageConfigGrid(ComponentJson owner, string tableNameCSharp, string fieldNameCSharp, string configName)
            : base(owner)
        {
            TableNameCSharp = tableNameCSharp;
            FieldNameCSharp = fieldNameCSharp;
            ConfigName = configName;
            ConfigNameDeveloper = FrameworkConfigGridIntegrateFramework.IdEnum.dboFrameworkConfigFieldDisplayDeveloper.Row().ConfigName;
        }

        /// <summary>
        /// Gets TableNameCSharp. This is the table name for which this page is for.
        /// </summary>
        public string TableNameCSharp { get; }

        /// <summary>
        /// Gets FieldNameCSharp. This is the field name for which this page is for. If null, all fields are shown.
        /// </summary>
        public string FieldNameCSharp { get; }

        /// <summary>
        /// Gets ConfigName. This is the data grid ConfigName for GridConfigGrid and GridConfigField.
        /// </summary>
        public string ConfigName { get; }

        /// <summary>
        /// Gets ConfigNameDeveloper. This is strongly typed Developer.
        /// </summary>
        public string ConfigNameDeveloper { get; }

        public override async Task InitAsync()
        {
            new Html(DivHeader) { TextHtml = "<h1>Config</h1>" };
            new Html(DivBody) { TextHtml = "<h2>Config Grid</h2>" };
            GridConfigGrid = new GridConfigGrid(DivBody);
            new Html(DivBody) { TextHtml = "<h2>Config Field</h2>" };
            GridConfigField = new GridConfigField(DivBody);

            await GridConfigGrid.LoadAsync();
            
            if (GridConfigGrid.RowList.Count == 0)
            {
                throw new Exception(string.Format("Grid has no entry in table FrameworkTable! Run cli command deployDb to register for config. ({0})", TableNameCSharp));
            }
        }

        public GridConfigGrid GridConfigGrid;

        /// <summary>
        /// Gets GridConfigGridSelect. Master row.
        /// </summary>
        public FrameworkConfigGridDisplay GridConfigGridRowSelect => (FrameworkConfigGridDisplay)GridConfigGrid.RowSelect;

        public GridConfigField GridConfigField;

        /// <summary>
        /// User clicked close button. Reload grids with new config.
        /// </summary>
        protected internal override async Task ProcessAsync()
        {
            if (ButtonClose.IsClick)
            {
                foreach (var grid in this.ComponentOwner<AppJson>().ComponentListAll<Grid>())
                {
                    if (grid.TypeRow != null) // Grid has been loaded.
                    {
                        string tableNameCSharp = UtilDalType.TypeRowToTableNameCSharp(grid.TypeRow);
                        if (TableNameCSharp == tableNameCSharp)
                        {
                            await UtilGrid.LoadConfigOnlyAsync(grid);
                        }
                    }
                }
            }

            await base.ProcessAsync();
        }
    }

    internal class GridConfigGrid : Grid<FrameworkConfigGridDisplay>
    {
        public GridConfigGrid(ComponentJson owner) : base(owner) { }

        public string TableNameCSharp
        {
            get
            {
                return this.ComponentOwner<PageConfigGrid>().TableNameCSharp;
            }
        }

        private static async Task<FrameworkConfigGridDisplay> Reload(FrameworkConfigGridDisplay row)
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
                result.Query = Data.Query<FrameworkConfigGridDisplay>().Where(item => item.TableNameCSharp == TableNameCSharp);
            }
        }

        protected override void QueryConfig(QueryConfigArgs args, QueryConfigResult result)
        {
            result.ConfigName = this.ComponentOwner<PageConfigGrid>().ConfigName;
        }

        /// <summary>
        /// Load detail grid (config field).
        /// </summary>
        protected internal override async Task RowSelectAsync()
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
                    rowDest.IsDelete = false;
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
                rowDest.IsDelete = false;
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
            rowDest.IsDelete = false;
            await Data.InsertAsync(rowDest);
            Data.RowCopy(rowDest, result.Row);
            var rowReload = await Reload(result.Row);
            Data.RowCopy(rowReload, result.Row);

            result.IsHandled = true;
        }
    }

    internal class GridConfigField : Grid<FrameworkConfigFieldDisplay>
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
        /// Gets GridConfigGridSelect. Master row.
        /// </summary>
        public FrameworkConfigGridDisplay GridConfigGridRowSelect
        {
            get
            {
                return this.ComponentOwner<PageConfigGrid>().GridConfigGrid.RowSelect;
            }
        }

        private static async Task<FrameworkConfigFieldDisplay> Reload(FrameworkConfigFieldDisplay row)
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
            var rowSelect = GridConfigGridRowSelect;
            result.Query = Data.Query<FrameworkConfigFieldDisplay>().Where(item => item.ConfigGridTableId == rowSelect.TableId && item.ConfigGridConfigName == rowSelect.ConfigName);
            if (FieldNameCSharp != null)
            {
                result.Query = result.Query.Where(item => item.FieldFieldNameCSharp == FieldNameCSharp);
            }
            result.Query = result.Query.OrderBy(item => item.FieldFieldSort);
        }

        protected override void QueryConfig(QueryConfigArgs args, QueryConfigResult result)
        {
            result.ConfigName = this.ComponentOwner<PageConfigGrid>().ConfigName;
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
                    rowDest.IsDelete = false;
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
                rowDest.IsDelete = false;
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

        protected override void CellAnnotation(AnnotationArgs args, AnnotationResult result)
        {
            var pageConfigGrid = this.ComponentOwner<PageConfigGrid>();
            if (pageConfigGrid.ConfigName != pageConfigGrid.ConfigNameDeveloper)
            {
                // User needs flag SettingResult.GridIsShowConfigDeveloper and (coffee icon) button clicked to modify developer config.
                result.IsReadOnly = args.Row.ConfigGridConfigName == pageConfigGrid.ConfigNameDeveloper;
            }
        }

        protected override async Task InsertAsync(InsertArgs args, InsertResult result)
        {
            args.Row.ConfigGridTableId = GridConfigGridRowSelect.TableId; // Master
            args.Row.ConfigGridConfigName = GridConfigGridRowSelect.ConfigName; // Master

            var rowDest = new FrameworkConfigField();
            Data.RowCopy(args.Row, rowDest, "ConfigField");
            if (GridConfigGridRowSelect.Id == null) // Master does not have FrameworkConfigGrid in database
            {
                var rowDestConfigGrid = new FrameworkConfigGrid();
                Data.RowCopy(GridConfigGridRowSelect, rowDestConfigGrid);
                rowDestConfigGrid.IsDelete = false;
                await Data.InsertAsync(rowDestConfigGrid);
                GridConfigGridRowSelect.Id = rowDestConfigGrid.Id;
            }
            rowDest.ConfigGridId = GridConfigGridRowSelect.Id.Value; // Master

            // Lookup field
            string fieldNameCSharp = ((FrameworkConfigFieldDisplay)args.Row).FieldFieldNameCSharp; // Text entered by user.
            var fieldList = await Data.Query<FrameworkField>().Where(item => item.TableId == GridConfigGridRowSelect.TableId && item.FieldNameCSharp == fieldNameCSharp).QueryExecuteAsync();
            if (fieldList.Count == 0)
            {
                throw new Exception("Field not found!");
            }
            int fieldId = fieldList.Single().Id;
            rowDest.FieldId = fieldId;
            args.Row.FieldId = fieldId;
            rowDest.IsDelete = false;
            await Data.InsertAsync(rowDest);
            var rowReload = await Reload(args.Row);
            Data.RowCopy(rowReload, args.Row);

            result.IsHandled = true;
        }
    }
}
