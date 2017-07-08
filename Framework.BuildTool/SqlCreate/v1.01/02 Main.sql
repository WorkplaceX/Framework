--IF (NOT ISNULL((SELECT Version FROM FrameworkVersion), '') = 'v1.01') RETURN -- Version Check

/* ---------------------------------------------------- Drop ---------------------------------------------------- */

/* Drop all Framework table constraints */
DECLARE @sql NVARCHAR(MAX) = '';
SELECT @sql += 'ALTER TABLE ' + QUOTENAME(OBJECT_SCHEMA_NAME(parent_object_id)) + '.' + QUOTENAME(OBJECT_NAME(parent_object_id)) + ' DROP CONSTRAINT ' + QUOTENAME(name)  + ';' + CHAR(10) 
FROM sys.foreign_keys
WHERE 
	QUOTENAME(OBJECT_SCHEMA_NAME(parent_object_id)) + '.' + QUOTENAME(OBJECT_NAME(parent_object_id)) LIKE '\[dbo\].\[Framework%' ESCAPE '\'
EXEC sys.sp_executesql @sql;
GO
-- Drop all Framework table
DECLARE @sql NVARCHAR(MAX) = '';
SELECT @sql += 'DROP TABLE ' + QUOTENAME(OBJECT_SCHEMA_NAME(object_id)) + '.' + QUOTENAME(OBJECT_NAME(object_id)) + CHAR(13)
FROM sys.tables
WHERE 
	QUOTENAME(OBJECT_SCHEMA_NAME(object_id)) + '.' + QUOTENAME(OBJECT_NAME(object_id)) LIKE '\[dbo\].\[Framework%' ESCAPE '\' AND NOT 
	QUOTENAME(OBJECT_SCHEMA_NAME(object_id)) + '.' + QUOTENAME(OBJECT_NAME(object_id)) = '[dbo].[FrameworkVersion]'
EXEC sys.sp_executesql @sql;
GO
-- Drop all Framework views
DECLARE @sql NVARCHAR(MAX) = '';
SELECT @sql += 'DROP VIEW ' + QUOTENAME(OBJECT_SCHEMA_NAME(object_id)) + '.' + QUOTENAME(OBJECT_NAME(object_id)) + CHAR(13)
FROM sys.views
WHERE 
	QUOTENAME(OBJECT_SCHEMA_NAME(object_id)) + '.' + QUOTENAME(OBJECT_NAME(object_id)) LIKE '\[dbo\].\[Framework%' ESCAPE '\'
EXEC sys.sp_executesql @sql;

IF EXISTS(SELECT * FROM sys.procedures WHERE QUOTENAME(OBJECT_SCHEMA_NAME(object_id)) + '.' + QUOTENAME(OBJECT_NAME(object_id)) = '[dbo].[FrameworkConfigurationPathUpdate]')
DROP PROCEDURE FrameworkConfigurationPathUpdate

GO

/* ---------------------------------------------------- Create Table ---------------------------------------------------- */

CREATE TABLE FrameworkApplicationType
(
	Id INT PRIMARY KEY IDENTITY,
  	Name NVARCHAR(256) NOT NULL UNIQUE
)

CREATE TABLE FrameworkApplication
(
	Id INT PRIMARY KEY IDENTITY,
	ParentId INT FOREIGN KEY REFERENCES FrameworkApplication(Id),
  	Name NVARCHAR(256) NOT NULL UNIQUE,
	ApplicationTypeId INT FOREIGN KEY REFERENCES FrameworkApplicationType(Id),
	Domain NVARCHAR(256) NOT NULL UNIQUE /* Url */
)

CREATE TABLE FrameworkConfiguration
(
	Id INT PRIMARY KEY IDENTITY,
	ApplicationId INT FOREIGN KEY REFERENCES FrameworkApplication(Id),
	LanguageId INT, /* ADD CONSTRAINT */
	UserId INT /* ADD CONSTRAINT */ /* Logged in user */ 
	INDEX IX_FrameworkConfiguration UNIQUE (ApplicationId, LanguageId, UserId)
)

