IF ((SELECT Version FROM FrameworkVersion) = 'v0.0')
BEGIN
	CREATE TABLE FrameworkFileStorage
	(
		Id INT PRIMARY KEY IDENTITY,
  		FileName NVARCHAR(256) NOT NULL UNIQUE,
		Data VARBINARY(MAX)
	)
END