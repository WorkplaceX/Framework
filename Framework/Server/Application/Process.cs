﻿namespace Framework.Server.Application
{
    using Framework.Server.Application.Json;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public abstract class ProcessBase
    {
        public ProcessBase(BusinessApplicationBase businessApplication)
        {
            this.BusinessApplication = businessApplication;
        }

        public readonly BusinessApplicationBase BusinessApplication;

        protected virtual internal void ProcessBegin(JsonApplication jsonApplication)
        {

        }

        protected virtual internal void ProcessBegin(JsonApplication jsonApplicationIn, JsonApplication jsonApplicationOut)
        {

        }

        protected virtual internal void ProcessEnd(JsonApplication jsonApplication)
        {

        }

        protected virtual internal void ProcessEnd(JsonApplication jsonApplicationIn, JsonApplication jsonApplicationOut)
        {

        }
    }

    public class ProcessGridOrderBy : ProcessBase
    {
        public ProcessGridOrderBy(BusinessApplicationBase businessApplication) 
            : base(businessApplication)
        {

        }

        protected internal override void ProcessBegin(JsonApplication jsonApplication)
        {
            foreach (string gridName in jsonApplication.GridData.ColumnList.Keys)
            {
                foreach (GridColumn gridColumn in jsonApplication.GridData.ColumnList[gridName])
                {
                    if (gridColumn.IsClick)
                    {
                        GridLoad gridLoad = jsonApplication.GridData.GridLoadList[gridName];
                        if (gridLoad.FieldNameOrderBy == gridColumn.FieldName)
                        {
                            gridLoad.IsOrderByDesc = !gridLoad.IsOrderByDesc;
                        }
                        else
                        {
                            gridLoad.FieldNameOrderBy = gridColumn.FieldName;
                            gridLoad.IsOrderByDesc = true;
                        }
                        break;
                    }
                }
            }
        }

        protected internal override void ProcessEnd(JsonApplication jsonApplication)
        {
            foreach (string gridName in jsonApplication.GridData.ColumnList.Keys)
            {
                GridLoad gridLoad = jsonApplication.GridData.GridLoadList[gridName];
                foreach (GridColumn gridColumn in jsonApplication.GridData.ColumnList[gridName])
                {
                    gridColumn.IsClick = false;
                    if (gridColumn.FieldName == gridLoad.FieldNameOrderBy)
                    {
                        if (gridLoad.IsOrderByDesc)
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
    /// Call method Process(); on class JsonComponent.
    /// </summary>
    public class ProcessJson : ProcessBase
    {
        public ProcessJson(BusinessApplicationBase businessApplication)
            : base(businessApplication)
        {

        }

        protected internal override void ProcessEnd(JsonApplication jsonApplication)
        {
            foreach (Component jsonComponent in jsonApplication.ListAll())
            {
                jsonComponent.Process(BusinessApplication, jsonApplication);
            }
        }
    }

    public class ProcessGridIsIsClick : ProcessBase
    {
        public ProcessGridIsIsClick(BusinessApplicationBase businessApplication)
            : base(businessApplication)
        {

        }

        private void ProcessGridSelectRowClear(JsonApplication jsonApplicatio, string gridName)
        {
            foreach (GridRow gridRow in jsonApplicatio.GridData.RowList[gridName])
            {
                gridRow.IsSelectSet(false);
            }
        }

        private void ProcessGridSelectCell(JsonApplication jsonApplication, string gridName, string index, string fieldName)
        {
            GridData gridData = jsonApplication.GridData;
            gridData.FocusGridName = gridName;
            gridData.FocusIndex = index;
            gridData.FocusFieldName = fieldName;
            ProcessGridSelectCellClear(jsonApplication);
            gridData.CellList[gridName][fieldName][index].IsSelect = true;
        }

        private void ProcessGridSelectCellClear(JsonApplication jsonApplication)
        {
            GridData gridData = jsonApplication.GridData;
            foreach (string gridName in gridData.RowList.Keys)
            {
                foreach (GridRow gridRow in gridData.RowList[gridName])
                {
                    foreach (var gridColumn in gridData.ColumnList[gridName])
                    {
                        GridCell gridCell = gridData.CellList[gridName][gridColumn.FieldName][gridRow.Index];
                        gridCell.IsSelect = false;
                    }
                }
            }
        }

        protected internal override void ProcessBegin(JsonApplication jsonApplication)
        {
            GridData gridData = jsonApplication.GridData;
            foreach (GridLoad gridLoad in gridData.GridLoadList.Values)
            {
                string gridName = gridLoad.GridName;
                foreach (GridRow gridRow in gridData.RowList[gridName])
                {
                    if (gridRow.IsClick)
                    {
                        ProcessGridSelectRowClear(jsonApplication, gridName);
                        gridRow.IsSelectSet(true);
                    }
                    foreach (var gridColumn in gridData.ColumnList[gridName])
                    {
                        GridCell gridCell = gridData.CellList[gridName][gridColumn.FieldName][gridRow.Index];
                        if (gridCell.IsClick == true)
                        {
                            ProcessGridSelectCell(jsonApplication, gridName, gridRow.Index, gridColumn.FieldName);
                        }
                    }
                }
            }
        }

        protected internal override void ProcessEnd(JsonApplication jsonApplication)
        {
            GridData gridData = jsonApplication.GridData;
            foreach (GridLoad gridLoad in gridData.GridLoadList.Values)
            {
                string gridName = gridLoad.GridName;
                foreach (GridRow gridRow in gridData.RowList[gridName])
                {
                    gridRow.IsClick = false;
                    foreach (var gridColumn in gridData.ColumnList[gridName])
                    {
                        GridCell gridCell = gridData.CellList[gridName][gridColumn.FieldName][gridRow.Index];
                        gridCell.IsClick = false;
                    }
                }
            }
        }
    }

    public class ProcessGridSave : ProcessBase
    {
        public ProcessGridSave(BusinessApplicationBase businessApplication) 
            : base(businessApplication)
        {

        }

        public bool IsModify;

        protected internal override void ProcessBegin(JsonApplication jsonApplication)
        {
            IsModify = false;
            GridData gridData = jsonApplication.GridData;
            //
            GridDataProcess gridDataProcess = Util.GridDataProcessFromJson(jsonApplication, BusinessApplication.GetType());
            gridDataProcess.TextParse();
            gridDataProcess.SaveDatabase();
            //
            foreach (string gridName in gridData.GridLoadList.Keys)
            {
                foreach (GridRow gridRow in gridData.RowList[gridName])
                {
                    foreach (GridColumn gridColumn in gridData.ColumnList[gridName])
                    {
                        GridCell gridCell = gridData.CellList[gridName][gridColumn.FieldName][gridRow.Index];
                        if (gridCell.IsO)
                        {
                            IsModify = true;
                        }
                    }
                }
            }
        }
    }

    public class ProcessGridRowFirstIsClick : ProcessBase
    {
        public ProcessGridRowFirstIsClick(BusinessApplicationBase businessApplication) 
            : base(businessApplication)
        {

        }

        protected internal override void ProcessBegin(JsonApplication jsonApplication)
        {
            GridData gridData = jsonApplication.GridData;
            foreach (string gridName in gridData.RowList.Keys)
            {
                bool isSelect = false; // A row is selected
                foreach (GridRow gridRow in gridData.RowList[gridName])
                {
                    if (gridRow.IsSelectGet() || gridRow.IsClick)
                    {
                        isSelect = true;
                        break;
                    }
                }
                if (isSelect == false)
                {
                    foreach (GridRow gridRow in gridData.RowList[gridName])
                    {
                        int index;
                        if (int.TryParse(gridRow.Index, out index)) // Exclude "Header"
                        {
                            gridRow.IsClick = true;
                            break;
                        }
                    }
                }
            }
        }
    }

    public class ProcessGridLookUp : ProcessBase
    {
        public ProcessGridLookUp(BusinessApplicationBase businessApplication)
            : base(businessApplication)
        {

        }

        protected internal override void ProcessBegin(JsonApplication jsonApplication)
        {
            GridData gridData = jsonApplication.GridData;
            if (gridData.FocusFieldName != null)
            {
                var grid = Util.GridFromJson(jsonApplication, gridData.FocusGridName, BusinessApplication.GetType());
                var row = grid.RowList[int.Parse(gridData.FocusIndex)];
                DataAccessLayer.Cell cell = DataAccessLayer.Util.CellList(row).Where(item => item.FieldNameCSharp == gridData.FocusFieldName).First();
                Type typeRow;
                List<DataAccessLayer.Row> rowList;
                cell.LookUp(out typeRow, out rowList);
                Util.TypeRowValidate(typeRow, ref rowList);
                Util.GridToJson(jsonApplication, "LookUp", typeRow, rowList);
            }
        }

        protected internal override void ProcessEnd(JsonApplication jsonApplication)
        {
            GridData gridData = jsonApplication.GridData;
            bool isExist = false; // Focused field exists
            if (gridData.FocusFieldName != null)
            {
                if (gridData.RowList[gridData.FocusGridName].Exists(item => item.Index == gridData.FocusIndex)) // Focused row exists
                {
                    if (gridData.ColumnList[gridData.FocusGridName].Exists(item => item.FieldName == gridData.FocusFieldName)) // Focused column exists
                    {
                        isExist = true;
                    }
                }
            }
            if (isExist == false)
            {
                if (jsonApplication.GridData != null)
                {
                    jsonApplication.GridData.FocusFieldName = null;
                    jsonApplication.GridData.FocusGridName = null;
                    jsonApplication.GridData.FocusIndex = null;
                }
            }
        }
    }
}