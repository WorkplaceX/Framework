IF NOT EXISTS(SELECT * FROM FrameworkVersion WHERE Name = 'Framework' AND Version = 'v1.01') BEGIN SELECT 'RETURN' RETURN END -- Version Check

CREATE TABLE FrameworkApplicationType
(
	Id INT PRIMARY KEY IDENTITY,
  	Name NVARCHAR(256) NOT NULL UNIQUE,
	IsExist BIT
)

CREATE TABLE FrameworkApplication
(
	Id INT PRIMARY KEY IDENTITY,
  	Name NVARCHAR(256) NOT NULL UNIQUE,
	Path NVARCHAR(256) UNIQUE, /* Url */
	ApplicationTypeId INT FOREIGN KEY REFERENCES FrameworkApplicationType(Id) NOT NULL,
	IsActive BIT
)

GO

CREATE VIEW FrameworkApplicationView AS
SELECT
	Application.Id,
	Application.Name,
	Application.Path,
	Application.ApplicationTypeId,
	(SELECT ApplicationType.Name FROM FrameworkApplicationType ApplicationType WHERE ApplicationType.Id = Application.ApplicationTypeId) AS Type,
	(SELECT ApplicationType.IsExist FROM FrameworkApplicationType ApplicationType WHERE ApplicationType.Id = Application.ApplicationTypeId) AS IsExist,
	Application.IsActive

FROM
	FrameworkApplication Application

GO

CREATE TABLE FrameworkSession
(
	Id INT PRIMARY KEY IDENTITY,
  	Name UNIQUEIDENTIFIER NOT NULL UNIQUE,
	ApplicationId INT FOREIGN KEY REFERENCES FrameworkApplication(Id) NOT NULL,
)

CREATE TABLE FrameworkTable /* Used for configuration. Contains all in source code defined tables. */
(
	Id INT PRIMARY KEY IDENTITY,
	Name NVARCHAR(256) NOT NULL UNIQUE,
	IsExist BIT
)

CREATE TABLE FrameworkColumn /* Used for configuration. Contains all in source code defined columns. Also calculated fields. */
(
	Id INT PRIMARY KEY IDENTITY,
	TableId INT FOREIGN KEY REFERENCES FrameworkTable(Id) NOT NULL,
	FieldNameSql NVARCHAR(256),
	FieldNameCsharp NVARCHAR(256),
	IsExist BIT,
	INDEX IX_FrameworkColumn UNIQUE (TableId, FieldNameSql, FieldNameCsharp)
)

CREATE TABLE FrameworkConfigColumn
(
	Id INT PRIMARY KEY IDENTITY,
	ColumnId INT FOREIGN KEY REFERENCES FrameworkColumn(Id) NOT NULL,
	Text NVARCHAR(256),
	IsVisible BIT,
	Sort FLOAT,
	INDEX IX_FrameworkConfigColumn UNIQUE (ColumnId)
)

GO

CREATE VIEW FrameworkConfigColumnView AS
SELECT
	TableX.Id AS TableId,
	TableX.Name AS TableName,
	TableX.IsExist AS TableIsExist,
	ColumnX.Id AS ColumnId,
	ColumnX.FieldNameSql,
	ColumnX.FieldNameCsharp,
	ColumnX.IsExist AS ColumnIsExist,
	Config.Id AS ConfigId,
	Config.Text,
	Config.Sort,
	Config.IsVisible

FROM
	FrameworkColumn ColumnX
	
LEFT JOIN
	FrameworkTable TableX ON TableX.Id = ColumnX.TableId

LEFT JOIN
	FrameworkConfigColumn Config ON Config.ColumnId = ColumnX.Id

GO

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