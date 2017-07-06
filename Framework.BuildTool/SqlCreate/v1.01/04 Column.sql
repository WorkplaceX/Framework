IF (NOT ISNULL((SELECT Version FROM FrameworkVersion), '') = 'v1.01') RETURN  -- Version Check

CREATE TABLE FrameworkColumn /* Used for configuration. Contains all in source code defined columns. Also calculated fields. */
(
	Id INT PRIMARY KEY IDENTITY,
	TableNameSql NVARCHAR(256),
	FieldNameSql NVARCHAR(256),
	FieldNameCsharp NVARCHAR(256),
	IsExist BIT
)
