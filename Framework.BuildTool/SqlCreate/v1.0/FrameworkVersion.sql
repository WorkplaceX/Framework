IF EXISTS(SELECT * FROM sys.tables WHERE name = 'FrameworkVersion') RETURN

CREATE TABLE FrameworkVersion
(
	Id INT PRIMARY KEY IDENTITY,
	Version NVARCHAR(8) NOT NULL
)
INSERT INTO FrameworkVersion(Version)
SELECT 'v1.0'