CREATE TABLE FrameworkLanguage /* For example English, German */
(
	Id INT PRIMARY KEY IDENTITY,
	ConfigurationId INT FOREIGN KEY REFERENCES FrameworkConfiguration(Id) NOT NULL,
	ParentId INT FOREIGN KEY REFERENCES FrameworkLanguage(Id),
  	Name NVARCHAR(256) NOT NULL,
	INDEX IX_FrameworkLanguageName UNIQUE (ConfigurationId, Name)
)
ALTER TABLE FrameworkConfiguration ADD CONSTRAINT FK_FrameworkConfiguration_LanguageId FOREIGN KEY (LanguageId) REFERENCES FrameworkLanguage(Id)

CREATE TABLE FrameworkUser
(
	Id INT PRIMARY KEY IDENTITY,
	ApplicationId INT FOREIGN KEY REFERENCES FrameworkApplication(Id) NOT NULL,
  	Name NVARCHAR(256) NOT NULL, /* User name or email */
	Password NVARCHAR(256),
	LanguageId INT FOREIGN KEY REFERENCES FrameworkLanguage(Id), /* Default language. See also FrameworkSession.LanguageId */
	Confirmation UNIQUEIDENTIFIER,
	IsConfirmation BIT,
	IsActive BIT,
	INDEX IX_FrameworkUser UNIQUE (ApplicationId, Name)
)
ALTER TABLE FrameworkConfiguration ADD CONSTRAINT FK_FrameworkConfiguration_UserId FOREIGN KEY (UserId) REFERENCES FrameworkUser(Id)

CREATE TABLE FrameworkConfigurationPath
(
	Id INT PRIMARY KEY IDENTITY,
	ApplicationId INT FOREIGN KEY REFERENCES FrameworkApplication(Id),
	LanguageId INT FOREIGN KEY REFERENCES FrameworkLanguage(Id),
	UserId INT FOREIGN KEY REFERENCES FrameworkUser(Id),
	SessionId INT /* ADD CONSTRAINT */,
	ConfigurationId INT FOREIGN KEY REFERENCES FrameworkConfiguration(Id) NOT NULL,
	ConfigurationIdContain INT FOREIGN KEY REFERENCES FrameworkConfiguration(Id) NOT NULL,
	Level INT NOT NULL,
	INDEX IX_FrameworkConfigurationTree UNIQUE (ApplicationId, LanguageId, UserId, SessionId, ConfigurationId, ConfigurationIdContain)
)

CREATE TABLE FrameworkSession
(
	Id INT PRIMARY KEY IDENTITY,
  	Name UNIQUEIDENTIFIER NOT NULL UNIQUE,
	ApplicationId INT FOREIGN KEY REFERENCES FrameworkApplication(Id),
	ApplicationTypeId INT FOREIGN KEY REFERENCES FrameworkApplicationType(Id),
	LanguageId INT FOREIGN KEY REFERENCES FrameworkApplication(Id), /* User interface language for this session */
	ConfigurationId INT FOREIGN KEY REFERENCES FrameworkConfiguration(Id),
	UserId INT FOREIGN KEY REFERENCES FrameworkUser(Id) /* Link to user when logged in. */
)
ALTER TABLE FrameworkConfigurationPath ADD CONSTRAINT FK_FrameworkConfigurationPath_SessionId FOREIGN KEY (SessionId) REFERENCES FrameworkSession(Id)

GO

CREATE VIEW FrameworkConfigurationView
AS
SELECT
	Configuration.Id AS ConfigurationId,
	Application.Id AS ApplicationId,
	Application.Name AS ApplicationName,
	Language.Id AS LanguageId,
	Language.Name AS LanguageName,
	[User].Id AS UserId,
	[User].Name AS UserName,
	CONCAT(
		CASE WHEN Application.Id IS NOT NULL THEN 'Application: ' END, 
		Application.Name,
		CASE WHEN Language.Id IS NOT NULL THEN 'Language: ' END,
		Language.Name,
		CASE WHEN [User].Id IS NOT NULL THEN 'User: ' END,
		[User].Name
	) AS Debug

FROM
	FrameworkConfiguration Configuration

LEFT JOIN
	FrameworkApplication Application ON Application.Id = Configuration.ApplicationId

