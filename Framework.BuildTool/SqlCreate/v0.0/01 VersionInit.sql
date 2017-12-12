IF NOT EXISTS(SELECT * FROM sys.tables WHERE name = 'FrameworkVersion')
CREATE TABLE FrameworkVersion
(
	Id INT PRIMARY KEY IDENTITY,
	Name NVARCHAR(256) NOT NULL UNIQUE, -- Module name like (Framework, Application)
	Version NVARCHAR(8) NOT NULL
)

IF NOT EXISTS (SELECT * FROM FrameworkVersion WHERE Name = 'Framework')
INSERT INTO FrameworkVersion (Name, Version)
SELECT 'Framework', 'v0.0'
