IF (NOT ISNULL((SELECT Version FROM FrameworkVersion), '') = 'v1.02') RETURN -- Version check

CREATE TABLE FrameworkApplication
(
	Id INT PRIMARY KEY IDENTITY,
  	Name NVARCHAR(256) NOT NULL UNIQUE
)

CREATE TABLE FrameworkSwitch
(
	Id INT PRIMARY KEY IDENTITY,
	ApplicationId INT FOREIGN KEY REFERENCES FrameworkApplication(Id),
	LanguageId INT
)

CREATE TABLE FrameworkSwitchTree
(
	Id INT PRIMARY KEY IDENTITY,
	SwitchId INT FOREIGN KEY REFERENCES FrameworkSwitch(Id) NOT NULL,
	ParentId INT FOREIGN KEY REFERENCES FrameworkSwitchTree(Id) NOT NULL,
)

CREATE TABLE FrameworkSwitchPath
(
	Id INT PRIMARY KEY IDENTITY,
	SwitchId INT FOREIGN KEY REFERENCES FrameworkSwitch(Id) NOT NULL,
	ContainSwitchId INT FOREIGN KEY REFERENCES FrameworkSwitch(Id) NOT NULL,
	Level INT NOT NULL,
	INDEX IX_FrameworkSwitchTree UNIQUE (SwitchId, ContainSwitchId)
)

CREATE TABLE FrameworkFileStorage
(
	Id INT PRIMARY KEY IDENTITY,
	SwitchId INT FOREIGN KEY REFERENCES FrameworkSwitch(Id) NOT NULL,
  	FileName NVARCHAR(256) NOT NULL,
  	FileNameUpload NVARCHAR(256),
	Data VARBINARY(MAX),
	IsDelete BIT,
	INDEX IX_FrameworkSwitchTree UNIQUE (SwitchId, FileName)
)

CREATE TABLE FrameworkLanguage /* For example English, German */
(
	Id INT PRIMARY KEY IDENTITY,
	SwitchId INT FOREIGN KEY REFERENCES FrameworkSwitch(Id) NOT NULL,
	ParentId INT FOREIGN KEY REFERENCES FrameworkLanguage(Id), -- Fallback if language does not exist.
  	Name NVARCHAR(256) NOT NULL,
	INDEX IX_FrameworkLanguageName UNIQUE (SwitchId, Name)
)

ALTER TABLE FrameworkSwitch ADD	CONSTRAINT FK_FrameworkSwitch_LanguageId FOREIGN KEY (LanguageId) REFERENCES FrameworkLanguage(Id)

CREATE TABLE FrameworkText
(
	Id INT PRIMARY KEY IDENTITY,
	SwitchId INT FOREIGN KEY REFERENCES FrameworkSwitch(Id) NOT NULL,
  	Name NVARCHAR(256) NOT NULL,
  	Text NVARCHAR(256),
	INDEX IX_FrameworkLanguageName UNIQUE (SwitchId, Name)
)

CREATE TABLE FrameworkUser
(
	Id INT PRIMARY KEY IDENTITY,
	SwitchId INT FOREIGN KEY REFERENCES FrameworkSwitch(Id) NOT NULL,
  	Name NVARCHAR(256) NOT NULL, /* User name or email */
	Password NVARCHAR(256) NOT NULL,
	LanguageId INT FOREIGN KEY REFERENCES FrameworkLanguage(Id) NOT NULL, /* Default language. See also FrameworkSession.LanguageId */
	Confirmation UNIQUEIDENTIFIER,
	IsConfirmation BIT,
	IsActive BIT,
	INDEX IX_FrameworkUser UNIQUE (SwitchId, Name)
)

CREATE TABLE FrameworkColumn /* Used for configuration. Contains all in source code defined columns. Also calculated fields. */
(
	Id INT PRIMARY KEY IDENTITY,
	TableName NVARCHAR(256) NOT NULL,
	FieldName NVARCHAR(256) NOT NULL,
)

CREATE TABLE FrameworkRole /* For example Admin, DataRead */
(
	Id INT PRIMARY KEY IDENTITY,
	SwitchId INT FOREIGN KEY REFERENCES FrameworkSwitch(Id) NOT NULL,
  	Name NVARCHAR(256) NOT NULL,
	INDEX IX_FrameworkRole UNIQUE (Name, SwitchId)
)

CREATE TABLE FrameworkUserRole
(
	Id INT PRIMARY KEY IDENTITY,
	UserId INT FOREIGN KEY REFERENCES FrameworkUser(Id) NOT NULL,
	RoleId INT FOREIGN KEY REFERENCES FrameworkRole(Id) NOT NULL,
	INDEX IX_FrameworkUserRole UNIQUE (UserId, RoleId)
)

CREATE TABLE FrameworkSession
(
	Id INT PRIMARY KEY IDENTITY,
  	Name UNIQUEIDENTIFIER NOT NULL UNIQUE,
	LanguageId INT FOREIGN KEY REFERENCES FrameworkLanguage(Id) NOT NULL, /* User interface language */
	UserId INT FOREIGN KEY REFERENCES FrameworkUser(Id) /* Link to user when logged in. */
)