LEFT JOIN
	FrameworkLanguage Language ON Language.Id = Configuration.LanguageId

LEFT JOIN
	FrameworkUser [User] ON [User].Id = Configuration.UserId

GO

CREATE VIEW FrameworkLanguageDisplay
AS
SELECT 
	Path.SessionId,
	Path.Name AS LanguageName,
	Language.Id AS LanguageId,
	Language.ConfigurationId AS ConfigurationIdApplication, /* Application on which Language is defined */
	ConfigurationLanguage.Id AS ConfigurationIdLanguage,
	Application.Name AS ApplicationName /* Session ApplicationName */

FROM
	(
		SELECT
			Path.SessionId,
			Language.Name,
			MAX(Path.Level) AS Level
		FROM
			FrameworkConfigurationPath Path,
			FrameworkLanguage Language
		WHERE
			Language.ConfigurationId = Path.ConfigurationIdContain 
		GROUP BY
			Path.SessionId,
			Language.Name
	) AS Path

JOIN
	FrameworkConfigurationPath Path2 ON (Path2.SessionId = Path.SessionId AND Path2.Level = Path.Level)

LEFT JOIN
	FrameworkLanguage Language ON (Language.ConfigurationId = Path2.ConfigurationIdContain AND Language.Name = Path.Name)

LEFT JOIN
	FrameworkConfiguration ConfigurationLanguage ON (ConfigurationLanguage.LanguageId = Language.Id)

LEFT JOIN
	FrameworkSession Session ON (Session.Id = Path.SessionId)
	
LEFT JOIN
	FrameworkApplication Application ON (Application.Id = Session.ApplicationId)

GO

CREATE VIEW FrameworkApplicationHierarchy
AS
WITH Hierarchy
AS
(
	SELECT 
		FirstGeneration.*, 
		100 AS Level,
		Id AS LastChildId 
	FROM 
		FrameworkApplication AS FirstGeneration
	UNION ALL
	SELECT 
		NextGeneration.*, 
		Parent.Level - 1,
		Parent.LastChildId 
	FROM 
		FrameworkApplication AS NextGeneration,
		Hierarchy AS Parent 
	WHERE 
		NextGeneration.Id = Parent.ParentId
)
SELECT 
  Hierarchy.LastChildId AS ApplicationId, 
  Hierarchy.Id AS ApplicationIdContain, 
  Hierarchy.Level, 
  Hierarchy.ParentId,
  Application.Name,
  Configuration.Id ConfigurationId,
  ConfigurationContain.Id AS ConfigurationIdContain

FROM 
	Hierarchy Hierarchy

LEFT JOIN
	FrameworkApplication Application ON Application.Id = Hierarchy.Id

LEFT JOIN
	FrameworkConfiguration Configuration ON Configuration.ApplicationId = Hierarchy.LastChildId

LEFT JOIN
	FrameworkConfiguration ConfigurationContain ON ConfigurationContain.ApplicationId = Hierarchy.Id

GO

CREATE VIEW FrameworkLanguageHierarchy
AS
WITH Hierarchy
AS
(
	SELECT 
		FirstGeneration.*, 
		100 AS Level,
		Id AS LastChildId 
	FROM 
		FrameworkLanguage AS FirstGeneration
	UNION ALL
	SELECT 
		NextGeneration.*, 
		Parent.Level - 1,
		Parent.LastChildId 
	FROM 
		FrameworkLanguage AS NextGeneration,
		Hierarchy AS Parent 
	WHERE 
		NextGeneration.Id = Parent.ParentId
)
SELECT 
  Hierarchy.LastChildId AS LanguageId, 
  Hierarchy.Id AS LanguageIdContain, 
  Hierarchy.Level, 
  Hierarchy.ParentId,
  Language.Name,
  Configuration.Id ConfigurationId,
  ConfigurationContain.Id AS ConfigurationIdContain

FROM 
	Hierarchy Hierarchy

LEFT JOIN
	FrameworkLanguage Language ON Language.Id = Hierarchy.Id

LEFT JOIN
	FrameworkConfiguration Configuration ON Configuration.LanguageId = Hierarchy.LastChildId

