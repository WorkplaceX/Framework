namespace Framework.Server.Application
{
    using Framework.Server.Application.Json;
    using Framework.Server.DataAccessLayer;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public abstract class ProcessBase
    {
        public ProcessBase(ApplicationBase application)
        {
            this.Application = application;
        }

        public readonly ApplicationBase Application;

        protected virtual internal void ProcessBegin(ApplicationJson applicationJson)
        {

        }

        protected virtual internal void ProcessEnd(ApplicationJson applicationJson)
        {

        }
    }

    /// <summary>
    /// Process data grid filter.
    /// </summary>
    public class ProcessGridFilter : ProcessBase
    {
        public ProcessGridFilter(ApplicationBase application)
            : base(application)
        {

        }

        /// <summary>
        /// Detect data grid filter text changes from user.
        /// </summary>
        protected internal override void ProcessBegin(ApplicationJson applicationJson)
        {
            List<string> gridNameList = new List<string>();
            foreach (string gridName in applicationJson.GridDataJson.ColumnList.Keys)
            {
                foreach (Json.GridRow gridRow in applicationJson.GridDataJson.RowList[gridName])
                {
                    if (Util.IndexToIndexEnum(gridRow.Index) == IndexEnum.Filter)
                    {
                        foreach (Json.GridColumn gridColumn in applicationJson.GridDataJson.ColumnList[gridName])
                        {
                            Json.GridCell gridCell = applicationJson.GridDataJson.CellList[gridName][gridColumn.FieldName][gridRow.Index];
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
                GridData gridData = new GridData();
                gridData.LoadJson(applicationJson, Application.GetType());
                gridData.TextParse(); // Parse text filter.
                gridData.LoadDatabase(gridName);
                gridData.SaveJson(applicationJson);
            }
        }
    }

    public class ProcessGridCellButtonIsClick : ProcessBase
    {
        public ProcessGridCellButtonIsClick(ApplicationBase application)
            : base(application)
        {

        }

        protected internal override void ProcessBegin(ApplicationJson applicationJson)
        {
            GridData gridData = new GridData();
            gridData.LoadJson(applicationJson, Application.GetType());
            //
            foreach (string gridName in gridData.GridNameList())
            {
                foreach (DataAccessLayer.Cell column in gridData.ColumnList(gridName))
                {
                    if (column.ColumnIsButton())
                    {
                        foreach (string index in gridData.IndexList(gridName))
                        {
                            string text = gridData.CellText(gridName, index, column.FieldNameCSharp);
                            if (text == "Click")
                            {
                                DataAccessLayer.Row row = gridData.Row(gridName, index);
                                Cell cell = column.Constructor(row);
                                cell.CellProcessButtonIsClick();
                            }
                        }
                    }
                }
            }
        }
    }

    public class ProcessGridOrderBy : ProcessBase
    {
        public ProcessGridOrderBy(ApplicationBase application) 
            : base(application)
        {

        }

        private void DatabaseLoad(ApplicationJson applicationJson, string gridName, string fieldNameOrderBy, bool isOrderByDesc)
        {
            GridDataJson gridDataJson = applicationJson.GridDataJson;
            //
            GridData gridData = new GridData();
            gridData.LoadJson(applicationJson, gridName, Application.GetType());
            Type typeRow = gridData.TypeRow(gridName);
            gridData.LoadDatabase(gridName, null, fieldNameOrderBy, isOrderByDesc, typeRow);
            gridData.SaveJson(applicationJson);
        }

        protected internal override void ProcessBegin(ApplicationJson applicationJson)
        {
            // Detect OrderBy click
            foreach (string gridName in applicationJson.GridDataJson.ColumnList.Keys.ToArray())
            {
                foreach (Json.GridColumn gridColumn in applicationJson.GridDataJson.ColumnList[gridName])
                {
                    if (gridColumn.IsClick)
                    {
                        Json.GridQuery gridQuery = applicationJson.GridDataJson.GridQueryList[gridName];
                        if (gridQuery.FieldNameOrderBy == gridColumn.FieldName)
                        {
                            gridQuery.IsOrderByDesc = !gridQuery.IsOrderByDesc;
                        }
                        else
                        {
                            gridQuery.FieldNameOrderBy = gridColumn.FieldName;
                            gridQuery.IsOrderByDesc = true;
                        }
                        DatabaseLoad(applicationJson, gridName, gridQuery.FieldNameOrderBy, gridQuery.IsOrderByDesc);
                        break;
                    }
                }
            }
        }

        protected internal override void ProcessEnd(ApplicationJson applicationJson)
        {
            foreach (string gridName in applicationJson.GridDataJson.ColumnList.Keys)
            {
                Json.GridQuery gridQuery = applicationJson.GridDataJson.GridQueryList[gridName];
                foreach (Json.GridColumn gridColumn in applicationJson.GridDataJson.ColumnList[gridName])
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
    /// Call method Process(); on class JsonComponent.
    /// </summary>
    public class ProcessJson : ProcessBase
    {
        public ProcessJson(ApplicationBase application)
            : base(application)
        {

        }

        protected internal override void ProcessEnd(ApplicationJson applicationJson)
        {
            foreach (Component jsonComponent in applicationJson.ListAll())
            {
                jsonComponent.Process(Application, applicationJson);
            }
        }
    }

    /// <summary>
    /// Set GridCell.IsModify to false.
    /// </summary>
    public class ProcessGridCellIsModifyFalse : ProcessBase
    {
        public ProcessGridCellIsModifyFalse(ApplicationBase application)
            : base(application)
        {

        }

        protected internal override void ProcessBegin(ApplicationJson applicationJson)
        {
            GridDataJson gridDataJson = applicationJson.GridDataJson;
            //
            foreach (string gridName in gridDataJson.RowList.Keys)
            {
                foreach (Json.GridRow gridRow in gridDataJson.RowList[gridName])
                {
                    foreach (var gridColumn in gridDataJson.ColumnList[gridName])
                    {
                        Json.GridCell gridCell = gridDataJson.CellList[gridName][gridColumn.FieldName][gridRow.Index];
                        if (gridCell.IsModify)
                        {
                            gridCell.IsModify = false;
                        }
                    }
                }
            }
        }
    }
    public class ProcessGridIsIsClick : ProcessBase
    {
        public ProcessGridIsIsClick(ApplicationBase application)
            : base(application)
        {

        }

        private void ProcessGridSelectRowClear(ApplicationJson applicationJson, string gridName)
        {
            foreach (Json.GridRow gridRow in applicationJson.GridDataJson.RowList[gridName])
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
                foreach (Json.GridRow gridRow in gridDataJson.RowList[gridName])
                {
                    foreach (var gridColumn in gridDataJson.ColumnList[gridName])
                    {
                        Json.GridCell gridCell = gridDataJson.CellList[gridName][gridColumn.FieldName][gridRow.Index];
                        gridCell.IsSelect = false;
                    }
                }
            }
        }

        protected internal override void ProcessBegin(ApplicationJson applicationJson)
        {
            GridDataJson gridDataJson = applicationJson.GridDataJson;
            foreach (Json.GridQuery gridQuery in gridDataJson.GridQueryList.Values)
            {
                string gridName = gridQuery.GridName;
                foreach (Json.GridRow gridRow in gridDataJson.RowList[gridName])
                {
                    if (gridRow.IsClick)
                    {
                        ProcessGridSelectRowClear(applicationJson, gridName);
                        gridRow.IsSelectSet(true);
                    }
                    foreach (var gridColumn in gridDataJson.ColumnList[gridName])
                    {
                        Json.GridCell gridCell = gridDataJson.CellList[gridName][gridColumn.FieldName][gridRow.Index];
                        if (gridCell.IsClick == true)
                        {
                            ProcessGridSelectCell(applicationJson, gridName, gridRow.Index, gridColumn.FieldName);
                        }
                    }
                }
            }
        }

        protected internal override void ProcessEnd(ApplicationJson applicationJson)
        {
            GridDataJson gridDataJson = applicationJson.GridDataJson;
            foreach (Json.GridQuery gridQuery in gridDataJson.GridQueryList.Values)
            {
                string gridName = gridQuery.GridName;
                foreach (Json.GridRow gridRow in gridDataJson.RowList[gridName])
                {
                    gridRow.IsClick = false;
                    foreach (var gridColumn in gridDataJson.ColumnList[gridName])
                    {
                        Json.GridCell gridCell = gridDataJson.CellList[gridName][gridColumn.FieldName][gridRow.Index];
                        gridCell.IsClick = false;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Process first application request coming from client.
    /// </summary>
    public class ProcessApplicationInit : ProcessBase
    {
        public ProcessApplicationInit(ApplicationBase application)
            : base(application)
        {

        }

        protected internal override void ProcessBegin(ApplicationJson applicationJson)
        {
            if (applicationJson.Session == Guid.Empty) // First application request coming from client.
            {
                applicationJson.Session = Guid.NewGuid();
                Application.ApplicationJsonInit(applicationJson);
            }
            else
            {
                applicationJson.ResponseCount += 1;
            }
            applicationJson.Name = ".NET Core=" + DateTime.Now.ToString("HH:mm:ss.fff");
            applicationJson.VersionServer = Framework.Util.VersionServer;
        }
    }

    public class ProcessGridSave : ProcessBase
    {
        public ProcessGridSave(ApplicationBase application) 
            : base(application)
        {

        }

        public bool IsModify;

        protected internal override void ProcessBegin(ApplicationJson applicationJson)
        {
            IsModify = false;
            GridDataJson gridDataJson = applicationJson.GridDataJson;
            //
            GridData gridData = new GridData();
            gridData.LoadJson(applicationJson, Application.GetType());
            gridData.TextParse();
            gridData.SaveDatabase();
            gridData.SaveJson(applicationJson);
            //
            foreach (string gridName in gridDataJson.GridQueryList.Keys)
            {
                foreach (Json.GridRow gridRow in gridDataJson.RowList[gridName])
                {
                    foreach (Json.GridColumn gridColumn in gridDataJson.ColumnList[gridName])
                    {
                        Json.GridCell gridCell = gridDataJson.CellList[gridName][gridColumn.FieldName][gridRow.Index];
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
        public ProcessGridRowFirstIsClick(ApplicationBase application) 
            : base(application)
        {

        }

        protected internal override void ProcessBegin(ApplicationJson applicationJson)
        {
            GridDataJson gridDataJson = applicationJson.GridDataJson;
            foreach (string gridName in gridDataJson.RowList.Keys)
            {
                bool isSelect = false; // A row is selected
                foreach (Json.GridRow gridRow in gridDataJson.RowList[gridName])
                {
                    if (gridRow.IsSelectGet() || gridRow.IsClick)
                    {
                        isSelect = true;
                        break;
                    }
                }
                if (isSelect == false)
                {
                    foreach (Json.GridRow gridRow in gridDataJson.RowList[gridName])
                    {
                        int index;
                        if (int.TryParse(gridRow.Index, out index)) // Exclude "Filter"
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
        public ProcessGridLookUp(ApplicationBase application)
            : base(application)
        {

        }

        protected internal override void ProcessBegin(ApplicationJson applicationJson)
        {
            GridDataJson gridDataJson = applicationJson.GridDataJson;
            if (gridDataJson.FocusFieldName != null)
            {
                GridData gridData = new GridData();
                gridData.LoadJson(applicationJson, gridDataJson.FocusGridName, Application.GetType());
                Type typeRow = gridData.TypeRow(gridDataJson.FocusGridName);
                var row = gridData.Row(gridDataJson.FocusGridName, gridDataJson.FocusIndex);
                DataAccessLayer.Cell cell = DataAccessLayer.Util.CellList(typeRow, row).Where(item => item.FieldNameCSharp == gridDataJson.FocusFieldName).First();
                List<DataAccessLayer.Row> rowList;
                cell.LookUp(out typeRow, out rowList);
                gridData = new GridData();
                gridData.LoadRow("LookUp", typeRow, rowList);
                gridData.SaveJson(applicationJson);
            }
        }

        protected internal override void ProcessEnd(ApplicationJson applicationJson)
        {
            GridDataJson gridDataJson = applicationJson.GridDataJson;
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
                if (applicationJson.GridDataJson != null)
                {
                    applicationJson.GridDataJson.FocusFieldName = null;
                    applicationJson.GridDataJson.FocusGridName = null;
                    applicationJson.GridDataJson.FocusIndex = null;
                }
            }
        }
    }
}
