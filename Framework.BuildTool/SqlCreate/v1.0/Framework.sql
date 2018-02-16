CREATE TABLE FrameworkApplicationType
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
	TableNameCSharp NVARCHAR(256) NOT NULL UNIQUE, -- See also method UtilDataAccessLayer.TypeRowToNameCSharp();
	TableNameSql NVARCHAR(256), -- Can be null for memory rows.
	IsExist BIT NOT NULL
)

CREATE TABLE FrameworkColumn /* Used for configuration. Contains all in source code defined columns. Also calculated fields. */
(
	Id INT PRIMARY KEY IDENTITY,
	TableId INT FOREIGN KEY REFERENCES FrameworkTable(Id) NOT NULL ,
	ColumnNameCSharp NVARCHAR(256) NOT NULL,
	ColumnNameSql NVARCHAR(256), -- Can be null for calculated columns.
	IsExist BIT NOT NULL
	INDEX IX_FrameworkColumn UNIQUE (TableId, ColumnNameCSharp)
)

CREATE TABLE FrameworkGrid /* Used for configuration. Contains all in source code defined grids. (Static GridName) */
(
	Id INT PRIMARY KEY IDENTITY,
	TableId INT FOREIGN KEY REFERENCES FrameworkTable(Id) NOT NULL,
	GridName NVARCHAR(256),
	IsExist BIT NOT NULL
	INDEX IX_FrameworkGrid UNIQUE (TableId, GridName) -- For example new GridName<Table>("Master");
)

CREATE TABLE FrameworkConfigGrid
(
	Id INT PRIMARY KEY IDENTITY,
	GridId INT FOREIGN KEY REFERENCES FrameworkGrid(Id) NOT NULL UNIQUE,
	PageRowCountDefault INT, /* Defined in CSharp code (Attribute). */
	PageRowCount INT, /* Number of records to load on one page */
	PageColumnCountDefault INT, /* Defined in CSharp code (Attribute). */
	PageColumnCount INT, /* Number of columns to load (transfer to client) for one page */
	IsInsertDefault BIT, /* Defined in CSharp code (Attribute). */
	IsInsert BIT, /* Allow insert record */
)

CREATE TABLE FrameworkConfigColumn
(
	Id INT PRIMARY KEY IDENTITY,
	GridId INT FOREIGN KEY REFERENCES FrameworkGrid(Id) NOT NULL,
	ColumnId INT FOREIGN KEY REFERENCES FrameworkColumn(Id) NOT NULL,
	TextDefault NVARCHAR(256), -- Column header text.
	Text NVARCHAR(256), -- Column header text.
	DescriptionDefault NVARCHAR(256), -- Column header text.
	Description NVARCHAR(256), -- Column header text.
	IsVisibleDefault BIT,
	IsVisible BIT,
	IsReadOnlyDefault BIT,
	IsReadOnly BIT,
	SortDefault FLOAT,
	Sort FLOAT,
	WidthPercentDefault FLOAT,
	WidthPercent FLOAT,
	INDEX IX_FrameworkConfigColumn UNIQUE (GridId, ColumnId)
)

GO

CREATE VIEW FrameworkConfigGridView AS
SELECT
	Grid.Id AS GridId,
	Grid.GridName,
	Grid.IsExist AS GridIsExist,
	TableX.Id AS TableId,
	TableX.TableNameCSharp,
	TableX.TableNameSql,
	TableX.IsExist AS TableIsExist,
	Config.Id AS ConfigId,
	Config.PageRowCountDefault,
	Config.PageRowCount,
	Config.PageColumnCountDefault,
	Config.PageColumnCount,
	Config.IsInsertDefault,
	Config.IsInsert

FROM
	FrameworkGrid Grid

LEFT JOIN
	FrameworkTable TableX ON TableX.Id = Grid.TableId
	
LEFT JOIN
	FrameworkConfigGrid Config ON Config.GridId = Grid.Id

GO

CREATE VIEW FrameworkConfigColumnView AS
SELECT
	Grid.Id AS GridId,
	Grid.GridName,
	Grid.IsExist AS GridIsExist,
	TableX.Id AS TableId,
	TableX.TableNameCSharp,
	TableX.TableNameSql,
	TableX.IsExist AS TableIsExist,
	ColumnX.Id AS ColumnId,
	ColumnX.ColumnNameCSharp,
	ColumnX.ColumnNameSql,
	ColumnX.IsExist AS ColumnIsExist,
	Config.Id AS ConfigId,
	Config.TextDefault,
	Config.Text,
	Config.DescriptionDefault,
	Config.Description,
	Config.IsVisibleDefault,
	Config.IsVisible,
	Config.IsReadOnlyDefault,
	Config.IsReadOnly,
	Config.SortDefault,
	Config.Sort,
	Config.WidthPercentDefault,
	Config.WidthPercent

FROM
	FrameworkGrid Grid

LEFT JOIN
	FrameworkTable TableX ON TableX.Id = Grid.TableId

LEFT JOIN
	FrameworkColumn ColumnX ON ColumnX.TableId = Grid.TableId

LEFT JOIN
	FrameworkConfigColumn Config ON Config.GridId = Grid.Id AND Config.ColumnId = ColumnX.Id

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

GO

CREATE TABLE FrameworkPage
(
	Id INT PRIMARY KEY IDENTITY,
	PageNameCSharp NVARCHAR(256),
	IsExist BIT NOT NULL
)