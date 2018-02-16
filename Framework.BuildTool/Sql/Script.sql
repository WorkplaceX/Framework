IF NOT EXISTS(SELECT * FROM sys.tables WHERE name = 'FrameworkScript')
CREATE TABLE FrameworkScript
(
	Id INT PRIMARY KEY IDENTITY,
	Name NVARCHAR(256) NOT NULL UNIQUE, -- Script name, file name
	IsExist BIT NOT NULL,
	IsRun BIT NOT NULL, -- Corresponding drop script will set it back to 0
	DateCreate DATETIME2, -- Date and time when create script run
	DateDrop DATETIME2 -- Date and time when drop script run.
)