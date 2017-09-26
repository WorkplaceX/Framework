namespace Framework.Application
{
    using Framework.Component;
    using Framework.DataAccessLayer;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Process OrderBy click.
    /// </summary>
    internal class ProcessGridOrderBy : Process
    {
        protected internal override void Run(App app)
        {
            AppJson appJson = app.AppJson;
            // Detect OrderBy click
            foreach (string gridName in appJson.GridDataJson.ColumnList.Keys.ToArray())
            {
                foreach (GridColumn gridColumn in appJson.GridDataJson.ColumnList[gridName])
                {
                    if (gridColumn.IsClick)
                    {
                        GridQuery gridQuery = appJson.GridDataJson.GridQueryList[gridName];
                        if (gridQuery.FieldNameOrderBy == gridColumn.FieldName)
                        {
                            gridQuery.IsOrderByDesc = !gridQuery.IsOrderByDesc;
                        }
                        else
                        {
                            gridQuery.FieldNameOrderBy = gridColumn.FieldName;
                            gridQuery.IsOrderByDesc = false;
                        }
                        app.GridData.TextParse(isFilterParse: true);
                        app.GridData.LoadDatabaseReload(new GridName(gridName, true));
                        break;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Set OrderBy up or down arrow.
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
                    if (gridColumn.FieldName == gridQuery.FieldNameOrderBy)
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
            foreach (string gridName in appJson.GridDataJson.ColumnList.Keys)
            {
                foreach (GridRow gridRow in appJson.GridDataJson.RowList[gridName])
                {
                    if (new Index(gridRow.Index).Enum == IndexEnum.Filter)
                    {
                        foreach (GridColumn gridColumn in appJson.GridDataJson.ColumnList[gridName])
                        {
                            GridCell gridCell = appJson.GridDataJson.CellList[gridName][gridColumn.FieldName][gridRow.Index];
                            if (gridCell.IsModify)
                            {
                                if (!gridNameList.Contains(gridName))
                                {
                                    gridNameList.Add(gridName);
                                }
                            }
                        }
                    }
                }
            }
            //
            foreach (string gridName in gridNameList)
            {
                app.GridDataTextParse();
                GridData gridData = app.GridData;
                gridData.LoadDatabaseReload(new GridName(gridName, true));
                gridData.SaveJson();
            }
        }
    }

    /// <summary>
    /// Grid row or cell is clicked. Set focus.
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

        private void ProcessGridSelectCell(AppJson appJson, string gridName, Index index, string fieldName)
        {
            GridDataJson gridDataJson = appJson.GridDataJson;
            //
            gridDataJson.FocusGridNamePrevious = gridDataJson.FocusGridName;
            gridDataJson.FocusIndexPrevious = gridDataJson.FocusIndex;
            gridDataJson.FocusFieldNamePrevious = gridDataJson.FocusFieldName;
            //
            gridDataJson.FocusGridName = gridName;
            gridDataJson.FocusIndex = index.Value;
            gridDataJson.FocusFieldName = fieldName;
        }

        protected internal override void Run(App app)
        {
            GridDataJson gridDataJson = app.AppJson.GridDataJson;
            foreach (GridQuery gridQuery in gridDataJson.GridQueryList.Values)
            {
                string gridName = gridQuery.GridName;
                foreach (GridRow gridRow in gridDataJson.RowList[gridName])
                {
                    if (gridRow.IsClick)
                    {
                        ProcessGridSelectRowClear(app.AppJson, gridName);
                        gridRow.IsSelectSet(true);
                    }
                    foreach (var gridColumn in gridDataJson.ColumnList[gridName])
                    {
                        GridCell gridCell = gridDataJson.CellList[gridName][gridColumn.FieldName][gridRow.Index];
                        if (gridCell.IsClick == true)
                        {
                            ProcessGridSelectCell(app.AppJson, gridName, new Index(gridRow.Index), gridColumn.FieldName);
                        }
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
                    Index indexFirst = gridData.IndexList(gridName).Where(item => item.Enum == IndexEnum.Index).FirstOrDefault();
                    if (indexFirst == null)
                    {
                        indexFirst = gridData.IndexList(gridName).Where(item => item.Enum == IndexEnum.New).FirstOrDefault();
                    }
                    Row rowSelect = gridData.RowSelect(gridName, indexFirst);
                    ProcessGridIsClickMasterDetail.MasterDetailIsClick(app, gridName, rowSelect);
                }
            }
        }
    }

    internal class ProcessGridIsClickMasterDetail : Process
    {
        internal static void MasterDetailIsClick(App app, GridName gridNameMaster, Row rowMaster)
        {
            GridData gridData = app.GridData;
            foreach (GridName gridName in gridData.GridNameList())
            {
                Type typeRow = gridData.TypeRow(gridName);
                Row rowTable = UtilDataAccessLayer.RowCreate(typeRow); // RowTable is the API. No data in record!
                bool isReload = false;
                rowTable.MasterIsClick(app, gridNameMaster, rowMaster, ref isReload);
                if (isReload)
                {
                    gridData.LoadDatabaseReload(gridName);
                }
            }
        }

        protected internal override void Run(App app)
        {
            GridDataJson gridDataJson = app.AppJson.GridDataJson;
            foreach (GridQuery gridQuery in gridDataJson.GridQueryList.Values)
            {
                string gridName = gridQuery.GridName;
                foreach (GridRow gridRow in gridDataJson.RowList[gridName])
                {
                    if (gridRow.IsClick)
                    {
                        Index gridRowIndex = new Index(gridRow.Index);
                        if (gridRowIndex.Enum == IndexEnum.Index || gridRowIndex.Enum == IndexEnum.New)
                        {
                            GridData gridData = app.GridData;
                            var row = gridData.Row(new GridName(gridName, true), gridRowIndex);
                            MasterDetailIsClick(app, new GridName(gridName, true), row);
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
    /// Set row and cell IsClick to false
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
                        GridCell gridCell = gridDataJson.CellList[gridName][gridColumn.FieldName][gridRow.Index];
                        gridCell.IsClick = false;
                    }
                }
            }
            //
            gridDataJson.FocusGridNamePrevious = null;
            gridDataJson.FocusIndexPrevious = null;
            gridDataJson.FocusFieldNamePrevious = null;
        }
    }

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
                        GridCell gridCell = gridDataJson.CellList[gridName][gridColumn.FieldName][gridRow.Index];
                        if (gridCell.IsModify)
                        {
                            gridCell.IsModify = false;
                        }
                    }
                }
            }
        }
    }

    internal class ProcessGridLookupIsClick : Process
    {
        protected internal override void Run(App app)
        {
            Row rowLookup = null;
            GridDataJson gridDataJson = app.AppJson.GridDataJson;
            foreach (string gridName in gridDataJson.RowList.Keys)
            {
                if (gridName == "Lookup")
                {
                    foreach (GridRow gridRow in gridDataJson.RowList[gridName])
                    {
                        if (gridRow.IsClick)
                        {
                            Index gridRowIndex = new Index(gridRow.Index);
                            if (gridRowIndex.Enum == IndexEnum.Index)
                            {
                                GridData gridData = app.GridData;
                                rowLookup = gridData.Row(new GridName("Lookup"), gridRowIndex);
                            }
                        }
                    }
                }
            }
            //
            if (rowLookup != null)
            {
                GridData gridData = app.GridData;
                var row = gridData.Row(new GridName(gridDataJson.FocusGridName, true), new Index(gridDataJson.FocusIndex));
                Cell cell = UtilDataAccessLayer.CellList(row.GetType(), row).Where(item => item.FieldNameCSharp == gridDataJson.FocusFieldNamePrevious).First();
                Cell cellLookup = UtilDataAccessLayer.CellList(rowLookup.GetType(), rowLookup).Where(item => item.FieldNameCSharp == gridDataJson.FocusFieldName).First();
                string result = cellLookup.Value.ToString();
                cell.CellLookupIsClick(rowLookup, ref result);
                GridCell gridCell = gridDataJson.CellList[gridDataJson.FocusGridNamePrevious][gridDataJson.FocusFieldNamePrevious][gridDataJson.FocusIndexPrevious];
                gridCell.IsModify = true;
                if (gridCell.IsO == false)
                {
                    gridCell.IsO = true;
                    gridCell.O = gridCell.T;
                }
                gridCell.T = result;
                gridData.LoadJson();
            }
        }
    }

    /// <summary>
    /// Open Lookup grid.
    /// </summary>
    internal class ProcessGridLookup : Process
    {
        /// <summary>
        /// Returns true, if cell has been clicked or text has been entered.
        /// </summary>
        private bool IsLookupOpen(App app, out GridName gridName, out Index index, out string fieldName)
        {
            bool result = false;
            gridName = null;
            index = null;
            fieldName = null;
            //
            GridDataJson gridDataJson = app.AppJson.GridDataJson;
            foreach (string gridNameItem in gridDataJson.RowList.Keys)
            {
                foreach (GridRow gridRow in gridDataJson.RowList[gridNameItem])
                {
                    foreach (var gridColumn in gridDataJson.ColumnList[gridNameItem])
                    {
                        GridCell gridCell = gridDataJson.CellList[gridNameItem][gridColumn.FieldName][gridRow.Index];
                        if (gridCell.IsClick || gridCell.IsModify)
                        {
                            result = true;
                            gridName = new GridName(gridNameItem, true);
                            index = new Index(gridRow.Index);
                            fieldName = gridColumn.FieldName;
                            break;
                        }
                    }
                }
            }
            return result;
        }

        protected internal override void Run(App app)
        {
            GridName gridName;
            Index index;
            string fieldName;
            bool isLookupOpen = IsLookupOpen(app, out gridName, out index, out fieldName);
            //
            GridData gridData = app.GridData;
            if (isLookupOpen)
            {
                Row row = gridData.Row(gridName, index);
                GridCellInternal gridCellInternal = gridData.CellGet(gridName, index, fieldName);
                //
                Type typeRow = gridData.TypeRow(gridName);
                Cell cell = UtilDataAccessLayer.CellList(typeRow, row).Where(item => item.FieldNameCSharp == fieldName).First();
                List<Row> rowList;
                cell.CellLookup(out typeRow, out rowList);
                new GridNameTypeRow(null);
                bool isLoadRow = gridData.LoadRow(new GridNameTypeRow(typeRow, UtilApplication.GridNameLookup), rowList);
                //
                if (isLoadRow)
                {
                    gridData.LookupOpen(gridName, index, fieldName);
                }
                else
                {
                    gridData.LookupClose();
                }
                gridData.SaveJson();
            }
            else
            {
                gridData.LookupClose();
            }
        }
    }

    /// <summary>
    /// Set focus on focused GridCell, or to null, if cell does not exist anymore.
    /// </summary>
    internal class ProcessGridFocus : Process
    {
        private void IsFocus(GridDataJson gridDataJson)
        {
            foreach (string gridName in gridDataJson.RowList.Keys)
            {
                foreach (GridRow gridRow in gridDataJson.RowList[gridName])
                {
                    foreach (var gridColumn in gridDataJson.ColumnList[gridName])
                    {
                        GridCell gridCell = gridDataJson.CellList[gridName][gridColumn.FieldName][gridRow.Index];
                        bool isSelect = gridDataJson.FocusGridName == gridName && gridDataJson.FocusFieldName == gridColumn.FieldName && gridDataJson.FocusIndex == gridRow.Index;
                        gridCell.IsFocus = isSelect;
                    }
                }
            }
        }

        protected internal override void Run(App app)
        {
            GridDataJson gridDataJson = app.AppJson.GridDataJson;
            bool isExist = false; // Focused field exists
            if (gridDataJson.FocusFieldName != null)
            {
                if (gridDataJson.RowList[gridDataJson.FocusGridName].Exists(item => item.Index == gridDataJson.FocusIndex)) // Focused row exists
                {
                    if (gridDataJson.ColumnList[gridDataJson.FocusGridName].Exists(item => item.FieldName == gridDataJson.FocusFieldName)) // Focused column exists
                    {
                        isExist = true;
                    }
                }
            }
            if (isExist == false)
            {
                if (app.AppJson.GridDataJson != null)
                {
                    app.AppJson.GridDataJson.FocusFieldName = null;
                    app.AppJson.GridDataJson.FocusGridName = null;
                    app.AppJson.GridDataJson.FocusIndex = null;
                }
            }
            //
            IsFocus(gridDataJson);
        }
    }

    internal class ProcessGridFieldWithLabelIndex : Process
    {
        protected internal override void Run(App app)
        {
            foreach (GridFieldWithLabel gridFieldWithLabel in app.AppJson.ListAll().OfType<GridFieldWithLabel>())
            {
                gridFieldWithLabel.Index = app.GridData.RowSelectedIndex(new GridName(gridFieldWithLabel.GridName, true))?.Value; // Set index to selected row.
            }
        }
    }

    internal class ProcessGridSave : Process
    {
        protected internal override void Run(App app)
        {
            bool isSave = false;
            GridDataJson gridDataJson = app.AppJson.GridDataJson;
            foreach (string gridName in gridDataJson.RowList.Keys)
            {
                foreach (GridRow gridRow in gridDataJson.RowList[gridName])
                {
                    foreach (var gridColumn in gridDataJson.ColumnList[gridName])
                    {
                        GridCell gridCell = gridDataJson.CellList[gridName][gridColumn.FieldName][gridRow.Index];
                        if (gridCell.IsModify)
                        {
                            isSave = true;
                            break;
                        }
                    }
                }
            }
            //
            if (isSave)
            {
                app.GridData.TextParse();
                app.GridData.SaveDatabase();
                app.GridData.SaveJson();
            }
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
            string fieldNameClick = null;
            foreach (string gridName in gridDataJson.RowList.Keys)
            {
                foreach (GridRow gridRow in gridDataJson.RowList[gridName])
                {
                    foreach (var gridColumn in gridDataJson.ColumnList[gridName])
                    {
                        GridCell gridCell = gridDataJson.CellList[gridName][gridColumn.FieldName][gridRow.Index];
                        if (gridCell.IsModify && gridCell.CellEnum == GridCellEnum.Button)
                        {
                            gridNameClick = gridName;
                            indexClick = gridRow.Index;
                            fieldNameClick = gridColumn.FieldName;
                            break;
                        }
                    }
                }
            }
            //
            if (gridNameClick != null)
            {
                Row row = app.GridData.Row(new GridName(gridNameClick, true), new Index(indexClick));
                Type typeRow = app.GridData.TypeRow(new GridName(gridNameClick, true));
                Cell cell = UtilDataAccessLayer.CellList(typeRow, row).Where(item => item.FieldNameCSharp == fieldNameClick).Single();
                bool isReload = false;
                bool isException = false;
                try
                {
                    cell.CellButtonIsClick(app, new GridName(gridNameClick, true), new Index(indexClick), row, fieldNameClick, ref isReload);
                }
                catch (Exception exception)
                {
                    isException = true;
                    app.GridData.ErrorRowSet(new GridName(gridNameClick, true), new Index(indexClick), UtilFramework.ExceptionToText(exception));
                }
                if (isReload && isException == false)
                {
                    app.GridData.LoadDatabaseReload(new GridName(gridNameClick, true));
                }
            }
        }
    }
}