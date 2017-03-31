CREATE TABLE 
	TempName
(
	[Id] INT PRIMARY KEY IDENTITY (1, 1),
	[Name] NVARCHAR(256) UNIQUE NOT NULL -- See also property ConnectionManager.ExcelVaueTextLengthMax
)
