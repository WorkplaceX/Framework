namespace Framework.Server.Application
{
    using Framework.Server.Application.Json;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public abstract class ProcessBase
    {
        public ProcessBase(ApplicationServerBase applicationServer)
        {
            this.ApplicationServer = applicationServer;
        }

        public readonly ApplicationServerBase ApplicationServer;

        protected virtual internal void ProcessBegin(ApplicationJson applicationJson)
        {

        }

        protected virtual internal void ProcessEnd(ApplicationJson applicationJson)
        {

        }
    }

    public class ProcessGridOrderBy : ProcessBase
    {
        public ProcessGridOrderBy(ApplicationServerBase applicationServer) 
            : base(applicationServer)
        {

        }

        protected internal override void ProcessBegin(ApplicationJson applicationJson)
        {
            foreach (string gridName in applicationJson.GridDataJson.ColumnList.Keys)
            {
                foreach (GridColumn gridColumn in applicationJson.GridDataJson.ColumnList[gridName])
                {
                    if (gridColumn.IsClick)
                    {
                        GridLoad gridLoad = applicationJson.GridDataJson.GridLoadList[gridName];
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

        protected internal override void ProcessEnd(ApplicationJson applicationJson)
        {
            foreach (string gridName in applicationJson.GridDataJson.ColumnList.Keys)
            {
                GridLoad gridLoad = applicationJson.GridDataJson.GridLoadList[gridName];
                foreach (GridColumn gridColumn in applicationJson.GridDataJson.ColumnList[gridName])
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
        public ProcessJson(ApplicationServerBase applicationServer)
            : base(applicationServer)
        {

        }

        protected internal override void ProcessEnd(ApplicationJson applicationJson)
        {
            foreach (Component jsonComponent in applicationJson.ListAll())
            {
                jsonComponent.Process(ApplicationServer, applicationJson);
            }
        }
    }

    public class ProcessGridIsIsClick : ProcessBase
    {
        public ProcessGridIsIsClick(ApplicationServerBase applicationServer)
            : base(applicationServer)
        {

        }

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

        protected internal override void ProcessBegin(ApplicationJson applicationJson)
        {
            GridDataJson gridDataJson = applicationJson.GridDataJson;
            foreach (GridLoad gridLoad in gridDataJson.GridLoadList.Values)
            {
                string gridName = gridLoad.GridName;
                foreach (GridRow gridRow in gridDataJson.RowList[gridName])
                {
                    if (gridRow.IsClick)
                    {
                        ProcessGridSelectRowClear(applicationJson, gridName);
                        gridRow.IsSelectSet(true);
                    }
                    foreach (var gridColumn in gridDataJson.ColumnList[gridName])
                    {
                        GridCell gridCell = gridDataJson.CellList[gridName][gridColumn.FieldName][gridRow.Index];
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
            foreach (GridLoad gridLoad in gridDataJson.GridLoadList.Values)
            {
                string gridName = gridLoad.GridName;
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

    /// <summary>
    /// Process first application request coming from client.
    /// </summary>
    public class ProcessApplicationInit : ProcessBase
    {
        public ProcessApplicationInit(ApplicationServerBase applicationServer)
            : base(applicationServer)
        {

        }

        protected internal override void ProcessBegin(ApplicationJson applicationJson)
        {
            if (applicationJson.Session == Guid.Empty) // First application request coming from client.
            {
                applicationJson.Session = Guid.NewGuid();
                ApplicationServer.ApplicationJsonInit(applicationJson);
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
        public ProcessGridSave(ApplicationServerBase applicationServer) 
            : base(applicationServer)
        {

        }

        public bool IsModify;

        protected internal override void ProcessBegin(ApplicationJson applicationJson)
        {
            IsModify = false;
            GridDataJson gridDataJson = applicationJson.GridDataJson;
            //
            GridDataServer gridDataServer = new GridDataServer();
            gridDataServer.LoadJson(applicationJson, ApplicationServer.GetType());
            gridDataServer.TextParse();
            gridDataServer.SaveDatabase();
            //
            foreach (string gridName in gridDataJson.GridLoadList.Keys)
            {
                foreach (GridRow gridRow in gridDataJson.RowList[gridName])
                {
                    foreach (GridColumn gridColumn in gridDataJson.ColumnList[gridName])
                    {
                        GridCell gridCell = gridDataJson.CellList[gridName][gridColumn.FieldName][gridRow.Index];
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
        public ProcessGridRowFirstIsClick(ApplicationServerBase applicationServer) 
            : base(applicationServer)
        {

        }

        protected internal override void ProcessBegin(ApplicationJson applicationJson)
        {
            GridDataJson gridDataJson = applicationJson.GridDataJson;
            foreach (string gridName in gridDataJson.RowList.Keys)
            {
                bool isSelect = false; // A row is selected
                foreach (GridRow gridRow in gridDataJson.RowList[gridName])
                {
                    if (gridRow.IsSelectGet() || gridRow.IsClick)
                    {
                        isSelect = true;
                        break;
                    }
                }
                if (isSelect == false)
                {
                    foreach (GridRow gridRow in gridDataJson.RowList[gridName])
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
        public ProcessGridLookUp(ApplicationServerBase applicationServer)
            : base(applicationServer)
        {

        }

        protected internal override void ProcessBegin(ApplicationJson applicationJson)
        {
            GridDataJson gridDataJson = applicationJson.GridDataJson;
            if (gridDataJson.FocusFieldName != null)
            {
                GridDataServer gridDataServer = new GridDataServer();
                gridDataServer.LoadJson(applicationJson, gridDataJson.FocusGridName, ApplicationServer.GetType());
                var row = gridDataServer.RowGet(gridDataJson.FocusGridName, gridDataJson.FocusIndex);
                DataAccessLayer.Cell cell = DataAccessLayer.Util.CellList(row.Row).Where(item => item.FieldNameCSharp == gridDataJson.FocusFieldName).First();
                Type typeRow;
                List<DataAccessLayer.Row> rowList;
                cell.LookUp(out typeRow, out rowList);
                gridDataServer = new GridDataServer();
                gridDataServer.Load("LookUp", typeRow, rowList);
                gridDataServer.SaveJson(applicationJson);
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
