﻿CREATE TABLE FrameworkApplicationType
(
	Id INT PRIMARY KEY IDENTITY,
  	Name NVARCHAR(256) NOT NULL UNIQUE,
	IsExist BIT NOT NULL
)

CREATE TABLE FrameworkApplication
(
	Id INT PRIMARY KEY IDENTITY,
  	Text NVARCHAR(256),
	Path NVARCHAR(256) UNIQUE, /* Url */
	ApplicationTypeId INT FOREIGN KEY REFERENCES FrameworkApplicationType(Id) NOT NULL,
	IsActive BIT
)

GO

CREATE VIEW FrameworkApplicationView AS
SELECT
	Application.Id,
	Application.Text,
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
	Name NVARCHAR(256) NOT NULL UNIQUE, -- See also method UtilDataAccessLayer.TypeRowToNameCSharp(); This is TableNameCSharp
	IsExist BIT NOT NULL
)

CREATE TABLE FrameworkColumn /* Used for configuration. Contains all in source code defined columns. Also calculated fields. */
(
	Id INT PRIMARY KEY IDENTITY,
	TableId INT FOREIGN KEY REFERENCES FrameworkTable(Id) NOT NULL,
	Name NVARCHAR(256) NOT NULL, -- This is FieldNameCSharp
	IsExist BIT NOT NULL
	INDEX IX_FrameworkConfigTable UNIQUE (TableId, Name)
)

CREATE TABLE FrameworkGrid /* Used for configuration. Contains all in source code defined grids. (Static GridName) */
(
	Id INT PRIMARY KEY IDENTITY,
	TableId INT FOREIGN KEY REFERENCES FrameworkTable(Id) NOT NULL,
	Name NVARCHAR(256), -- GridName
	IsExist BIT NOT NULL
	INDEX IX_FrameworkGrid UNIQUE (TableId, Name) -- For example new GridName<Table>("Master");
)

CREATE TABLE FrameworkConfigTable
(
	Id INT PRIMARY KEY IDENTITY,
	TableId INT FOREIGN KEY REFERENCES FrameworkTable(Id) NOT NULL,
	PageRowCount INT, /* Number of records to load on one page */
	IsInsert BIT, /* Allow insert record */
	INDEX IX_FrameworkConfigTable UNIQUE (TableId)
)

CREATE TABLE FrameworkConfigColumn
(
	Id INT PRIMARY KEY IDENTITY,
	ColumnId INT FOREIGN KEY REFERENCES FrameworkColumn(Id) NOT NULL,
	Text NVARCHAR(256),
	IsVisible BIT,
	Sort FLOAT,
	WidthPercent FLOAT,
	INDEX IX_FrameworkConfigColumn UNIQUE (ColumnId)
)

GO

CREATE VIEW FrameworkConfigTableView AS
SELECT
	TableX.Id AS TableId,
	TableX.Name AS TableName,
	TableX.IsExist AS TableIsExist,
	Config.Id AS ConfigId,
	Config.PageRowCount,
	Config.IsInsert

FROM
	FrameworkTable TableX
	
LEFT JOIN
	FrameworkConfigTable Config ON Config.TableId = TableX.Id

GO

CREATE VIEW FrameworkConfigColumnView AS
SELECT
	TableX.Id AS TableId,
	TableX.Name AS TableName, -- TableNameCSharp
	TableX.IsExist AS TableIsExist,
	ColumnX.Id AS ColumnId,
	ColumnX.Name AS ColumnName, -- ColumnNameCSharp
	ColumnX.IsExist AS ColumnIsExist,
	Config.Id AS ConfigId,
	Config.Text,
	Config.IsVisible,
	Config.Sort,
	Config.WidthPercent

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