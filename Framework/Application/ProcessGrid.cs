﻿namespace Framework.Application
{
    using Framework.Application.Config;
    using Framework.Component;
    using Framework.DataAccessLayer;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Dynamic.Core;

    /// <summary>
    /// Process OrderBy click.
    /// </summary>
    internal class ProcessGridOrderBy : Process
    {
        protected internal override void Run(App app)
        {
            AppJson appJson = app.AppJson;
            // Detect OrderBy click
            foreach (GridName gridName in app.GridData.GridNameList())
            {
                foreach (GridColumnInternal gridColumn in app.GridData.ColumnInternalList(gridName))
                {
                    if (gridColumn.IsClick)
                    {
                        GridQueryInternal gridQuery = app.GridData.QueryInternalGet(gridName);
                        if (gridQuery.ColumnNameOrderBy == gridColumn.ColumnName)
                        {
                            gridQuery.IsOrderByDesc = !gridQuery.IsOrderByDesc;
                        }
                        else
                        {
                            gridQuery.ColumnNameOrderBy = gridColumn.ColumnName;
                            gridQuery.IsOrderByDesc = false;
                        }
                        app.GridData.TextParse(isFilterParse: true);
                        app.GridData.LoadDatabaseReload(gridName);
                        break;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Runs after ProcessGridSaveJson. Set OrderBy up or down arrow.
    /// </summary>
    internal class ProcessGridOrderByText : Process
    {
        protected internal override void Run(App app)
        {
            AppJson appJson = app.AppJson;
            //
            foreach (string gridName in appJson.GridDataJson.ColumnList.Keys)
            {
                GridQuery gridQuery = appJson.GridDataJson.GridQueryList[gridName];
                foreach (GridColumn gridColumn in appJson.GridDataJson.ColumnList[gridName])
                {
                    gridColumn.IsClick = false;
                    if (gridColumn.ColumnName == gridQuery.ColumnNameOrderBy)
                    {
                        if (gridQuery.IsOrderByDesc)
                        {
                            gridColumn.Text = "▼" + gridColumn.Text;
                        }
                        else
                        {
                            gridColumn.Text = "▲" + gridColumn.Text;
                        }
                    }
                }
            }
        }
    }

    internal class ProcessGridTextParse : Process
    {
        protected internal override void Run(App app)
        {
            app.GridData.TextParse();
        }
    }

    /// <summary>
    /// Load next or previous page.
    /// </summary>
    internal class ProcessGridPageIndex : Process
    {
        protected internal override void Run(App app)
        {
            AppJson appJson = app.AppJson;
            var gridQueryList = appJson.GridDataJson.GridQueryList;
            var gridData = app.GridData;
            //
            foreach (string gridName in gridQueryList.Keys)
            {
                // PageIndex
                if (gridQueryList[gridName].IsPageIndexNext)
                {
                    gridData.QueryInternalGet(GridName.FromJson(gridName)).PageIndex += 1;
                    gridData.LoadDatabaseReload(GridName.FromJson(gridName));
                }
                if (gridQueryList[gridName].IsPageIndexPrevious)
                {
                    gridData.QueryInternalGet(GridName.FromJson(gridName)).PageIndex -= 1;
                    if (gridData.QueryInternalGet(GridName.FromJson(gridName)).PageIndex < 0)
                    {
                        gridData.QueryInternalGet(GridName.FromJson(gridName)).PageIndex = 0;
                    }
                    gridData.LoadDatabaseReload(GridName.FromJson(gridName));
                }
                // PageIndex Horizontal
                if (gridQueryList[gridName].IsPageHorizontalIndexNext)
                {
                    gridData.QueryInternalGet(GridName.FromJson(gridName)).PageHorizontalIndex += 1;
                    gridData.LoadDatabaseReload(GridName.FromJson(gridName));
                }
                if (gridQueryList[gridName].IsPageIndexPrevious)
                {
                    gridData.QueryInternalGet(GridName.FromJson(gridName)).PageHorizontalIndex -= 1;
                    if (gridData.QueryInternalGet(GridName.FromJson(gridName)).PageHorizontalIndex < 0)
                    {
                        gridData.QueryInternalGet(GridName.FromJson(gridName)).PageHorizontalIndex = 0;
                    }
                    gridData.LoadDatabaseReload(GridName.FromJson(gridName));
                }
            }
        }
    }

    /// <summary>
    /// Process data grid filter.
    /// </summary>
    internal class ProcessGridFilter : Process
    {
        protected internal override void Run(App app)
        {
            AppJson appJson = app.AppJson;
            //
            List<string> gridNameList = new List<string>(); // Grids to reload after filter changed.
            app.GridData.CellInternalAll((GridCellInternal gridCell, AppEventArg e) =>
            {
                if (e.Index.Enum == IndexEnum.Filter && gridCell.IsModify)
                {
                    if (!gridNameList.Contains(e.GridName.Name))
                    {
                        gridNameList.Add(e.GridName.Name);
                    }
                }
            });
            //
            foreach (string gridName in gridNameList) // Grids with filter changed
            {
                GridData gridData = app.GridData;
                gridData.LoadDatabaseReload(GridName.FromJson(gridName));
            }
        }
    }

    /// <summary>
    /// Load grid from database where not yet loaded. Prevents load of GridConfig multiple times!
    /// </summary>
    internal class ProcessGridLoadDatabase : Process
    {
        protected internal override void Run(App app)
        {
            List<GridNameType> gridNameTypeList = new List<GridNameType>(); // Grids to load.
            foreach (Grid grid in app.AppJson.ListAll().OfType<Grid>())
            {
                GridNameType gridNameType = grid.GridNameInternal as GridNameType;
                if (gridNameType != null && !gridNameTypeList.Contains(gridNameType))
                {
                    gridNameTypeList.Add(gridNameType);
                }
            }
            //
            foreach (GridNameType gridNameType in gridNameTypeList)
            {
                if (app.GridData.TypeRow(gridNameType) == null) // Not yet loaded.
                {
                    app.GridData.QueryInternalCreate(gridNameType);
                    app.GridData.LoadDatabase(gridNameType); // Keeps method LoadDatabase(); internal and prevents load of GridConfig multiple times.
                }
            }
        }
    }

    /// <summary>
    /// Runs before ProcessGridLoadJson. ProcessGridLoadJson Grid row or cell is clicked. Set IsSelect.
    /// </summary>
    internal class ProcessGridIsClick : Process
    {
        private void ProcessGridSelectRowClear(AppJson appJson, string gridName)
        {
            foreach (GridRow gridRow in appJson.GridDataJson.RowList[gridName])
            {
                gridRow.IsSelectSet(false);
            }
        }

        private void ProcessGridSelectCell(AppJson appJson, string gridName, Index index, string columnName)
        {
            GridDataJson gridDataJson = appJson.GridDataJson;
            //
            gridDataJson.SelectGridNamePrevious = gridDataJson.SelectGridName;
            gridDataJson.SelectIndexPrevious = gridDataJson.SelectIndex;
            gridDataJson.SelectColumnNamePrevious = gridDataJson.SelectColumnName;
            //
            gridDataJson.SelectGridName = gridName;
            gridDataJson.SelectIndex = index.Value;
            gridDataJson.SelectColumnName = columnName;
        }

        protected internal override void Run(App app)
        {
            GridDataJson gridDataJson = app.AppJson.GridDataJson;
            foreach (GridQuery gridQuery in gridDataJson.GridQueryList.Values)
            {
                string gridName = gridQuery.GridName;
                foreach (GridRow gridRow in gridDataJson.RowList[gridName])
                {
                    bool cellIsClick = false;
                    foreach (var gridColumn in gridDataJson.ColumnList[gridName])
                    {
                        GridCell gridCell = gridDataJson.CellList[gridName][gridColumn.ColumnName][gridRow.Index];
                        if (gridCell.IsClick == true)
                        {
                            cellIsClick = true;
                            ProcessGridSelectCell(app.AppJson, gridName, new Index(gridRow.Index), gridColumn.ColumnName);
                        }
                    }
                    if (gridRow.IsClick || cellIsClick)
                    {
                        gridRow.IsClick = true; // If cell is clicked, row is also clicked.
                        ProcessGridSelectRowClear(app.AppJson, gridName);
                        gridRow.IsSelectSet(true);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Postpone method call GridData.RowNewAdd(); till Config is loaded.
    /// </summary>
    internal class ProcessGridIsInsert : Process
    {
        protected internal override void Run(App app)
        {
            foreach (GridName gridName in app.GridData.GridNameList())
            {
                if (app.GridData.QueryInternalGet(gridName).IsInsert)
                {
                    GridNameType gridNameType = app.GridData.GridNameType(gridName);
                    if (app.GridData.Config.ConfigGridGet(gridNameType).IsInsert)
                    {
                        app.GridData.RowNewAdd(gridName);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Select first row of grid, if no row is yet selected.
    /// </summary>
    internal class ProcessGridRowSelectFirst : Process
    {
        protected internal override void Run(App app)
        {
            GridData gridData = app.GridData;
            foreach (GridName gridName in gridData.GridNameList())
            {
                Index index = gridData.RowSelectedIndex(gridName);
                if (index == null)
                {
                    Index indexFirst = gridData.RowIndexList(gridName).Where(item => item.Enum == IndexEnum.Index).FirstOrDefault();
                    if (indexFirst == null)
                    {
                        indexFirst = gridData.RowIndexList(gridName).Where(item => item.Enum == IndexEnum.New).FirstOrDefault();
                    }
                    Row rowSelect = gridData.RowSelect(gridName, indexFirst);
                    ProcessGridIsClickMasterDetail.MasterDetailIsClick(app, gridName, rowSelect);
                }
            }
        }
    }

    internal class ProcessNavigationButtonIsClickFirst : Process
    {
        protected internal override void Run(App app)
        {
            foreach (Navigation navigation in app.AppJson.ListAll().OfType<Navigation>())
            {
                navigation.ProcessButtonIsClickFirst(app);
            }
        }
    }

    internal class ProcessGridIsClickMasterDetail : Process
    {
        internal static void MasterDetailIsClick(App app, GridName gridNameMaster, Row rowMaster)
        {
            GridData gridData = app.GridData;
            var list = gridData.GridNameList();
            foreach (GridName gridNameDetail in list)
            {
                if (gridNameMaster != gridNameDetail)
                {
                    if (GridName.FromJson(app.AppJson.GridDataJson.SelectGridNamePrevious) == gridNameMaster && app.GridData.RowSelectedIndex(gridNameMaster) == new Index(app.AppJson.GridDataJson.SelectIndexPrevious))
                    {
                        // Selected row did not change. User clicked a cell in the same row. No detail reload.
                    }
                    else
                    {
                        Type typeRow = gridData.TypeRow(gridNameDetail);
                        Row rowTable = UtilDataAccessLayer.RowCreate(typeRow); // RowTable is the API. No data in record!
                        bool isReload = false;
                        rowTable.MasterIsClick(gridNameMaster, rowMaster, ref isReload, new AppEventArg(app, gridNameDetail, null, null));
                        if (isReload)
                        {
                            gridData.LoadDatabaseReload(gridNameDetail);
                        }
                    }
                }
            }
        }

        protected internal override void Run(App app)
        {
            foreach (GridName gridName in app.GridData.GridNameList())
            {
                foreach (var indexRow in app.GridData.RowInternalList(gridName))
                {
                    Index index = indexRow.Key;
                    GridRowInternal rowInternal = indexRow.Value;
                    if (rowInternal.IsClick)
                    {
                        if (index.Enum == IndexEnum.Index || index.Enum == IndexEnum.New)
                        {
                            Row row = app.GridData.RowGet(gridName, index);
                            MasterDetailIsClick(app, gridName, row);
                            break;
                        }
                    }
                }
            }
        }
     }

    /// <summary>
    /// Save GridData back to json.
    /// </summary>
    internal class ProcessGridSaveJson : Process
    {
        protected internal override void Run(App app)
        {
            app.GridData.SaveJson();
        }
    }

    /// <summary>
    /// Runs after ProcessGridSaveJson. Set row and cell IsClick to false
    /// </summary>
    internal class ProcessGridIsClickFalse : Process
    {
        protected internal override void Run(App app)
        {
            GridDataJson gridDataJson = app.AppJson.GridDataJson;
            foreach (GridQuery gridQuery in gridDataJson.GridQueryList.Values)
            {
                string gridName = gridQuery.GridName;
                foreach (GridRow gridRow in gridDataJson.RowList[gridName])
                {
                    gridRow.IsClick = false;
                    foreach (var gridColumn in gridDataJson.ColumnList[gridName])
                    {
                        GridCell gridCell = gridDataJson.CellList[gridName][gridColumn.ColumnName][gridRow.Index];
                        gridCell.IsClick = false;
                    }
                }
            }
            //
            gridDataJson.SelectGridNamePrevious = null;
            gridDataJson.SelectIndexPrevious = null;
            gridDataJson.SelectColumnNamePrevious = null;
        }
    }

    /// <summary>
    /// Runs after ProcessGridSaveJson.
    /// </summary>
    internal class ProcessGridCellIsModifyFalse : Process
    {
        protected internal override void Run(App app)
        {
            GridDataJson gridDataJson = app.AppJson.GridDataJson;
            //
            foreach (string gridName in gridDataJson.RowList.Keys)
            {
                foreach (GridRow gridRow in gridDataJson.RowList[gridName])
                {
                    foreach (var gridColumn in gridDataJson.ColumnList[gridName])
                    {
                        GridCell gridCell = gridDataJson.CellList[gridName][gridColumn.ColumnName][gridRow.Index];
                        if (gridCell.IsModify)
                        {
                            gridCell.IsModify = false;
                            gridCell.TOld = null;
                        }
                    }
                }
            }
        }
    }

    internal class ProcessGridLookupRowIsClick : Process
    {
        protected internal override void Run(App app)
        {
            GridDataJson gridDataJson = app.AppJson.GridDataJson;
            GridName gridNameLookup = null;
            app.GridData.CellInternalAll((GridCellInternal gridCell, AppEventArg e) => 
            {
                if (gridCell.IsLookup)
                {
                    gridNameLookup = GridName.FromJson(gridCell.GridNameLookup);
                }
            });
            //
            Row rowLookup = null; // Clicked row
            if (gridNameLookup != null)
            {
                foreach (KeyValuePair<Index, GridRowInternal> item in app.GridData.RowInternalList(gridNameLookup))
                {
                    if (item.Value.IsClick)
                    {
                        if (item.Key.Enum == IndexEnum.Index)
                        {
                            GridData gridData = app.GridData;
                            rowLookup = gridData.RowGet(gridNameLookup, item.Key);
                        }
                    }
                }
            }
            //
            if (rowLookup != null)
            {
                GridData gridData = app.GridData;
                // Row and cell, on which lookup is open.
                GridName gridName = GridName.FromJson(gridDataJson.SelectGridNamePrevious);
                Index index = new Index(gridDataJson.SelectIndexPrevious);
                string columnName = gridDataJson.SelectColumnNamePrevious;
                // Lookup
                UtilFramework.Assert(gridNameLookup == GridName.FromJson(gridDataJson.SelectGridName));
                Index indexLookup = new Index(gridDataJson.SelectIndex);
                string columnNameLookup = gridDataJson.SelectColumnName;
                // Set IsModify
                gridData.CellIsModifySet(gridName, index, columnName); // Put row into edit mode.
                var row = gridData.RowGet(gridName, index);
                Cell cell = UtilDataAccessLayer.CellList(row.GetType(), row).Where(item => item.ColumnNameCSharp == columnName).First();
                // Cell of lookup which user clicked.
                Cell cellLookup = UtilDataAccessLayer.CellList(rowLookup.GetType(), rowLookup).Where(item => item.ColumnNameCSharp == columnNameLookup).First();
                string text = app.GridData.CellInternalGet(gridNameLookup, indexLookup, columnNameLookup).Text;
                app.GridData.ErrorCellSet(gridName, index, columnName, null); // Clear error
                cell.LookupIsClick(rowLookup, new AppEventArg(app, gridName, index, cell.ColumnNameCSharp)); // (cellLookup.ColumnNameCSharp, text);
                //
                app.GridData.SelectGridName = GridName.ToJson(gridName);
                app.GridData.SelectIndex = index;
                app.GridData.SelectColumnName = columnName;
                //
                app.GridData.LookupClose(app); // Close lookup window.
            }
        }
    }

    /// <summary>
    /// Open Lookup grid.
    /// </summary>
    internal class ProcessGridLookupOpen : Process
    {
        /// <summary>
        /// Returns true, if cell has been clicked or text has been entered.
        /// </summary>
        private bool IsLookupOpen(App app, out AppEventArg e)
        {
            bool result = false;
            AppEventArg eLocal = null;
            //
            app.GridData.CellInternalAll((GridCellInternal cell, AppEventArg eAll) => {
                if (cell.IsClick || cell.IsModify)
                {
                    if (cell.IsLookupClose == false)
                    {
                        result = true;
                        eLocal = eAll;
                        eAll.IsBreak = true;
                    }
                }
            });
            e = eLocal;
            return result;
        }

        protected internal override void Run(App app)
        {
            bool isLookupOpen = IsLookupOpen(app, out AppEventArg e);
            //
            GridData gridData = app.GridData;
            if (isLookupOpen)
            {
                Row row = gridData.RowGet(e.GridName, e.Index);
                GridCellInternal gridCellInternal = gridData.CellInternalGet(e.GridName, e.Index, e.ColumnName);
                //
                Type typeRow = gridData.TypeRow(e.GridName);
                Cell cell = UtilDataAccessLayer.CellList(typeRow, row).Where(item => item.ColumnNameCSharp == e.ColumnName).Single();
                if (e.Index.Enum != IndexEnum.Filter) // No Lookup for filter column for now. It would work though for example for distinct.
                {
                    cell.Lookup(out GridNameType gridName, out IQueryable query, e);
                    gridData.LookupClose(app);
                    if (gridName != null)
                    {
                        UtilFramework.Assert(gridName.TypeRow == query.ElementType);
                        gridData.QueryInternalCreate(gridName);
                        gridData.LoadDatabase(gridName, null, null, false, query);
                        gridData.LookupOpen(e.GridName, e.Index, e.ColumnName, gridName);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Runs after ProcessGridSaveJson.
    /// </summary>
    internal class ProcessGridFocus : Process
    {
        private void FocusClear(App app)
        {
            GridDataJson gridDataJson = app.AppJson.GridDataJson;
            foreach (string gridNameItem in gridDataJson.RowList.Keys)
            {
                foreach (GridRow gridRow in gridDataJson.RowList[gridNameItem])
                {
                    foreach (var gridColumn in gridDataJson.ColumnList[gridNameItem])
                    {
                        GridCell gridCell = gridDataJson.CellList[gridNameItem][gridColumn.ColumnName][gridRow.Index];
                        gridCell.FocusId = null;
                        gridCell.FocusIdRequest = null;
                    }
                }
            }
            //
            app.GridData.CellInternalAll((GridCellInternal gridCellInternal, AppEventArg e) => { gridCellInternal.FocusId = null; gridCellInternal.FocusIdRequest = null; });
        }

        protected internal override void Run(App app)
        {
            GridDataJson gridDataJson = app.AppJson.GridDataJson;
            foreach (string gridNameItem in gridDataJson.RowList.Keys)
            {
                foreach (GridRow gridRow in gridDataJson.RowList[gridNameItem])
                {
                    foreach (GridColumn gridColumn in gridDataJson.ColumnList[gridNameItem])
                    {
                        GridCell gridCell = gridDataJson.CellList[gridNameItem][gridColumn.ColumnName][gridRow.Index];
                        if (gridCell.FocusIdRequest != null && gridCell.IsSelect)
                        {
                            int? focusIdRequest = gridCell.FocusIdRequest;
                            FocusClear(app);
                            gridCell.FocusId = focusIdRequest;
                            app.GridData.CellInternalGet(GridName.FromJson(gridNameItem), new Index(gridRow.Index), gridColumn.ColumnName).FocusId = focusIdRequest; 
                            break;
                        }
                    }
                }
            }
        }
    }


    /// <summary>
    /// Runs after ProcessGridSaveJson. Set IsSelect on selected GridCell, or to null, if cell does not exist anymore.
    /// </summary>
    internal class ProcessGridCellIsSelect : Process
    {
        private void IsSelect(GridDataJson gridDataJson)
        {
            foreach (string gridName in gridDataJson.RowList.Keys)
            {
                foreach (GridRow gridRow in gridDataJson.RowList[gridName])
                {
                    foreach (var gridColumn in gridDataJson.ColumnList[gridName])
                    {
                        GridCell gridCell = gridDataJson.CellList[gridName][gridColumn.ColumnName][gridRow.Index];
                        bool isSelect = gridDataJson.SelectGridName == gridName && gridDataJson.SelectColumnName == gridColumn.ColumnName && gridDataJson.SelectIndex == gridRow.Index;
                        gridCell.IsSelect = isSelect;
                    }
                }
            }
        }

        protected internal override void Run(App app)
        {
            GridDataJson gridDataJson = app.AppJson.GridDataJson;
            bool isExist = false; // Selected cell exists
            if (gridDataJson.SelectColumnName != null)
            {
                if (gridDataJson.RowList[gridDataJson.SelectGridName].Exists(item => item.Index == gridDataJson.SelectIndex)) // Selected row exists
                {
                    if (gridDataJson.ColumnList[gridDataJson.SelectGridName].Exists(item => item.ColumnName == gridDataJson.SelectColumnName)) // Selected column exists
                    {
                        isExist = true;
                    }
                }
            }
            if (isExist == false)
            {
                app.AppJson.GridDataJson.SelectColumnName = null;
                app.AppJson.GridDataJson.SelectGridName = null;
                app.AppJson.GridDataJson.SelectIndex = null;
            }
            //
            IsSelect(gridDataJson);
        }
    }

    /// <summary>
    /// Runs after ProcessGridSaveJson.
    /// </summary>
    internal class ProcessGridFieldWithLabelIndex : Process
    {
        protected internal override void Run(App app)
        {
            foreach (GridFieldWithLabel gridFieldWithLabel in app.AppJson.ListAll().OfType<GridFieldWithLabel>())
            {
                gridFieldWithLabel.Index = app.GridData.RowSelectedIndex(GridName.FromJson(gridFieldWithLabel.GridName))?.Value; // Set index to selected row.
            }
        }
    }

    internal class ProcessGridSaveDatabase : Process
    {
        protected internal override void Run(App app)
        {
            app.GridData.SaveDatabase();
        }
    }

    /// <summary>
    /// Cell rendered as button is clicked.
    /// </summary>
    internal class ProcessGridCellButtonIsClick : Process
    {
        protected internal override void Run(App app)
        {
            GridDataJson gridDataJson = app.AppJson.GridDataJson;
            //
            string gridNameClick = null;
            string indexClick = null;
            string columnNameClick = null;
            foreach (string gridName in gridDataJson.RowList.Keys)
            {
                foreach (GridRow gridRow in gridDataJson.RowList[gridName])
                {
                    foreach (var gridColumn in gridDataJson.ColumnList[gridName])
                    {
                        GridCell gridCell = gridDataJson.CellList[gridName][gridColumn.ColumnName][gridRow.Index];
                        if (gridCell.IsModify && gridCell.CellEnum == GridCellEnum.Button)
                        {
                            gridNameClick = gridName;
                            indexClick = gridRow.Index;
                            columnNameClick = gridColumn.ColumnName;
                            break;
                        }
                    }
                }
            }
            //
            if (gridNameClick != null)
            {
                Row row = app.GridData.RowGet(GridName.FromJson(gridNameClick), new Index(indexClick));
                Type typeRow = app.GridData.TypeRow(GridName.FromJson(gridNameClick));
                Cell cell = UtilDataAccessLayer.CellList(typeRow, row).Where(item => item.ColumnNameCSharp == columnNameClick).Single();
                bool isReload = false;
                bool isException = false;
                try
                {
                    cell.ButtonIsClick(ref isReload, new AppEventArg(app, GridName.FromJson(gridNameClick), new Index(indexClick), columnNameClick));
                }
                catch (Exception exception)
                {
                    isException = true;
                    app.GridData.ErrorRowSet(GridName.FromJson(gridNameClick), new Index(indexClick), UtilFramework.ExceptionToText(exception));
                }
                if (isReload && isException == false)
                {
                    app.GridData.LoadDatabaseReload(GridName.FromJson(gridNameClick));
                }
            }
        }
    }
}