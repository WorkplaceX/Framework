IF NOT EXISTS(SELECT * FROM sys.tables WHERE name = 'FrameworkDeployDb')
CREATE TABLE FrameworkDeployDb /* Contains list of all executed sql script files in "DeployDb/" folders. */
(
	Id INT PRIMARY KEY IDENTITY,
	FileName NVARCHAR(256) NOT NULL UNIQUE, -- For example "Framework/Framework.Cli/DeployDb/Config.sql"
	Date DATETIME2, -- Date and time when script run
)