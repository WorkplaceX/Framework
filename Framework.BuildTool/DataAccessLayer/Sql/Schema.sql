SELECT
	NEWID() AS IdView, -- For EF
	TableList.SchemaName,
	TableList.TableName,
	ColumnList.name AS FieldName,
	ColumnList.column_id AS FieldNameOrderBy,
	CAST(CASE WHEN TableList.type = 'V' THEN 1 ELSE 0 END AS BIT) AS IsView,
	ColumnList.system_type_id AS SqlType,
	ColumnList.is_nullable AS IsNullable,
	/* IsPrimaryKey */
	(
		CASE WHEN EXISTS(
		SELECT * 
		FROM sys.index_columns PkFieldName 
		WHERE PkFieldName.object_id = Pk.object_id AND PkFieldName.index_id = Pk.index_id AND PkFieldName.column_id = ColumnList.column_id)
		THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END
	) AS IsPrimaryKey,
	/* IsForeignKey */
	(
		CASE WHEN EXISTS(
		SELECT *
		FROM sys.foreign_keys Fk JOIN sys.foreign_key_columns FkFieldName ON FkFieldName.constraint_object_id = Fk.object_id
		WHERE FkFieldName.parent_object_id = ColumnList.object_id AND FkFieldName.parent_column_id = ColumnList.column_id)
		THEN 1 ELSE 0 END
	) AS IsForeignKey,
	/* ForeignTableName */
	(
		SELECT TOP 1 OBJECT_SCHEMA_NAME(FkFieldName.referenced_object_id) + '.' + OBJECT_NAME(FkFieldName.referenced_object_id) AS TableName -- ForeignKey can have more than one column
		FROM sys.foreign_keys Fk JOIN sys.foreign_key_columns FkFieldName ON FkFieldName.constraint_object_id = Fk.object_id
		WHERE FkFieldName.parent_object_id = ColumnList.object_id AND FkFieldName.parent_column_id = ColumnList.column_id
	) AS ForeignTableName,
	/* ForeignFieldName */
	(
		SELECT TOP 1 COL_NAME(FkFieldName.referenced_object_id, FkFieldName.referenced_column_id) AS FieldName -- ForeignKey can have more than one column
		FROM sys.foreign_keys Fk JOIN sys.foreign_key_columns FkFieldName ON FkFieldName.constraint_object_id = Fk.object_id
		WHERE FkFieldName.parent_object_id = ColumnList.object_id AND FkFieldName.parent_column_id = ColumnList.column_id
		ORDER BY COL_NAME(FkFieldName.referenced_object_id, FkFieldName.referenced_column_id)
	) AS ForeignFieldName,
	CASE WHEN (SELECT Extended.value FROM sys.extended_properties Extended WHERE Extended.major_id = ColumnList.object_id) = 1 THEN CAST(1 AS BIT) ELSE CAST(0 AS bit) END AS IsSystemTable

FROM 
	sys.columns ColumnList

JOIN
	(
		SELECT OBJECT_SCHEMA_NAME(object_id) AS SchemaName, name as TableName, object_id, type FROM sys.tables
		UNION ALL
		SELECT OBJECT_SCHEMA_NAME(object_id) AS SchemaName, name as TableName, object_id, type FROM sys.views
	) TableList ON (TableList.object_id = ColumnList.object_id)

LEFT JOIN
	sys.indexes Pk ON Pk.object_id = TableList.object_id AND Pk.is_primary_key = 1

ORDER BY
	TableList.SchemaName, TableList.TableName, ColumnList.column_id