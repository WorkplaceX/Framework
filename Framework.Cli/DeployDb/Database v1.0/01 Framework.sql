CREATE TABLE FrameworkTable /* Used for configuration. Contains all in source code defined tables. */
(
    Id INT PRIMARY KEY IDENTITY,
    TableNameCSharp NVARCHAR(256) NOT NULL UNIQUE, -- See also method UtilDataAccessLayer.TypeRowToNameCSharp();
    TableNameSql NVARCHAR(256), -- Can be null for memory rows.
    IsDelete BIT NOT NULL
)

GO
CREATE VIEW FrameworkTableIntegrate AS
SELECT
    Id,
    TableNameCSharp AS IdName
FROM
    FrameworkTable
GO

CREATE TABLE FrameworkField /* Used for configuration. Contains all in source code defined columns. Also calculated fields. */
(
    Id INT PRIMARY KEY IDENTITY,
    TableId INT FOREIGN KEY REFERENCES FrameworkTable(Id) NOT NULL ,
    FieldNameCSharp NVARCHAR(256) NOT NULL,
    FieldNameSql NVARCHAR(256), -- Can be null for calculated columns.
    Sort INT NOT NULL, -- See also method UtilDalType.TypeRowToFieldList();
    IsDelete BIT NOT NULL
    INDEX IX_FrameworkField UNIQUE (TableId, FieldNameCSharp)
)

GO
CREATE VIEW FrameworkFieldIntegrate
AS
SELECT
    Field.Id,
    CONCAT((SELECT TableIntegrate.IdName FROM FrameworkTableIntegrate TableIntegrate WHERE TableIntegrate.Id = Field.TableId), '; ', Field.FieldNameCSharp) AS IdName,
    Field.TableId,
    (SELECT TableIntegrate.IdName FROM FrameworkTableIntegrate TableIntegrate WHERE TableIntegrate.Id = Field.TableId) AS TableIdName,
    Field.FieldNameCSharp,
    Field.FieldNameSql,
    Field.Sort,
    Field.IsDelete
FROM
    FrameworkField Field
GO

CREATE TABLE FrameworkConfigGrid
(
    Id INT PRIMARY KEY IDENTITY,
    TableId INT FOREIGN KEY REFERENCES FrameworkTable(Id) NOT NULL ,
    ConfigName NVARCHAR(256),
    RowCountMax INT,
    WidthMax FLOAT, -- Default 5
    IsAllowInsert BIT,
    IsShowHeader BIT,
    IsShowPagination BIT,
    IsDelete BIT NOT NULL
    INDEX IX_FrameworkConfigGrid UNIQUE (TableId, ConfigName)
)

GO
CREATE VIEW FrameworkConfigGridIntegrate
AS
SELECT 
    ConfigGrid.Id,
    CONCAT((SELECT FrameworkTable.TableNameCSharp FROM FrameworkTable WHERE FrameworkTable.Id = ConfigGrid.TableId), '; ', ConfigGrid.ConfigName) AS IdName,
    ConfigGrid.TableId,
    (SELECT TableIntegrate.IdName FROM FrameworkTableIntegrate TableIntegrate WHERE TableIntegrate.Id = ConfigGrid.TableId) AS TableIdName,
    (SELECT FrameworkTable.TableNameCSharp FROM FrameworkTable FrameworkTable WHERE FrameworkTable.Id = ConfigGrid.TableId) AS TableNameCSharp,
    ConfigGrid.ConfigName,
    ConfigGrid.RowCountMax,
    ConfigGrid.WidthMax,
    ConfigGrid.IsAllowInsert,
    ConfigGrid.IsShowHeader,
    ConfigGrid.IsShowPagination,
    ConfigGrid.IsDelete
FROM
    FrameworkConfigGrid ConfigGrid
GO

CREATE TABLE FrameworkConfigField
(
    Id INT PRIMARY KEY IDENTITY,
    ConfigGridId INT FOREIGN KEY REFERENCES FrameworkConfigGrid(Id) NOT NULL,
    FieldId INT FOREIGN KEY REFERENCES FrameworkField(Id) NOT NULL,
    InstanceName NVARCHAR(256), -- Same field can be defined twice. For example, once as text link and once as image.
    Text NVARCHAR(256), -- Column header text.
    Description NVARCHAR(256), -- Column header description.
    IsVisible BIT,
    IsReadOnly BIT,
    Width FLOAT, -- Default 1
    IsMultiline BIT,
    Sort FLOAT,
    IsDelete BIT NOT NULL
    INDEX IX_FrameworkConfigField UNIQUE (ConfigGridId, FieldId, InstanceName)
)

GO
CREATE VIEW FrameworkConfigFieldIntegrate
AS
SELECT 
    /* Id */
    ConfigField.Id,
    ConfigField.ConfigGridId,
    (SELECT ConfigGridIntegrate.IdName FROM FrameworkConfigGridIntegrate ConfigGridIntegrate WHERE ConfigGridIntegrate.Id = ConfigField.ConfigGridId) AS ConfigGridIdName,
    ConfigField.FieldId,
    (SELECT FieldIntegrate.IdName FROM FrameworkFieldIntegrate FieldIntegrate WHERE FieldIntegrate.Id = ConfigField.FieldId) AS FieldIdName,
    ConfigField.InstanceName,
    /* Extension */
    (SELECT FrameworkTable.TableNameCSharp FROM FrameworkConfigGrid Grid, FrameworkTable FrameworkTable WHERE Grid.Id = ConfigField.ConfigGridId AND FrameworkTable.Id = Grid.TableId) AS TableNameCSharp,
    (SELECT Grid.ConfigName FROM FrameworkConfigGrid Grid WHERE Grid.Id = ConfigField.ConfigGridId) AS ConfigName,
    (SELECT Field.FieldNameCSharp FROM FrameworkField Field WHERE Field.Id = ConfigField.FieldId) AS FieldNameCSharp,
    /* Data */
    ConfigField.Text,
    ConfigField.Description,
    ConfigField.IsVisible,
    ConfigField.IsReadOnly,
    ConfigField.Width,
    ConfigField.IsMultiline,
    ConfigField.Sort,
    ConfigField.IsDelete