LEFT JOIN
	FrameworkConfiguration ConfigurationContain ON ConfigurationContain.LanguageId = Hierarchy.Id

GO

CREATE VIEW FrameworkConfigurationPathView
AS
SELECT
	Path.Id,
	Path.ApplicationId,
	Path.LanguageId,
	Path.UserId,
	Path.SessionId,
	Path.ConfigurationId,
	Path.ConfigurationIdContain,
	Path.Level,
	Application.Name AS ApplicationName,
	Language.Name AS LanguageName,
	[User].Name AS UserName,
	Configuration.Debug AS ConfigurationDebug,
	ConfigurationContain.Debug AS ConfigurationDebugContain


FROM
	FrameworkConfigurationPath Path

LEFT JOIN
	FrameworkApplication Application ON Application.Id = Path.ApplicationId

LEFT JOIN
	FrameworkLanguage Language ON Language.Id = Path.LanguageId

LEFT JOIN
	FrameworkUser [User] ON [User].Id = Path.UserId

LEFT JOIN
	FrameworkConfigurationView Configuration ON Configuration.ConfigurationId = Path.ConfigurationId

LEFT JOIN
	FrameworkConfigurationView ConfigurationContain ON ConfigurationContain.ConfigurationId = Path.ConfigurationIdContain

GO

CREATE TABLE FrameworkColumn /* Used for configuration. Contains all in source code defined columns. Also calculated fields. */
(
	Id INT PRIMARY KEY IDENTITY,
	TableNameSql NVARCHAR(256),
	FieldNameSql NVARCHAR(256),
	FieldNameCsharp NVARCHAR(256),
	IsExist BIT
)

CREATE TABLE FrameworkRole /* For example Admin, DataRead */
(
	Id INT PRIMARY KEY IDENTITY,
	ApplicationTypeId INT FOREIGN KEY REFERENCES FrameworkApplicationType(Id) NOT NULL,
  	Name NVARCHAR(256) NOT NULL,
	INDEX IX_FrameworkRole UNIQUE (ApplicationTypeId, Name)
)

CREATE TABLE FrameworkUserRole
(
	Id INT PRIMARY KEY IDENTITY,
	UserId INT FOREIGN KEY REFERENCES FrameworkUser(Id) NOT NULL,
	RoleId INT FOREIGN KEY REFERENCES FrameworkRole(Id) NOT NULL,
	INDEX IX_FrameworkUserRole UNIQUE (UserId, RoleId)
)

CREATE TABLE FrameworkFileStorage
(
	Id INT PRIMARY KEY IDENTITY,
	ConfigurationId INT FOREIGN KEY REFERENCES FrameworkConfiguration(Id) NOT NULL,
  	Name NVARCHAR(256) NOT NULL, /* File name with path */
  	FileNameUpload NVARCHAR(256),
	Data VARBINARY(MAX),
	IsDelete BIT,
	INDEX IX_FrameworkConfigurationTree UNIQUE (ConfigurationId, Name)
)

GO

CREATE VIEW FrameworkFileStorageView
AS
SELECT 
	Path.ApplicationId,
	Path.LanguageId,
	Path.UserId,
	Path.SessionId,
	Path.Name,
	FileStorage.ConfigurationId,
	FileStorage.FileNameUpload,
	FileStorage.Data,
	FileStorage.IsDelete

FROM
	FrameworkConfigurationPath Path2,
	(
		SELECT
			Path.ApplicationId,
			Path.LanguageId,
			Path.UserId,
			Path.SessionId,
			Storage.Name,
			MAX(Path.Level) AS Level
		FROM
			FrameworkConfigurationPath Path,
			FrameworkFileStorage Storage
		WHERE
			Storage.ConfigurationId = Path.ConfigurationIdContain 
		GROUP BY
			Path.ApplicationId,
			Path.LanguageId,
			Path.UserId,
			Path.SessionId,
			Storage.Name
	) AS Path,
	FrameworkFileStorage FileStorage

WHERE
	Path2.SessionId = Path.SessionId AND
	Path2.Level = Path.Level AND
	FileStorage.ConfigurationId = Path2.ConfigurationIdContain AND 
	FileStorage.Name = Path.Name

