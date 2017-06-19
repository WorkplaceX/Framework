namespace Framework.Server.Application
{
    using Framework.Server.Application.Json;
    using Framework.Server.DataAccessLayer;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Process OrderBy click.
    /// </summary>
    public class ProcessGridOrderBy : Process
    {
        private void DatabaseLoad(ApplicationBase application, ApplicationJson applicationJson, string gridName, string fieldNameOrderBy, bool isOrderByDesc)
        {
            GridDataJson gridDataJson = applicationJson.GridDataJson;
            //
            GridData gridData = application.GridData();
            Type typeRow = gridData.TypeRow(gridName);
            gridData.LoadDatabase(gridName, null, fieldNameOrderBy, isOrderByDesc, typeRow);
            gridData.SaveJson(applicationJson);
        }

        protected internal override void Run(ApplicationBase application)
        {
            ApplicationJson applicationJson = application.ApplicationJson;
            // Detect OrderBy click
            foreach (string gridName in applicationJson.GridDataJson.ColumnList.Keys.ToArray())
            {
                foreach (GridColumn gridColumn in applicationJson.GridDataJson.ColumnList[gridName])
                {
                    if (gridColumn.IsClick)
                    {
                        GridQuery gridQuery = applicationJson.GridDataJson.GridQueryList[gridName];
                        if (gridQuery.FieldNameOrderBy == gridColumn.FieldName)
                        {
                            gridQuery.IsOrderByDesc = !gridQuery.IsOrderByDesc;
                        }
                        else
                        {
                            gridQuery.FieldNameOrderBy = gridColumn.FieldName;
                            gridQuery.IsOrderByDesc = true;
                        }
                        DatabaseLoad(application, applicationJson, gridName, gridQuery.FieldNameOrderBy, gridQuery.IsOrderByDesc);
                        break;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Set OrderBy up or down arrow.
    /// </summary>
    public class ProcessGridOrderByText : Process
    {
        protected internal override void Run(ApplicationBase application)
        {
            ApplicationJson applicationJson = application.ApplicationJson;
            //
            foreach (string gridName in applicationJson.GridDataJson.ColumnList.Keys)
            {
                GridQuery gridQuery = applicationJson.GridDataJson.GridQueryList[gridName];
                foreach (GridColumn gridColumn in applicationJson.GridDataJson.ColumnList[gridName])
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
    public class ProcessGridFilter : Process
    {
        protected internal override void Run(ApplicationBase application)
        {
            ApplicationJson applicationJson = application.ApplicationJson;
            //
            List<string> gridNameList = new List<string>();
            foreach (string gridName in applicationJson.GridDataJson.ColumnList.Keys)
            {
                foreach (GridRow gridRow in applicationJson.GridDataJson.RowList[gridName])
                {
                    if (Util.IndexToIndexEnum(gridRow.Index) == IndexEnum.Filter)
                    {
                        foreach (GridColumn gridColumn in applicationJson.GridDataJson.ColumnList[gridName])
                        {
                            GridCell gridCell = applicationJson.GridDataJson.CellList[gridName][gridColumn.FieldName][gridRow.Index];
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
                application.GridDataTextParse();
                GridData gridData = application.GridData();
                gridData.LoadDatabase(gridName);
                gridData.SaveJson(applicationJson);
            }
        }
    }

    /// <summary>
    /// Grid row or cell is clicked. Set focus.
    /// </summary>
    public class ProcessGridIsClick : Process
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

        protected internal override void Run(ApplicationBase application)
        {
            GridDataJson gridDataJson = application.ApplicationJson.GridDataJson;
            foreach (GridQuery gridQuery in gridDataJson.GridQueryList.Values)
            {
                string gridName = gridQuery.GridName;
                foreach (GridRow gridRow in gridDataJson.RowList[gridName])
                {
                    if (gridRow.IsClick)
                    {
                        ProcessGridSelectRowClear(application.ApplicationJson, gridName);
                        gridRow.IsSelectSet(true);
                    }
                    foreach (var gridColumn in gridDataJson.ColumnList[gridName])
                    {
                        GridCell gridCell = gridDataJson.CellList[gridName][gridColumn.FieldName][gridRow.Index];
                        if (gridCell.IsClick == true)
                        {
                            ProcessGridSelectCell(application.ApplicationJson, gridName, gridRow.Index, gridColumn.FieldName);
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Set row and cell IsClick to false
    /// </summary>
    public class ProcessGridIsClickFalse : Process
    {
        protected internal override void Run(ApplicationBase application)
        {
            GridDataJson gridDataJson = application.ApplicationJson.GridDataJson;
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

    public class ProcessGridCellIsModifyFalse : Process
    {
        protected internal override void Run(ApplicationBase application)
        {
            GridDataJson gridDataJson = application.ApplicationJson.GridDataJson;
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

    public class ProcessGridLookUp : Process
    {
        protected internal override void Run(ApplicationBase application)
        {
            bool isLookUp = false;
            GridDataJson gridDataJson = application.ApplicationJson.GridDataJson;
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
                    GridData gridData = application.GridData();
                    Type typeRow = gridData.TypeRow(gridDataJson.FocusGridName);
                    var row = gridData.Row(gridDataJson.FocusGridName, gridDataJson.FocusIndex);
                    DataAccessLayer.Cell cell = DataAccessLayer.Util.CellList(typeRow, row).Where(item => item.FieldNameCSharp == gridDataJson.FocusFieldName).First();
                    List<DataAccessLayer.Row> rowList;
                    cell.LookUp(out typeRow, out rowList);
                    gridData.LoadRow("LookUp", typeRow, rowList);
                    gridData.SaveJson(application.ApplicationJson);
                }
            }
        }
    }

    /// <summary>
    /// Set focus to null, if cell does not exist anymore.
    /// </summary>
    public class ProcessGridFocusNull : Process
    {
        protected internal override void Run(ApplicationBase application)
        {
            GridDataJson gridDataJson = application.ApplicationJson.GridDataJson;
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
                if (application.ApplicationJson.GridDataJson != null)
                {
                    application.ApplicationJson.GridDataJson.FocusFieldName = null;
                    application.ApplicationJson.GridDataJson.FocusGridName = null;
                    application.ApplicationJson.GridDataJson.FocusIndex = null;
                }
            }
        }
    }

    public class ProcessGridSave : Process
    {
        protected internal override void Run(ApplicationBase application)
        {
            bool isSave = false;
            GridDataJson gridDataJson = application.ApplicationJson.GridDataJson;
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
                application.GridData().TextParse();
                application.GridData().SaveDatabase();
                application.GridData().SaveJson(application.ApplicationJson);
            }
        }
    }

    /// <summary>
    /// Cell rendered as button is clicked.
    /// </summary>
    public class ProcessGridCellButtonIsClick : Process
    {
        protected internal override void Run(ApplicationBase application)
        {
            GridDataJson gridDataJson = application.ApplicationJson.GridDataJson;
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
                Row row = application.GridData().Row(gridNameClick, indexClick);
                Cell cell = DataAccessLayer.Util.CellList(row.GetType(), row).Where(item => item.FieldNameCSharp == fieldNameClick).Single();
                cell.CellProcessButtonIsClick(application, gridNameClick, indexClick, fieldNameClick);
            }
        }
    }
}