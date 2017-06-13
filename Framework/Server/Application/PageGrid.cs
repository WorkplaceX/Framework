namespace Framework.Server.Application
{
    using Framework.Server.Application.Json;
    using Framework.Server.DataAccessLayer;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Page to process data grid.
    /// </summary>
    public class PageGrid : Page
    {
        protected override void ProcessInit(ProcessList processList)
        {
            processList.Add<ProcessGridIsClick>();
            processList.Add<ProcessGridOrderBy>();
            processList.Add<ProcessGridFilter>();
            processList.Add<ProcessGridLookUp>();
//            processList.Add<ProcessGridSave>();
            processList.Add<ProcessGridCellButtonIsClick>();
            processList.Add<ProcessGridOrderByText>();
            processList.Add<ProcessGridFocusNull>();
            processList.Add<ProcessGridCellIsModifyFalse>();
            processList.Add<ProcessGridIsClickFalse>();
            base.ProcessInit(processList);
        }

        private bool isGridDataTextParse;

        /// <summary>
        /// Make sure method GridData.Text(); has been called. It's called only once.
        /// </summary>
        public void GridDataTextParse()
        {
            if (isGridDataTextParse == false)
            {
                isGridDataTextParse = true;
                Application.GridData().TextParse();
            }
        }
    }

    /// <summary>
    /// Process OrderBy click.
    /// </summary>
    public class ProcessGridOrderBy : ProcessBase<PageGrid>
    {
        private void DatabaseLoad(ApplicationJson applicationJson, string gridName, string fieldNameOrderBy, bool isOrderByDesc)
        {
            GridDataJson gridDataJson = applicationJson.GridDataJson;
            //
            GridData gridData = Application.GridData();
            Type typeRow = gridData.TypeRow(gridName);
            gridData.LoadDatabase(gridName, null, fieldNameOrderBy, isOrderByDesc, typeRow);
            gridData.SaveJson(applicationJson);
        }

        protected internal override void Process()
        {
            // Detect OrderBy click
            foreach (string gridName in ApplicationJson.GridDataJson.ColumnList.Keys.ToArray())
            {
                foreach (GridColumn gridColumn in ApplicationJson.GridDataJson.ColumnList[gridName])
                {
                    if (gridColumn.IsClick)
                    {
                        GridQuery gridQuery = ApplicationJson.GridDataJson.GridQueryList[gridName];
                        if (gridQuery.FieldNameOrderBy == gridColumn.FieldName)
                        {
                            gridQuery.IsOrderByDesc = !gridQuery.IsOrderByDesc;
                        }
                        else
                        {
                            gridQuery.FieldNameOrderBy = gridColumn.FieldName;
                            gridQuery.IsOrderByDesc = true;
                        }
                        DatabaseLoad(ApplicationJson, gridName, gridQuery.FieldNameOrderBy, gridQuery.IsOrderByDesc);
                        break;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Set OrderBy up or down arrow.
    /// </summary>
    public class ProcessGridOrderByText : ProcessBase
    {
        protected internal override void Process()
        {
            foreach (string gridName in ApplicationJson.GridDataJson.ColumnList.Keys)
            {
                GridQuery gridQuery = ApplicationJson.GridDataJson.GridQueryList[gridName];
                foreach (GridColumn gridColumn in ApplicationJson.GridDataJson.ColumnList[gridName])
                {
                    gridColumn.IsClick = false;
                    if (gridColumn.FieldName == gridQuery.FieldNameOrderBy)
                    {
                        if (gridQuery.IsOrderByDesc)
                        {
                            gridColumn.Text = "▼" + " " + gridColumn.FieldName;
                        }
                        else
                        {
                            gridColumn.Text = "▲" + " " + gridColumn.FieldName;
                        }
                    }
                    else
                    {
                        gridColumn.Text = gridColumn.FieldName;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Process data grid filter.
    /// </summary>
    public class ProcessGridFilter : ProcessBase<PageGrid>
    {
        protected internal override void Process()
        {
            List<string> gridNameList = new List<string>();
            foreach (string gridName in ApplicationJson.GridDataJson.ColumnList.Keys)
            {
                foreach (GridRow gridRow in ApplicationJson.GridDataJson.RowList[gridName])
                {
                    if (Util.IndexToIndexEnum(gridRow.Index) == IndexEnum.Filter)
                    {
                        foreach (GridColumn gridColumn in ApplicationJson.GridDataJson.ColumnList[gridName])
                        {
                            GridCell gridCell = ApplicationJson.GridDataJson.CellList[gridName][gridColumn.FieldName][gridRow.Index];
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
                Page.GridDataTextParse();
                GridData gridData = Application.GridData();
                gridData.LoadDatabase(gridName);
                gridData.SaveJson(ApplicationJson);
            }
        }
    }

    /// <summary>
    /// Grid row or cell is clicked. Set focus.
    /// </summary>
    public class ProcessGridIsClick : ProcessBase
    {
        private void ProcessGridSelectRowClear(ApplicationJson applicationJson, string gridName)
        {
            foreach (GridRow gridRow in applicationJson.GridDataJson.RowList[gridName])
            {
                gridRow.IsSelectSet(false);
            }
        }

        private void ProcessGridSelectCell(ApplicationJson applicationJson, string gridName, string index, string fieldName)
        {
            GridDataJson gridDataJson = applicationJson.GridDataJson;
            gridDataJson.FocusGridName = gridName;
            gridDataJson.FocusIndex = index;
            gridDataJson.FocusFieldName = fieldName;
            ProcessGridSelectCellClear(applicationJson);
            gridDataJson.CellList[gridName][fieldName][index].IsSelect = true;
        }

        private void ProcessGridSelectCellClear(ApplicationJson applicationJson)
        {
            GridDataJson gridDataJson = applicationJson.GridDataJson;
            foreach (string gridName in gridDataJson.RowList.Keys)
            {
                foreach (GridRow gridRow in gridDataJson.RowList[gridName])
                {
                    foreach (var gridColumn in gridDataJson.ColumnList[gridName])
                    {
                        GridCell gridCell = gridDataJson.CellList[gridName][gridColumn.FieldName][gridRow.Index];
                        gridCell.IsSelect = false;
                    }
                }
            }
        }

        protected internal override void Process()
        {
            GridDataJson gridDataJson = ApplicationJson.GridDataJson;
            foreach (GridQuery gridQuery in gridDataJson.GridQueryList.Values)
            {
                string gridName = gridQuery.GridName;
                foreach (GridRow gridRow in gridDataJson.RowList[gridName])
                {
                    if (gridRow.IsClick)
                    {
                        ProcessGridSelectRowClear(ApplicationJson, gridName);
                        gridRow.IsSelectSet(true);
                    }
                    foreach (var gridColumn in gridDataJson.ColumnList[gridName])
                    {
                        GridCell gridCell = gridDataJson.CellList[gridName][gridColumn.FieldName][gridRow.Index];
                        if (gridCell.IsClick == true)
                        {
                            ProcessGridSelectCell(ApplicationJson, gridName, gridRow.Index, gridColumn.FieldName);
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Set row and cell IsClick to false
    /// </summary>
    public class ProcessGridIsClickFalse : ProcessBase
    {
        protected internal override void Process()
        {
            GridDataJson gridDataJson = ApplicationJson.GridDataJson;
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
        }
    }

    public class ProcessGridCellIsModifyFalse : ProcessBase
    {
        protected internal override void Process()
        {
            GridDataJson gridDataJson = ApplicationJson.GridDataJson;
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

    public class ProcessGridLookUp : ProcessBase<PageGrid>
    {
        protected internal override void Process()
        {
            bool isLookUp = false;
            GridDataJson gridDataJson = ApplicationJson.GridDataJson;
            foreach (string gridName in gridDataJson.RowList.Keys)
            {
                foreach (GridRow gridRow in gridDataJson.RowList[gridName])
                {
                    foreach (var gridColumn in gridDataJson.ColumnList[gridName])
                    {
                        GridCell gridCell = gridDataJson.CellList[gridName][gridColumn.FieldName][gridRow.Index];
                        if (gridCell.IsClick || gridCell.IsModify)
                        {
                            isLookUp = true;
                            break;
                        }
                    }
                }
            }
            //
            if (isLookUp)
            {
                if (gridDataJson.FocusFieldName != null)
                {
                    GridData gridData = Application.GridData();
                    Type typeRow = gridData.TypeRow(gridDataJson.FocusGridName);
                    var row = gridData.Row(gridDataJson.FocusGridName, gridDataJson.FocusIndex);
                    DataAccessLayer.Cell cell = DataAccessLayer.Util.CellList(typeRow, row).Where(item => item.FieldNameCSharp == gridDataJson.FocusFieldName).First();
                    List<DataAccessLayer.Row> rowList;
                    cell.LookUp(out typeRow, out rowList);
                    gridData.LoadRow("LookUp", typeRow, rowList);
                    gridData.SaveJson(ApplicationJson);
                }
            }
        }
    }

    /// <summary>
    /// Set focus to null, if cell does not exist anymore.
    /// </summary>
    public class ProcessGridFocusNull : ProcessBase
    {
        protected internal override void Process()
        {
            GridDataJson gridDataJson = ApplicationJson.GridDataJson;
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
                if (ApplicationJson.GridDataJson != null)
                {
                    ApplicationJson.GridDataJson.FocusFieldName = null;
                    ApplicationJson.GridDataJson.FocusGridName = null;
                    ApplicationJson.GridDataJson.FocusIndex = null;
                }
            }
        }
    }

    public class ProcessGridSave : ProcessBase<PageGrid>
    {
        protected internal override void Process()
        {
            bool isSave = false;
            GridDataJson gridDataJson = ApplicationJson.GridDataJson;
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
                Application.GridData().TextParse();
                Application.GridData().SaveDatabase();
                Application.GridData().SaveJson(ApplicationJson);
            }
        }
    }

    /// <summary>
    /// Cell rendered as button is clicked.
    /// </summary>
    public class ProcessGridCellButtonIsClick : ProcessBase<PageGrid>
    {
        protected internal override void Process()
        {
            GridDataJson gridDataJson = ApplicationJson.GridDataJson;
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
                        if (gridCell.IsModify && gridCell.T == "Click")
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
                Row row = Application.GridData().Row(gridNameClick, indexClick);
                Cell cell = DataAccessLayer.Util.CellList(row.GetType(), row).Where(item => item.FieldNameCSharp == fieldNameClick).Single();
                cell.CellProcessButtonIsClick(Page, gridNameClick, indexClick, fieldNameClick);
            }
        }
    }
}