CREATE TABLE 
	TempExcel
(
	[Id] INT PRIMARY KEY IDENTITY (1, 1) NOT NULL,
    [FileNameId] INT FOREIGN KEY REFERENCES TempName(Id),
	[SheetNameId] INT FOREIGN KEY REFERENCES TempName(Id),
	[Row] INT,
	[ColumnNameId] INT FOREIGN KEY REFERENCES TempName(Id),
	[ValueNumber] FLOAT,
	[ValueText] NVARCHAR(512)
)
CREATE UNIQUE INDEX IX_TempExcelCell ON TempExcel (FileNameId, SheetNameId, Row, ColumnNameId)
CREATE INDEX IX_TempExcelRow ON TempExcel (FileNameId, SheetNameId, Row)
GO

CREATE VIEW TempExcelDisplay AS
SELECT
	Excel.Id AS ExcelId,
	Excel.FileNameId AS FileNameId,
	FileName.Name AS FileName,
	Excel.SheetNameId AS SheetNameId,
	SheetName.Name AS SheetName,
	Excel.Row AS Row,
	Excel.ColumnNameId AS ColumnNameId,
	ColumnName.Name AS ColumnName,
	Excel.ValueNumber AS ValueNumber,
	Excel.ValueText AS ValueText

FROM
	TempExcel Excel
	
LEFT JOIN
	TempName FileName ON (FileName.Id = Excel.FileNameId)
	
LEFT JOIN
	TempName SheetName ON (SheetName.Id = Excel.SheetNameId)
	
LEFT JOIN
	TempName ColumnName ON (ColumnName.Id = Excel.ColumnNameId)