GO

/* ---------------------------------------------------- FrameworkText ---------------------------------------------------- */

CREATE TABLE FrameworkText
(
	Id INT PRIMARY KEY IDENTITY,
	ConfigurationId INT FOREIGN KEY REFERENCES FrameworkConfiguration(Id) NOT NULL,
  	Name NVARCHAR(256) NOT NULL,
  	Text NVARCHAR(256),
	INDEX IX_FrameworkLanguageName UNIQUE (ConfigurationId, Name)
)

GO

CREATE VIEW FrameworkTextView
AS
SELECT 
	Path.ApplicationId,
	Path.LanguageId,
	Path.UserId,
	Path.SessionId,
	Path.Name,
	Text.ConfigurationId,
	Text.Text

FROM
	FrameworkConfigurationPath Path2,
	(
		SELECT
			Path.ApplicationId,
			Path.LanguageId,
			Path.UserId,
			Path.SessionId,
			Text.Name,
			MAX(Path.Level) AS Level
		FROM
			FrameworkConfigurationPath Path,
			FrameworkText Text
		WHERE
			Text.ConfigurationId = Path.ConfigurationIdContain 
		GROUP BY
			Path.ApplicationId,
			Path.LanguageId,
			Path.UserId,
			Path.SessionId,
			Text.Name
	) AS Path,
	FrameworkText Text

WHERE
	Path2.SessionId = Path.SessionId AND
	Path2.Level = Path.Level AND
	Text.ConfigurationId = Path2.ConfigurationIdContain AND 
	Text.Name = Path.Name

GO

/* ---------------------------------------------------- Stored Procedure ---------------------------------------------------- */

CREATE PROCEDURE FrameworkConfigurationPathUpdate
AS

/* Every Application and Language gets it's Configuration layer */
INSERT INTO FrameworkConfiguration (ApplicationId)
SELECT Id FROM FrameworkApplication
EXCEPT SELECT ApplicationId FROM FrameworkConfiguration

INSERT INTO FrameworkConfiguration (LanguageId)
SELECT Id FROM FrameworkLanguage
EXCEPT SELECT LanguageId FROM FrameworkConfiguration

/* Session for every language exclusive */
INSERT INTO FrameworkSession (Name, LanguageId)
SELECT NEWID(), Id FROM FrameworkLanguage Language
WHERE EXISTS 
(
	SELECT NULL AS ApplicationId, Language.Id, NULL AS UserId
	EXCEPT
	SELECT Session.ApplicationId, Session.LanguageId, Session.UserId FROM FrameworkSession Session
)

/* Session for every application exclusive */
INSERT INTO FrameworkSession (Name, ApplicationId)
SELECT NEWID(), Id FROM FrameworkApplication Application
WHERE EXISTS 
(
	SELECT Application.Id, NULL AS LanguageId, NULL AS UserId
	EXCEPT
	SELECT Session.ApplicationId, Session.LanguageId, Session.UserId FROM FrameworkSession Session
)

DELETE FrameworkConfigurationPath

INSERT INTO FrameworkConfigurationPath (ApplicationId, LanguageId, UserId, SessionId, ConfigurationId, ConfigurationIdContain, Level)
/* Application layer */
SELECT
	Session.ApplicationId, Session.LanguageId, 
	Session.UserId,
	Session.Id AS SessionId,
	Application.ConfigurationId,
	Application.ConfigurationIdContain,
	Application.Level
FROM
	FrameworkSession Session
LEFT JOIN
	FrameworkApplicationHierarchy Application ON Application.ApplicationId = Session.ApplicationId
WHERE
	Session.ApplicationId IS NOT NULL
UNION ALL
/* Language layer */
SELECT
	Session.ApplicationId, Session.LanguageId, 
	Session.UserId,
	Session.Id,
	Language.ConfigurationId,
	Language.ConfigurationIdContain,
	Language.Level + 1000
FROM
	FrameworkSession Session
JOIN
	FrameworkLanguageHierarchy Language ON Language.LanguageId = Session.LanguageId