GO

CREATE VIEW FrameworkSwitchView
AS
SELECT
	Switch.Id AS SwitchId,
	Application.Id AS ApplicationId,
	Application.Name AS ApplicationName,
	Language.Id AS LanguageId,
	Language.Name AS LanguageName

FROM
	FrameworkSwitch Switch

LEFT JOIN
	FrameworkApplication Application ON Application.Id = Switch.ApplicationId

LEFT JOIN
	FrameworkLanguage Language ON Language.Id = Switch.LanguageId

GO

CREATE VIEW FrameworkFileStorageView
AS
SELECT
	Switch.Id AS SwitchId,
	SwitchView.ApplicationId AS SwitchApplicationId,
	SwitchView.ApplicationName AS SwitchApplicationName,
	SwitchView.LanguageId AS SwitchLanguageId,
	SwitchView.LanguageName AS SwitchLanguageName,
	FileStorage2.Id AS FileStorageId,
	FileStorage.FileName,
	FileStorage2.FileNameUpload,
	FileStorage2.Data,
	FileStorage2.IsDelete

FROM
	FrameworkSwitch Switch
	CROSS JOIN FrameworkFileStorage FileStorage
	OUTER APPLY
		(
			SELECT TOP 1 
				FileStorage2.*
			FROM
				FrameworkFileStorage FileStorage2,
				FrameworkSwitchPath SwitchPath2
			WHERE
				FileStorage2.FileName = FileStorage.FileName AND
				SwitchPath2.SwitchId = Switch.Id AND
				FileStorage2.SwitchId = SwitchPath2.ContainSwitchId

			ORDER BY
				SwitchPath2.Level
			-- FOR XML AUTO
		) AS FileStorage2

LEFT JOIN
	FrameworkSwitchView SwitchView ON (SwitchView.SwitchId = Switch.Id)

WHERE
	FileStorage2.Id IS NOT NULL

GROUP BY
	Switch.Id,
	SwitchView.ApplicationId,
	SwitchView.ApplicationName,
	SwitchView.LanguageId,
	SwitchView.LanguageName,
	FileStorage2.Id,
	FileStorage.FileName,
	FileStorage2.FileNameUpload,
	FileStorage2.Data,
	FileStorage2.IsDelete

GO

CREATE VIEW FrameworkLanguageView
AS
SELECT
	Switch.Id AS SwitchId,
	SwitchView.ApplicationId AS SwitchApplicationId,
	SwitchView.ApplicationName AS SwitchApplicationName,
	SwitchView.LanguageId AS SwitchLanguageId,
	SwitchView.LanguageName AS SwitchLanguageName,
	Language2.Id AS LanguageId,
	Language2.ParentId,
	Language.Name

FROM
	FrameworkSwitch Switch
	CROSS JOIN FrameworkLanguage Language
	OUTER APPLY
		(
			SELECT TOP 1 
				Language2.*
			FROM
				FrameworkLanguage Language2,
				FrameworkSwitchPath SwitchPath2
			WHERE
				Language2.Name = Language.Name AND
				SwitchPath2.SwitchId = Switch.Id AND
				Language2.SwitchId = SwitchPath2.ContainSwitchId

			ORDER BY
				SwitchPath2.Level
			-- FOR XML AUTO
		) AS Language2

LEFT JOIN
	FrameworkSwitchView SwitchView ON (SwitchView.SwitchId = Switch.Id)

WHERE
	Language2.Id IS NOT NULL

GROUP BY
	Switch.Id,
	SwitchView.ApplicationId,
	SwitchView.ApplicationName,
	SwitchView.LanguageId,
	SwitchView.LanguageName,
	Language2.Id,
	Language2.ParentId,
	Language.Name

GO

CREATE VIEW FrameworkTextView
AS
SELECT
	Switch.Id AS SwitchId,
	SwitchView.ApplicationId AS SwitchApplicationId,
	SwitchView.ApplicationName AS SwitchApplicationName,
	SwitchView.LanguageId AS SwitchLanguageId,
	SwitchView.LanguageName AS SwitchLanguageName,
	Text2.Id AS TextId,
	Text.Name

FROM
	FrameworkSwitch Switch
	CROSS JOIN FrameworkText Text
	OUTER APPLY
		(
			SELECT TOP 1 
				Text2.*
			FROM
				FrameworkText Text2,
				FrameworkSwitchPath SwitchPath2
			WHERE
				Text2.Name = Text.Name AND
				SwitchPath2.SwitchId = Switch.Id AND
				Text2.SwitchId = SwitchPath2.ContainSwitchId

			ORDER BY
				SwitchPath2.Level
			-- FOR XML AUTO
		) AS Text2

LEFT JOIN
	FrameworkSwitchView SwitchView ON (SwitchView.SwitchId = Switch.Id)

