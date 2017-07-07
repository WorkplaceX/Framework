﻿using Framework.Application;
using Framework.DataAccessLayer;

namespace Database.dbo
{
    public partial class FrameworkFileStorage
    {
        [TypeCell(typeof(FrameworkFileStorage_Download))]
        public string Download { get; set; }
    }

    public partial class FrameworkFileStorage_Download : Cell<FrameworkFileStorage>
    {
        protected override internal void CellIsHtml(App app, string gridName, string index, ref bool result)
        {
            result = true;
        }

        protected override internal void CellValueToText(App app, string gridName, string index, ref string result)
        {
            result = null;
            if (UtilApplication.IndexEnumFromText(index) == IndexEnum.Index && Row.FileName != null)
            {
                string fileNameOnly = Row.FileName;
                if (Row.FileName.Contains("/"))
                {
                    fileNameOnly = Row.FileName.Substring(Row.FileName.LastIndexOf("/") + 1);
                    if (fileNameOnly.Length == 0)
                    {
                        fileNameOnly = Row.FileName;
                    }
                }
                result = string.Format("<a href={0} target='blank'>{1}</a>", Row.FileName, fileNameOnly);
            }
        }
    }

    public partial class FrameworkFileStorage_Data : Cell<FrameworkFileStorage>
    {
        protected override internal void CellIsFileUpload(App app, string gridName, string index, ref bool result)
        {
            result = true;
        }

        protected override internal void CellValueToText(App app, string gridName, string index, ref string result)
        {
            result = "File Upload";
        }
    }
}