UNION ALL
/* User layer */
SELECT
	Session.ApplicationId, Session.LanguageId, 
	Session.UserId,
	Session.Id,
	Session.ConfigurationId,
	Session.ConfigurationId AS ConfigurationIdContain,
	2000 AS Level
FROM
	FrameworkSession Session
WHERE
	Session.ConfigurationId IS NOT NULL

GO

/* ---------------------------------------------------- Data ---------------------------------------------------- */

/* ApplicationType */
INSERT INTO FrameworkApplicationType (Name)
SELECT 'Framework' AS Name
UNION ALL
SELECT 'PTC' AS Name
UNION ALL
SELECT 'LPN' AS Name

/* Application */
INSERT INTO FrameworkApplication (Name, ApplicationTypeId, Domain)
SELECT 'Framework' AS Name, (SELECT Id FROM FrameworkApplicationType WHERE Name = 'Framework') AS ApplicationTypeId, 'framework' AS Domain
INSERT INTO FrameworkApplication (Name, ApplicationTypeId, Domain)
SELECT 'PTC' AS Name, (SELECT Id FROM FrameworkApplicationType WHERE Name = 'PTC'), 'ptc' AS Domain
INSERT INTO FrameworkApplication (ParentId, Name, ApplicationTypeId, Domain)
SELECT (SELECT Id From FrameworkApplication WHERE Name = 'PTC'), 'PTC CH' AS Name, (SELECT Id FROM FrameworkApplicationType WHERE Name = 'PTC'), 'ptcch'
INSERT INTO FrameworkApplication (ParentId, Name, ApplicationTypeId, Domain)
SELECT (SELECT Id From FrameworkApplication WHERE Name = 'PTC CH'), 'PTC D' AS Name, (SELECT Id FROM FrameworkApplicationType WHERE Name = 'PTC'), 'ptcd'
INSERT INTO FrameworkApplication (Name, ApplicationTypeId, Domain)
SELECT 'LPN' AS Name, (SELECT Id FROM FrameworkApplicationType WHERE Name = 'LPN'), 'lpn'

GO
EXEC FrameworkConfigurationPathUpdate
GO

/* FileStorage */
INSERT INTO FrameworkFileStorage (ConfigurationId, Name, FileNameUpload)
SELECT (SELECT ConfigurationId FROM FrameworkConfigurationView WHERE ApplicationName = 'PTC'), 'Outline A', 'OutlineA.docx'
UNION ALL
SELECT (SELECT ConfigurationId FROM FrameworkConfigurationView WHERE ApplicationName = 'PTC'), 'Outline B', 'OutlineB.docx'
UNION ALL
SELECT (SELECT ConfigurationId FROM FrameworkConfigurationView WHERE ApplicationName = 'PTC CH'), 'Outline D', 'OutlineD.docx'
UNION ALL
SELECT (SELECT ConfigurationId FROM FrameworkConfigurationView WHERE ApplicationName = 'PTC CH'), 'Outline B', 'OutlineB2.docx'

/* Language */
INSERT FrameworkLanguage (ConfigurationId, Name)
SELECT (SELECT ConfigurationId FROM FrameworkConfigurationView WHERE ApplicationName = 'PTC'), 'Default'
INSERT FrameworkLanguage (ConfigurationId, ParentId, Name)
SELECT (SELECT ConfigurationId FROM FrameworkConfigurationView WHERE ApplicationName = 'PTC'), (SELECT Id FROM FrameworkLanguage WHERE ConfigurationId = (SELECT ConfigurationId FROM FrameworkConfigurationView WHERE ApplicationName = 'PTC') AND Name = 'Default'), 'English'
INSERT FrameworkLanguage (ConfigurationId, ParentId, Name)
SELECT (SELECT ConfigurationId FROM FrameworkConfigurationView WHERE ApplicationName = 'PTC'), (SELECT Id FROM FrameworkLanguage WHERE ConfigurationId = (SELECT ConfigurationId FROM FrameworkConfigurationView WHERE ApplicationName = 'PTC') AND Name = 'English'), 'German'
INSERT FrameworkLanguage (ConfigurationId, ParentId, Name)
SELECT (SELECT ConfigurationId FROM FrameworkConfigurationView WHERE ApplicationName = 'PTC CH'), (SELECT Id FROM FrameworkLanguage WHERE ConfigurationId = (SELECT ConfigurationId FROM FrameworkConfigurationView WHERE ApplicationName = 'PTC') AND Name = 'English'), 'Italian'
INSERT FrameworkLanguage (ConfigurationId, Name)
SELECT (SELECT ConfigurationId FROM FrameworkConfigurationView WHERE ApplicationName = 'LPN'), 'French'

