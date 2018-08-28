CREATE TABLE FrameworkTable /* Used for configuration. Contains all in source code defined tables. */
(
	Id INT PRIMARY KEY IDENTITY,
	TableNameCSharp NVARCHAR(256) NOT NULL UNIQUE, -- See also method UtilDataAccessLayer.TypeRowToNameCSharp();
	TableNameSql NVARCHAR(256), -- Can be null for memory rows.
	IsExist BIT NOT NULL
)

GO
CREATE VIEW FrameworkTableBuiltIn AS
SELECT
	Id,
	TableNameCSharp AS Name
FROM
	FrameworkTable
GO

CREATE TABLE FrameworkColumn /* Used for configuration. Contains all in source code defined columns. Also calculated fields. */
(
	Id INT PRIMARY KEY IDENTITY,
	TableId INT FOREIGN KEY REFERENCES FrameworkTable(Id) NOT NULL ,
	ColumnNameCSharp NVARCHAR(256) NOT NULL,
	ColumnNameSql NVARCHAR(256), -- Can be null for calculated columns.
	IsExist BIT NOT NULL
	INDEX IX_FrameworkColumn UNIQUE (TableId, ColumnNameCSharp)
)

GO
CREATE VIEW FrameworkColumnBuiltIn AS
SELECT
	Id,
	CONCAT(
		'Table=',
		(SELECT FrameworkTable.TableNameCSharp FROM FrameworkTable WHERE FrameworkTable.Id = FrameworkColumn.TableId), '; ',
		'Column=',
		FrameworkColumn.ColumnNameCSharp, ';') AS Name
FROM
	FrameworkColumn FrameworkColumn
GO

CREATE TABLE FrameworkConfigGrid
(
	Id INT PRIMARY KEY IDENTITY,
	IsBuiltIn BIT NOT NULL,
	ConfigName NVARCHAR(256) NOT NULL,
	TypeName NVARCHAR(256),
	RowCountMax INT,
	IsInsert BIT,
	INDEX IX_FrameworkConfigGrid UNIQUE (ConfigName, TypeName)
)

CREATE TABLE FrameworkConfigColumn
(
	Id INT PRIMARY KEY IDENTITY,
	IsBuiltIn BIT NOT NULL,
	ConfigName NVARCHAR(256) NOT NULL,
	TypeName NVARCHAR(256) NOT NULL,
	FieldName NVARCHAR(256) NOT NULL,
	Text NVARCHAR(256), -- Column header text.
	Description NVARCHAR(256), -- Column header description.
	IsVisible BIT NOT NULL,
	IsReadOnly BIT NOT NULL,
	Sort FLOAT,
	INDEX IX_FrameworkConfigColumn UNIQUE (ConfigName, TypeName, FieldName)
)
