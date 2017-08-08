IF NOT EXISTS(SELECT * FROM sys.tables WHERE name = 'FrameworkVersion')
CREATE TABLE FrameworkVersion
(
	Id INT PRIMARY KEY IDENTITY,
	Name NVARCHAR(256) NOT NULL UNIQUE, -- Module name
	Version NVARCHAR(8) NOT NULL
)

IF EXISTS(SELECT * FROM FrameworkVersion WHERE Name = 'Framework') BEGIN SELECT 'RETURN' RETURN END -- Version Check
INSERT INTO FrameworkVersion(Name, Version)
SELECT 'Framework', 'v1.0'