/* User */
INSERT INTO FrameworkUser (ApplicationId, Name, LanguageId)
SELECT (SELECT Id AS ApplicationId FROM FrameworkApplication WHERE Name = 'Framework') AS ApplicationId,  'Admin' AS Name, NULL
UNION ALL
SELECT (SELECT Id AS ApplicationId FROM FrameworkApplication WHERE Name = 'LPN') AS ApplicationId,  'John' AS Name, NULL
UNION ALL
SELECT (SELECT Id AS ApplicationId FROM FrameworkApplication WHERE Name = 'PTC') AS ApplicationId, 'Hnc' AS Name, NULL

/* Role */
INSERT INTO FrameworkRole (ApplicationTypeId, Name)
SELECT (SELECT Id FROM FrameworkApplication WHERE Name = 'Framework'), 'Admin'
UNION ALL
SELECT (SELECT Id FROM FrameworkApplication WHERE Name = 'PTC'), 'Coordy'
UNION ALL
SELECT (SELECT Id FROM FrameworkApplication WHERE Name = 'PTC'), 'Spky'

GO
EXEC FrameworkConfigurationPathUpdate
GO

/* Add User */
INSERT INTO FrameworkUser (ApplicationId, Name, LanguageId)
SELECT (SELECT Id AS ApplicationId FROM FrameworkApplication WHERE Name = 'PTC CH') AS ApplicationId, 'Hnc2' AS Name, (SELECT TOP 1 LanguageId FROM FrameworkLanguageDisplay WHERE ApplicationName = 'PTC CH' AND LanguageName = 'Italian')
INSERT INTO FrameworkSession (Name, ApplicationId, ApplicationTypeId, UserId, LanguageId)
SELECT NEWID(), (SELECT Id FROM FrameworkApplication WHERE Name = 'PTC CH'), (SELECT ApplicationTypeId FROM FrameworkApplication WHERE Name = 'PTC CH'), (SELECT Id FROM FrameworkUser WHERE ApplicationId = (SELECT Id FROM FrameworkApplication WHERE Name = 'PTC CH') AND Name = 'hnc2'), (SELECT LanguageId FROM FrameworkUser WHERE ApplicationId = (SELECT Id FROM FrameworkApplication WHERE Name = 'PTC CH') AND Name = 'hnc2')
INSERT INTO FrameworkConfiguration (UserId)
SELECT UserId FROM FrameworkSession
EXCEPT SELECT UserId FROM FrameworkConfiguration
UPDATE Session
SET Session.ConfigurationId = (SELECT Id FROM FrameworkConfiguration WHERE UserId = Session.UserId)
FROM FrameworkSession Session
WHERE Session.UserId IS NOT NULL AND Session.ConfigurationId IS NULL

GO
EXEC FrameworkConfigurationPathUpdate
GO

/* Add Text */
INSERT INTO FrameworkText (ConfigurationId, Name)
SELECT (SELECT TOP 1 ConfigurationIdLanguage FROM FrameworkLanguageDisplay WHERE ApplicationName = 'LPN' AND LanguageName = 'French'), 'Connecter' 
UNION ALL
SELECT (SELECT TOP 1 ConfigurationIdLanguage FROM FrameworkLanguageDisplay WHERE ApplicationName = 'PTC' AND LanguageName = 'Default'), 'Login' 
UNION ALL
SELECT (SELECT TOP 1 ConfigurationIdLanguage FROM FrameworkLanguageDisplay WHERE ApplicationName = 'PTC' AND LanguageName = 'German'), 'Anmelden' 

GO

EXEC FrameworkConfigurationPathUpdate