WHERE
	Text2.Id IS NOT NULL

GROUP BY
	Switch.Id,
	SwitchView.ApplicationId,
	SwitchView.ApplicationName,
	SwitchView.LanguageId,
	SwitchView.LanguageName,
	Text2.Id,
	Text.Name

GO

INSERT INTO FrameworkApplication (Name)
SELECT 'PTC' AS Name
UNION ALL
SELECT 'PTC CH' AS Name
UNION ALL
SELECT 'PTC D' AS Name
UNION ALL
SELECT 'LPN' AS Name

INSERT INTO FrameworkSwitch (ApplicationId)
SELECT Id FROM FrameworkApplication WHERE Name = 'PTC'
UNION ALL
SELECT Id FROM FrameworkApplication WHERE Name = 'PTC CH'
UNION ALL
SELECT Id FROM FrameworkApplication WHERE Name = 'PTC D'
UNION ALL
SELECT Id FROM FrameworkApplication WHERE Name = 'LPN'

INSERT INTO FrameworkFileStorage (SwitchId, FileName, FileNameUpload)
SELECT (SELECT SwitchId FROM FrameworkSwitchView WHERE ApplicationName = 'PTC'), 'Outline A', 'OutlineA.docx'
UNION ALL
SELECT (SELECT SwitchId FROM FrameworkSwitchView WHERE ApplicationName = 'PTC'), 'Outline B', 'OutlineB.docx'
UNION ALL
SELECT (SELECT SwitchId FROM FrameworkSwitchView WHERE ApplicationName = 'PTC CH'), 'Outline D', 'OutlineD.docx'
UNION ALL
SELECT (SELECT SwitchId FROM FrameworkSwitchView WHERE ApplicationName = 'PTC CH'), 'Outline B', 'OutlineB2.docx'

INSERT INTO FrameworkSwitchPath (SwitchId, ContainSwitchId, Level)
SELECT (SELECT SwitchId FROM FrameworkSwitchView WHERE ApplicationName = 'PTC'), (SELECT SwitchId FROM FrameworkSwitchView WHERE ApplicationName = 'PTC'), 1
UNION ALL
SELECT (SELECT SwitchId FROM FrameworkSwitchView WHERE ApplicationName = 'PTC CH'), (SELECT SwitchId FROM FrameworkSwitchView WHERE ApplicationName = 'PTC CH'), 1
UNION ALL
SELECT (SELECT SwitchId FROM FrameworkSwitchView WHERE ApplicationName = 'PTC CH'), (SELECT SwitchId FROM FrameworkSwitchView WHERE ApplicationName = 'PTC'), 2
UNION ALL
SELECT (SELECT SwitchId FROM FrameworkSwitchView WHERE ApplicationName = 'PTC D'), (SELECT SwitchId FROM FrameworkSwitchView WHERE ApplicationName = 'PTC'), 1
UNION ALL
SELECT (SELECT SwitchId FROM FrameworkSwitchView WHERE ApplicationName = 'LPN'), (SELECT SwitchId FROM FrameworkSwitchView WHERE ApplicationName = 'LPN'), 1

INSERT FrameworkLanguage (SwitchId, ParentId, Name)
SELECT (SELECT SwitchId FROM FrameworkSwitchView WHERE ApplicationName = 'PTC'), NULL, 'Default'
UNION ALL
SELECT (SELECT SwitchId FROM FrameworkSwitchView WHERE ApplicationName = 'PTC'), (SELECT Id FROM FrameworkLanguage WHERE Name = 'Default'), 'English'
UNION ALL
SELECT (SELECT SwitchId FROM FrameworkSwitchView WHERE ApplicationName = 'PTC'), (SELECT Id FROM FrameworkLanguage WHERE Name = 'English'), 'German'
UNION ALL
SELECT (SELECT SwitchId FROM FrameworkSwitchView WHERE ApplicationName = 'LPN'), NULL, 'French'

INSERT INTO FrameworkSwitch (LanguageId)
SELECT Id FROM FrameworkLanguage

GO

INSERT INTO FrameworkText (SwitchId, Name)
SELECT (SELECT SwitchId FROM FrameworkLanguageView WHERE SwitchApplicationName = 'LPN' AND Name='French'), 'Connecter'
UNION ALL
SELECT (SELECT SwitchId FROM FrameworkLanguageView WHERE SwitchApplicationName = 'PTC' AND Name='Default'), 'Login'
UNION ALL
SELECT (SELECT SwitchId FROM FrameworkLanguageView WHERE SwitchApplicationName = 'PTC' AND Name='German'), 'Anmelden'

GO

INSERT INTO FrameworkSwitchPath (SwitchId, ContainSwitchId, Level)
SELECT (SELECT SwitchId FROM FrameworkSwitchView WHERE ApplicationName = 'LPN'), (SELECT SwitchId FROM FrameworkSwitchView WHERE LanguageName = 'French'), 1
