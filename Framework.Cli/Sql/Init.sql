IF NOT EXISTS(SELECT * FROM sys.tables WHERE name = 'FrameworkScript')
CREATE TABLE FrameworkScript
(
	Id INT PRIMARY KEY IDENTITY,
	FileName NVARCHAR(256) NOT NULL UNIQUE, -- For example "Framework/Framework.Cli/SqlScript/Config.sql"
	Date DATETIME2, -- Date and time when script run
)