FROM
    FrameworkConfigField ConfigField
GO

GO
CREATE VIEW FrameworkConfigGridDisplay AS
SELECT
    ConfigGrid.Id AS Id,    
    ConfigGrid.TableId AS TableId,
    (SELECT FrameworkTable.TableNameCSharp FROM FrameworkTable FrameworkTable WHERE FrameworkTable.Id = ConfigGrid.TableId) AS TableNameCSharp,
    (SELECT FrameworkTable.TableNameSql FROM FrameworkTable FrameworkTable WHERE FrameworkTable.Id = ConfigGrid.TableId) AS TableNameSql,
    ConfigGrid.ConfigName AS ConfigName,
    ConfigGrid.RowCountMax AS RowCountMax,
    ConfigGrid.WidthMax AS WidthMax,
    ConfigGrid.IsAllowInsert AS IsAllowInsert,
    ConfigGrid.IsShowHeader AS IsShowHeader,
    ConfigGrid.IsShowPagination AS IsShowPagination,
    ConfigGrid.IsDelete AS IsDelete
FROM
    FrameworkConfigGrid ConfigGrid
UNION ALL
SELECT
    NULL AS Id,
    FrameworkTable.Id AS TableId,
    FrameworkTable.TableNameCSharp,
    FrameworkTable.TableNameSql,
    NULL AS ConfigName,
    NULL AS RowCountMax,
    NULL AS WidthMax,
    NULL AS IsAllowInsert,
    NULL AS IsShowHeader,
    NULL AS IsShowPagination,
    NULL AS IsDelete
FROM
    FrameworkTable FrameworkTable
WHERE
    FrameworkTable.Id NOT IN (SELECT TableId FROM FrameworkConfigGrid)
GO

GO
CREATE VIEW FrameworkConfigFieldDisplay AS
WITH ConfigGrid AS (
    SELECT
        ConfigGrid.Id AS Id,    
        ConfigGrid.TableId AS TableId,
        (SELECT FrameworkTable.TableNameCSharp FROM FrameworkTable FrameworkTable WHERE FrameworkTable.Id = ConfigGrid.TableId) AS TableNameCSharp,
        (SELECT FrameworkTable.TableNameSql FROM FrameworkTable FrameworkTable WHERE FrameworkTable.Id = ConfigGrid.TableId) AS TableNameSql,
        ConfigGrid.ConfigName AS ConfigName,
        ConfigGrid.IsDelete AS IsDelete
    FROM
        FrameworkConfigGrid ConfigGrid
    UNION ALL
    SELECT
        NULL AS Id,
        FrameworkTable.Id AS TableId,
        FrameworkTable.TableNameCSharp,
        FrameworkTable.TableNameSql,
        NULL AS ConfigName,
        NULL AS IsDelete
    FROM
        FrameworkTable FrameworkTable
    WHERE
        FrameworkTable.Id NOT IN (SELECT TableId FROM FrameworkConfigGrid)
)
SELECT 
    /* ConfigGrid */
    ConfigGrid.Id AS ConfigGridId,
    ConfigGrid.TableId AS ConfigGridTableId,
    (SELECT TableIntegrate.IdName FROM FrameworkTableIntegrate TableIntegrate WHERE TableIntegrate.Id = ConfigGrid.TableId) AS ConfigGridTableIdName,
    (SELECT FrameworkTable.TableNameCSharp FROM FrameworkTable FrameworkTable WHERE FrameworkTable.Id = ConfigGrid.TableId) AS ConfigGridTableNameCSharp,
    ConfigGrid.ConfigName AS ConfigGridConfigName,
    ConfigGrid.IsDelete AS ConfigGridIsDelete,
    /* Field */
    Field.Id AS FieldId,
    Field.TableId AS FieldTableId,
    Field.FieldNameCSharp AS FieldFieldNameCSharp,
    Field.FieldNameSql AS FieldFieldNameSql,
    Field.Sort AS FieldFieldSort,
    Field.IsDelete AS FieldIsDelete,
    /* ConfigField */
    ConfigField.Id AS ConfigFieldId,
    ConfigField.ConfigGridId AS ConfigFieldConfigGridId,
    ConfigField.FieldId AS ConfigFieldFieldId,
    ConfigField.InstanceName AS ConfigFieldInstanceName,
    ConfigField.Text AS ConfigFieldText,
    ConfigField.Description AS ConfigFieldDescription,
    ConfigField.IsVisible AS ConfigFieldIsVisible,
    ConfigField.IsReadOnly AS ConfigFieldIsReadOnly,
    ConfigField.Width AS ConfigFieldWidth,
    ConfigField.IsMultiline AS ConfigFieldIsMultiline,
    ConfigField.Sort AS ConfigFieldSort
FROM
    ConfigGrid ConfigGrid
JOIN 
    FrameworkField Field ON	Field.TableId = ConfigGrid.TableId
OUTER APPLY
(
    SELECT * FROM FrameworkConfigField ConfigField WHERE ConfigField.ConfigGridId = ConfigGrid.Id AND ConfigField.FieldId = Field.Id
) ConfigField