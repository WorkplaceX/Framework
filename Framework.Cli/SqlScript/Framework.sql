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

CREATE TABLE FrameworkField /* Used for configuration. Contains all in source code defined columns. Also calculated fields. */
(
	Id INT PRIMARY KEY IDENTITY,
	TableId INT FOREIGN KEY REFERENCES FrameworkTable(Id) NOT NULL ,
	FieldNameCSharp NVARCHAR(256) NOT NULL,
	FieldNameSql NVARCHAR(256), -- Can be null for calculated columns.
	IsExist BIT NOT NULL
	INDEX IX_FrameworkField UNIQUE (TableId, FieldNameCSharp)
)

GO
CREATE VIEW FrameworkFieldBuiltIn
AS
SELECT
	Id,
	CONCAT(
		'Table=',
		(SELECT FrameworkTable.TableNameCSharp FROM FrameworkTable WHERE FrameworkTable.Id = FrameworkField.TableId), '; ',
		'Field=',
		FrameworkField.FieldNameCSharp, ';') AS Name
FROM
	FrameworkField FrameworkField
GO

CREATE TABLE FrameworkConfigGrid
(
	Id INT PRIMARY KEY IDENTITY,
	TableId INT FOREIGN KEY REFERENCES FrameworkTable(Id) NOT NULL ,
	ConfigName NVARCHAR(256) NOT NULL,
	RowCountMax INT,
	IsAllowInsert BIT,
	IsExist BIT NOT NULL
	INDEX IX_FrameworkConfigGrid UNIQUE (TableId, ConfigName)
)

GO
CREATE VIEW FrameworkConfigGridBuiltIn
AS
	SELECT FrameworkConfigGrid.Id,
	CONCAT((SELECT FrameworkTable.TableNameCSharp FROM FrameworkTable WHERE FrameworkTable.Id = FrameworkConfigGrid.TableId), '; ', FrameworkConfigGrid.ConfigName) AS Name
FROM
	FrameworkConfigGrid FrameworkConfigGrid
GO

CREATE TABLE FrameworkConfigField
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
	INDEX IX_FrameworkConfigField UNIQUE (ConfigName, TypeName, FieldName)
)
