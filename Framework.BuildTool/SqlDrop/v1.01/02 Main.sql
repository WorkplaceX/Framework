IF (NOT ISNULL((SELECT Version FROM FrameworkVersion), '') = 'v1.01') RETURN  -- Version Check

-- Drop all Framework table constraints
DECLARE @sql NVARCHAR(MAX) = '';
SELECT @sql += 'ALTER TABLE ' + QUOTENAME(OBJECT_SCHEMA_NAME(parent_object_id)) + '.' + QUOTENAME(OBJECT_NAME(parent_object_id)) + ' DROP CONSTRAINT ' + QUOTENAME(name)  + ';' + CHAR(10) 
FROM sys.foreign_keys
WHERE 
	QUOTENAME(OBJECT_SCHEMA_NAME(parent_object_id)) + '.' + QUOTENAME(OBJECT_NAME(parent_object_id)) LIKE '\[dbo\].\[Framework%' ESCAPE '\'
PRINT @sql;
EXEC sys.sp_executesql @sql;
GO
-- Drop all Framework table
DECLARE @sql NVARCHAR(MAX) = '';
SELECT @sql += 'DROP TABLE ' + QUOTENAME(OBJECT_SCHEMA_NAME(object_id)) + '.' + QUOTENAME(OBJECT_NAME(object_id)) + CHAR(13)
FROM sys.tables
WHERE 
	QUOTENAME(OBJECT_SCHEMA_NAME(object_id)) + '.' + QUOTENAME(OBJECT_NAME(object_id)) LIKE '\[dbo\].\[Framework%' ESCAPE '\' AND NOT 
	QUOTENAME(OBJECT_SCHEMA_NAME(object_id)) + '.' + QUOTENAME(OBJECT_NAME(object_id)) = '[dbo].[FrameworkVersion]'
PRINT @sql;
EXEC sys.sp_executesql @sql;
GO
-- Drop all Framework views
DECLARE @sql NVARCHAR(MAX) = '';
SELECT @sql += 'DROP VIEW ' + QUOTENAME(OBJECT_SCHEMA_NAME(object_id)) + '.' + QUOTENAME(OBJECT_NAME(object_id)) + CHAR(13)
FROM sys.views
WHERE 
	QUOTENAME(OBJECT_SCHEMA_NAME(object_id)) + '.' + QUOTENAME(OBJECT_NAME(object_id)) LIKE '\[dbo\].\[Framework%' ESCAPE '\'
PRINT @sql;
EXEC sys.sp_executesql @sql;

DROP PROCEDURE FrameworkConfigurationPathUpdate

RETURN

/* Text */
DROP VIEW FrameworkTextView
DROP TABLE FrameworkText
/* FileStorageText */
DROP VIEW FrameworkFileStorageView
DROP TABLE FrameworkFileStorage
/* Role */
DROP TABLE FrameworkUserRole
DROP TABLE FrameworkRole
/* Column */
DROP TABLE FrameworkColumn
/* Session */
ALTER TABLE FrameworkConfigurationpATH DROP CONSTRAINT FK_FrameworkConfigurationPath_SessionId
DROP TABLE FrameworkSession
/* Configuration */
DROP PROCEDURE FrameworkConfigurationPathUpdate
DROP VIEW FrameworkConfigurationPathView
DROP VIEW FrameworkLanguageHierarchy
DROP VIEW FrameworkApplicationHierarchy
DROP VIEW FrameworkLanguageView
DROP VIEW FrameworkConfigurationView
DROP TABLE FrameworkConfigurationPath
ALTER TABLE FrameworkConfiguration DROP CONSTRAINT FK_FrameworkConfiguration_UserId
DROP TABLE FrameworkUser
ALTER TABLE FrameworkConfiguration DROP CONSTRAINT FK_FrameworkConfiguration_LanguageId
DROP TABLE FrameworkLanguage
DROP TABLE FrameworkConfiguration
DROP TABLE FrameworkApplication
DROP TABLE FrameworkApplicationType