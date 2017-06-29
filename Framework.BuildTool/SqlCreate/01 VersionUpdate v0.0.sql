IF NOT EXISTS(SELECT * FROM sys.tables WHERE name = 'FrameworkVersion')
BEGIN
	CREATE TABLE FrameworkVersion
	(
		Id INT PRIMARY KEY IDENTITY,
		Version NVARCHAR(8) NOT NULL
	)
	INSERT INTO FrameworkVersion(Version)
	SELECT 'v0.0'
END
