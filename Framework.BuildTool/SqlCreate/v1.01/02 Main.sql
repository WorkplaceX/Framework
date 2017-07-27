IF (NOT ISNULL((SELECT Version FROM FrameworkVersion), '') = 'v1.01') RETURN -- Version Check

CREATE TABLE FrameworkApplication
(
	Id INT PRIMARY KEY IDENTITY,
  	Name NVARCHAR(256) NOT NULL UNIQUE,
	Path NVARCHAR(256) NOT NULL UNIQUE, /* Url */
	Type NVARCHAR(256)
)

CREATE TABLE FrameworkSession
(
	Id INT PRIMARY KEY IDENTITY,
  	Name UNIQUEIDENTIFIER NOT NULL UNIQUE,
	ApplicationId INT FOREIGN KEY REFERENCES FrameworkApplication(Id) NOT NULL,
)

CREATE TABLE FrameworkColumn /* Used for configuration. Contains all in source code defined columns. Also calculated fields. */
(
	Id INT PRIMARY KEY IDENTITY,
	TableNameSql NVARCHAR(256),
	FieldNameSql NVARCHAR(256),
	FieldNameCsharp NVARCHAR(256),
	IsExist BIT
)

CREATE TABLE FrameworkFileStorage
(
	Id INT PRIMARY KEY IDENTITY,
	ApplicationId INT FOREIGN KEY REFERENCES FrameworkApplication(Id),
  	Name NVARCHAR(256) NOT NULL, /* File name with path */
  	FileNameUpload NVARCHAR(256),
	Data VARBINARY(MAX),
	IsDelete BIT,
	INDEX IX_FrameworkFileStorage UNIQUE (ApplicationId, Name)
)

INSERT INTO FrameworkApplication (Name, Path)
SELECT 'Framework', 'framework'
