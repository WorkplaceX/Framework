namespace Office
{
    using DocumentFormat.OpenXml;
    using DocumentFormat.OpenXml.Packaging;
    using DocumentFormat.OpenXml.Spreadsheet;
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlClient;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// Read Microsoft Office Excel files (*.xlsx) and (*.xlsm).
    /// </summary>
    internal class ExcelRead
    {
        /// <summary>
        /// (FileName, SheetName, RowName, ColumnName).
        /// </summary>
        private Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, ExcelCell>>>> cellList = new Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, ExcelCell>>>>();

        /// <summary>
        /// Gets CellList. Used by ExcelDb.
        /// </summary>
        internal Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, ExcelCell>>>> CellList
        {
            get
            {
                return cellList;
            }
        }

        private ExcelCell CellGet(string fileName, string sheetName, string rowName, string columnName)
        {
            if (!cellList[fileName][sheetName].ContainsKey(rowName))
            {
                cellList[fileName][sheetName].Add(rowName, new Dictionary<string, ExcelCell>());
            }
            //
            if (!cellList[fileName][sheetName][rowName].ContainsKey(columnName))
            {
                cellList[fileName][sheetName][rowName].Add(columnName, null);
            }
            //
            return cellList[fileName][sheetName][rowName][columnName];
        }

        private void CellSet(string fileName, string sheetName, string rowName, string columnName, ExcelCell value)
        {
            CellGet(fileName, sheetName, rowName, columnName);
            cellList[fileName][sheetName][rowName][columnName] = value;
        }

        private void FileNameAdd(string fileName)
        {
            cellList.Add(fileName, new Dictionary<string, Dictionary<string, Dictionary<string, ExcelCell>>>());
        }

        private void SheetNameAdd(string fileName, string sheetName)
        {
            cellList[fileName].Add(sheetName, new Dictionary<string, Dictionary<string, ExcelCell>>());
        }

        /// <summary>
        /// Load file and add it to Excel class.
        /// </summary>
        public void Load(string fileName)
        {
            FileNameAdd(fileName);
            //
            SpreadsheetDocument document = SpreadsheetDocument.Open(fileName, false);
            //
            Dictionary<string, string> sheetIdToName = new Dictionary<string, string>();
            foreach (Sheet sheet in document.WorkbookPart.Workbook.Sheets)
            {
                sheetIdToName.Add(sheet.Id, sheet.Name);
                SheetNameAdd(fileName, sheet.Name);
            }
            //
            foreach (WorksheetPart worksheetPart in document.WorkbookPart.WorksheetParts)
            {
                string sheetId = document.WorkbookPart.GetIdOfPart(worksheetPart);
                string sheetName = sheetIdToName[sheetId];
                SharedStringItem[] sharedStringList = new SharedStringItem[0];
                if (document.WorkbookPart.SharedStringTablePart != null)
                {
                    sharedStringList = document.WorkbookPart.SharedStringTablePart.SharedStringTable.Elements<SharedStringItem>().ToArray();
                }
                //
                OpenXmlReader reader = OpenXmlReader.Create(worksheetPart);
                while (reader.Read())
                {
                    if (reader.ElementType == typeof(Row))
                    {
                        Row row = (Row)reader.LoadCurrentElement();
                        string rowName = row.RowIndex;
                        foreach (Cell cell in row.Elements<Cell>().ToArray())
                        {
                            string text = null;
                            bool isText = false;
                            bool isError = false;
                            string columnName = cell.CellReference.Value.Substring(0, cell.CellReference.Value.Length - rowName.Length);
                            if (cell.DataType != null)
                            {
                                switch (cell.DataType.Value)
                                {
                                    case CellValues.Boolean:
                                        text = cell.CellValue.Text;
                                        break;
                                    case CellValues.SharedString:
                                        isText = true;
                                        int index = int.Parse(cell.CellValue.InnerText);
                                        SharedStringItem item = sharedStringList[index];
                                        if (item.Text != null)
                                        {
                                            text = item.Text.Text;
                                        }
                                        else
                                        {
                                            text = item.InnerText; // For cell text with formating. (Formating is removed when read like this).
                                        }
                                        break;
                                    case CellValues.String:
                                        isText = true;
                                        text = cell.CellValue.Text;
                                        break;
                                    case CellValues.Error:
                                        isError = true;
                                        text = cell.CellValue.Text;
                                        break;
                                    default:
                                        throw new Exception("Type unknown!");
                                }
                            }
                            else
                            {
                                if (cell.CellValue != null)
                                {
                                    text = cell.CellValue.Text; // Number.
                                }
                            }
                            //
                            if (text != null)
                            {
                                text = text.Trim();
                            }
                            if (!string.IsNullOrEmpty(text))
                            {
                                CellSet(fileName, sheetName, rowName, columnName, new ExcelCell(text, isText, isError));
                            }
                        }
                    }
                }
                reader.Close();
            }
            document.Close();
        }

        internal class ExcelCell
        {
            public ExcelCell(string text, bool isText, bool isError)
            {
                if (isText == false)
                {
                    if (!isError)
                    {
                        this.ValueNumber = double.Parse(text);
                    }
                    this.ValueText = text;
                }
                else
                {
                    double number;
                    if (double.TryParse(text, out number))
                    {
                        this.ValueNumber = number;
                    }
                    this.ValueText = text;
                }
                //
                this.IsText = isText;
                this.IsError = isError;
            }

            public readonly double? ValueNumber;

            public readonly string ValueText;

            public readonly bool IsText;

            public readonly bool IsError;
        }
    }

    /// <summary>
    /// Copy object ExcelRead data to database.
    /// </summary>
    internal class Excel
    {
        /// <summary>
        /// Create temp tables and views.
        /// </summary>
        public static void SqlCreate()
        {
            string sql = ConnectionManager.SqlResource("ExcelCreate.sql");
            sql = Util.Replace(sql, "[ValueText] NVARCHAR(512)", string.Format("[ValueText] NVARCHAR({0})", ConnectionManager.ExcelVaueTextLengthMax));
            SqlUtil.Execute(sql);
        }

        /// <summary>
        /// Drop temp tables and views.
        /// </summary>
        public static void SqlDrop()
        {
            string sql = ConnectionManager.SqlResource("ExcelDrop.sql");
            SqlUtil.Execute(sql);
        }

        /// <summary>
        /// Disable index for bulk insert. (Performance).
        /// </summary>
        public static void SqlInsertBegin()
        {
            ConnectionManager.OnLog("Excel SqlInsertBegin.");
            string sql = ConnectionManager.SqlResource("ExcelInsertBegin.sql");
            SqlUtil.Execute(sql);
        }

        /// <summary>
        /// Enable index after bulk insert. (Performance).
        /// </summary>
        public static void SqlInsertEnd()
        {
            ConnectionManager.OnLog("Excel SqlInsertEnd.");
            string sql = ConnectionManager.SqlResource("ExcelInsertEnd.sql");
            SqlUtil.Execute(sql);
        }
        /// <summary>
        /// Add name from ExcelRead.
        /// </summary>
        private void NameListAdd(ExcelRead excelRead)
        {
            foreach (string fileName in excelRead.CellList.Keys)
            {
                Name.Add(fileName);
                foreach (string sheetName in excelRead.CellList[fileName].Keys)
                {
                    Name.Add(sheetName);
                    foreach (string rowName in excelRead.CellList[fileName][sheetName].Keys)
                    {
                        foreach (string columnName in excelRead.CellList[fileName][sheetName][rowName].Keys)
                        {
                            Name.Add(columnName);
                        }
                    }
                }
            }
        }

        private void CellListInsert(ExcelRead excelRead)
        {
            SqlBulkCopy sqlBulkCopy = new SqlBulkCopy(ConnectionManager.Connection, SqlBulkCopyOptions.TableLock | SqlBulkCopyOptions.CheckConstraints, null); // TableLock because of Azure.
            sqlBulkCopy.DestinationTableName = string.Format("{0}Excel", ConnectionManager.Prefix);
            //
            DataTable dataTable = new DataTable();
            dataTable.Columns.Add("Id", typeof(int));
            dataTable.Columns.Add("FieldNameId", typeof(int));
            dataTable.Columns.Add("SheetNameId", typeof(int));
            dataTable.Columns.Add("RowName", typeof(int));
            dataTable.Columns.Add("ColumnNameId", typeof(int));
            dataTable.Columns.Add("ValueNumber", typeof(double));
            dataTable.Columns.Add("ValueText", typeof(string));
            //
            int count = 0;
            foreach (string fileName in excelRead.CellList.Keys)
            {
                int fileNameId = Name.Id(fileName);
                foreach (string sheetName in excelRead.CellList[fileName].Keys)
                {
                    int sheetNameId = Name.Id(sheetName);
                    foreach (string rowName in excelRead.CellList[fileName][sheetName].Keys)
                    {
                        foreach (string columnName in excelRead.CellList[fileName][sheetName][rowName].Keys)
                        {
                            ExcelRead.ExcelCell cell = excelRead.CellList[fileName][sheetName][rowName][columnName];
                            int columnNameId = Name.Id(columnName);
                            //
                            string valueText = cell.ValueText;
                            if (valueText != null && valueText.Length > ConnectionManager.ExcelVaueTextLengthMax)
                            {
                                // log("Warning! Cell truncate.");
                                valueText = valueText.Substring(0, ConnectionManager.ExcelVaueTextLengthMax);
                            }
                            dataTable.Rows.Add(0, fileNameId, sheetNameId, int.Parse(rowName), columnNameId, cell.ValueNumber, valueText);
                            // Flush
                            count += 1;
                            if (count == 100000)
                            {
                                count = 0;
                                sqlBulkCopy.WriteToServer(dataTable);
                                dataTable.Rows.Clear();
                            }
                        }
                    }
                }
            }
            //
            sqlBulkCopy.WriteToServer(dataTable);
        }

        public bool IsBulkCopy = true;

        /// <summary>
        /// Copy data from ExcelRead object to database.
        /// </summary>
        public void Run(ExcelRead excelRead)
        {
            NameListAdd(excelRead); // Add name from ExcelRead.
            Name.Insert(); // Insert name into database.
            CellListInsert(excelRead); // Insert cells into database.
        }
    }

    /// <summary>
    /// Maintains sql table with name list.
    /// </summary>
    internal static class Name
    {
        /// <summary>
        /// (Name, Id). Id as it is on database.
        /// </summary>
        private static Dictionary<string, int?> nameList = new Dictionary<string, int?>();

        /// <summary>
        /// Select NameList from database.
        /// </summary>
        public static void Select()
        {
            nameList.Clear();
            //
            string sql = ConnectionManager.SqlResource("NameSelect.sql");
            SqlDataReader reader = SqlUtil.Select(sql);
            try
            {
                while (reader.Read())
                {
                    int id = (int)reader["Id"];
                    string name = (string)reader["Name"];
                    //
                    nameList.Add(name, id);
                }
            }
            finally
            {
                reader.Close();
            }
        }

        /// <summary>
        /// Throws exception if item is not yet stored on database.
        /// </summary>
        public static int Id(string name)
        {
            return (int)nameList[name];
        }

        /// <summary>
        /// Insert missing names into database.
        /// </summary>
        public static void Insert()
        {
            string sqlWrapper = ConnectionManager.SqlResource("NameInsert.sql");
            SqlUtil sqlUtil = new SqlUtil(sqlWrapper);
            bool isFirst = true;
            foreach (string name in nameList.Keys)
            {
                if (isFirst)
                {
                    isFirst = false;
                }
                else
                {
                    sqlUtil.Add("UNION ALL");
                }
                sqlUtil.Add("SELECT @P0", new SqlUtil.Parameter<string>(name));
            }
            sqlUtil.Execute();
            //
            Select();
        }

        public static void Add(string name)
        {
            if (!nameList.ContainsKey(name))
            {
                nameList.Add(name, null); // Null because it is not yet in database.
            }
        }

        /// <summary>
        /// Create table on database.
        /// </summary>
        public static void SqlCreate()
        {
            string sql = ConnectionManager.SqlResource("NameCreate.sql");
            SqlUtil.Execute(sql);
        }

        /// <summary>
        /// Drop table on database.
        /// </summary>
        public static void SqlDrop()
        {
            string sql = ConnectionManager.SqlResource("NameDrop.sql");
            SqlUtil.Execute(sql);
        }
    }

    /// <summary>
    /// Main script loads excel files into database.
    /// </summary>
    internal static class Script
    {
        public static void Run()
        {
            ConnectionManager.OnLog("Begin");
            ConnectionManager.OnLog("SqlCreate");
            SqlDrop();
            SqlCreate();
            //
            // Get Local FileNameList.
            List<string> fileNameList = new List<string>();
            foreach (string fileName in Util.FileNameList())
            {
                if (fileName.EndsWith(".xlsx"))
                {
                    fileNameList.Add(fileName);
                }
            }
            // Copy local Excel files into database.
            Excel.SqlInsertBegin();
            try
            {
                foreach (string fileName in fileNameList)
                {
                    ExcelRead excelRead = new ExcelRead();
                    ConnectionManager.OnLog(string.Format("Load local. ({0})", System.IO.Path.GetFileName(fileName)));
                    excelRead.Load(fileName);
                    ConnectionManager.OnLog(string.Format("Copy local to database. ({0})", System.IO.Path.GetFileName(fileName)));
                    Excel excel = new Excel();
                    excel.Run(excelRead);
                }
            }
            finally
            {
                Excel.SqlInsertEnd();
            }

            ConnectionManager.OnLog("End");
        }

        public static void SqlCreate()
        {
            Name.SqlCreate();
            Excel.SqlCreate();
        }

        public static void SqlDrop()
        {
            Excel.SqlDrop();
            Name.SqlDrop();
        }
    }

    /// <summary>
    /// Create instance of SqlUtil if statement contains parameters.
    /// </summary>
    internal class SqlUtil
    {
        public SqlUtil(string sqlWrapper)
        {
            this.SqlWrapper = sqlWrapper;
        }

        public SqlUtil()
            : this(null)
        {

        }

        /// <summary>
        /// Gets SqlMain of the format "SELECT * FROM A EXCEPT {0}". Zero will be replaced with added sql.
        /// </summary>
        public readonly string SqlWrapper;

        private readonly StringBuilder sqlList = new StringBuilder();

        private readonly List<Parameter> parameterList = new List<Parameter>();

        private int parameterIndex = 0;

        /// <summary>
        /// Gets number of added parameters.
        /// </summary>
        public int ParameterIndex
        {
            get
            {
                return parameterIndex;
            }
        }

        public void Add(string sql, params Parameter[] parameterList)
        {
            for (int i = 0; i < parameterList.Length; i++)
            {
                sql = sql + " "; // Used to match whole word.
                sql = sql.Replace(string.Format("@P{0}\r\n ", i), string.Format("@P{0} ", i + parameterIndex));
                sql = sql.Replace(string.Format("@P{0} ", i), string.Format("@P{0} ", i + parameterIndex));
                sql = sql.Replace(string.Format("@P{0},", i), string.Format("@P{0},", i + parameterIndex));
                this.parameterList.Add(parameterList[i]);
            }
            parameterIndex += parameterList.Length;
            //
            sqlList.AppendLine(sql);
        }

        private SqlDataReader Execute(bool isNonQuery) // TODO Bulk insert.
        {
            string sql = sqlList.ToString();
            if (SqlWrapper != null)
            {
                sql = string.Format(SqlWrapper, sql);
            }
            SqlCommand command = new SqlCommand(sql, ConnectionManager.Connection);
            for (int i = 0; i < parameterList.Count; i++)
            {
                object value;
                SqlDbType valueType;
                parameterList[i].ValueDb(out value, out valueType);
                command.Parameters.Add(string.Format("@P{0}", i), valueType).Value = value;
            }
            //
            SqlDataReader result = null;
            if (isNonQuery == false)
            {
                result = command.ExecuteReader();
            }
            else
            {
                command.ExecuteNonQuery();
            }
            //
            parameterList.Clear();
            sqlList.Clear();
            parameterIndex = 0;
            //
            return result;
        }

        public void Execute()
        {
            Execute(true);
        }

        public SqlDataReader Select()
        {
            return Execute(false);
        }

        /// <summary>
        /// Execute sql directly.
        /// </summary>
        private static void Execute(string sql, int? commandTimeout, bool isLog)
        {
            foreach (string item in sql.Split(new string[] { "GO" }, StringSplitOptions.RemoveEmptyEntries))
            {
                SqlCommand command = new SqlCommand(item, ConnectionManager.Connection);
                if (commandTimeout != null)
                {
                    command.CommandTimeout = commandTimeout.Value;
                }
                int rowsAffected = command.ExecuteNonQuery();
                if (isLog)
                {
                    if (item.ToLower().Contains("insert into"))
                    {
                        ConnectionManager.OnLog(string.Format("-RowsAffected({0})", rowsAffected));
                    }
                }
            }
        }

        public static void Execute(string sql)
        {
            Execute(sql, null, false);
        }

        public static void Execute(string sql, bool isLog)
        {
            Execute(sql, null, isLog);
        }

        public static void Execute(string sql, int? commandTimeout)
        {
            Execute(sql, commandTimeout, true);
        }

        /// <summary>
        /// Select sql directly.
        /// </summary>
        public static SqlDataReader Select(string sql)
        {
            SqlCommand command = new SqlCommand(sql, ConnectionManager.Connection);
            SqlDataReader result = command.ExecuteReader();
            //
            return result;
        }

        public static T Read<T>(SqlDataReader reader, string fieldName)
        {
            object result = reader[fieldName];
            if (result == DBNull.Value)
            {
                result = null;
            }
            return (T)(object)result;
        }

        internal class Parameter
        {
            protected Parameter(object value, Type valueType)
            {
                this.Value = value;
                this.ValueType = valueType;
            }

            public readonly object Value;

            public readonly Type ValueType;

            public static int Max;

            public void ValueDb(out object value, out SqlDbType valueType)
            {
                value = Value;
                if (value == null)
                {
                    value = DBNull.Value;
                }
                if (ValueType == typeof(int) || ValueType == typeof(int?))
                {
                    valueType = SqlDbType.Int;
                }
                else
                {
                    if (ValueType == typeof(string))
                    {
                        valueType = SqlDbType.NVarChar;
                        if (value.ToString().Length > Max)
                        {
                            Max = value.ToString().Length;
                        }
                    }
                    else
                    {
                        if (ValueType == typeof(double) || ValueType == typeof(double?))
                        {
                            valueType = SqlDbType.Float;
                        }
                        else
                        {
                            if (ValueType == typeof(bool) || ValueType == typeof(bool?))
                            {
                                valueType = SqlDbType.Bit;
                            }
                            else
                            {
                                throw new Exception("Type unknown!");
                            }
                        }
                    }
                }
            }
        }

        internal class Parameter<T> : Parameter
        {
            public Parameter(T value)
                : base(value, typeof(T))
            {

            }

            public new T Value
            {
                get
                {
                    return (T)base.Value;
                }
            }
        }

        public static string BoolToSql(bool? value)
        {
            string result = "NULL";
            if (value == true)
            {
                result = "1";
            }
            else
            {
                result = "0";
            }
            return result;
        }
    }
}
