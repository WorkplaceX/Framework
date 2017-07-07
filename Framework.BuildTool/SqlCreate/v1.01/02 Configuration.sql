IF (NOT ISNULL((SELECT Version FROM FrameworkVersion), '') = 'v1.01') RETURN  -- Version Check

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

CREATE VIEW FrameworkLanguageView
AS
SELECT
	Configuration.Id AS ConfigurationId,
	Language2.Id AS LanguageId,
	Language.Name,
	Language2.ConfigurationId AS ConfigurationIdSource,
	Language2.Level AS Level,
	Language2.ApplicationId2,
	Language2.LanguageId2,
	Language2.UserId,
	Language2.SessionId,
	ConfigurationLanguage.ConfigurationId AS ConfigurationIdLanguage,
	ConfigurationView.ApplicationId,
	ConfigurationView.ApplicationName,
	ConfigurationView.Debug AS ConfigurationDebug,
	ConfigurationViewSource.Debug AS ConfigurationSourceDebug

FROM
	FrameworkConfiguration Configuration
	CROSS JOIN FrameworkLanguage Language
	OUTER APPLY
		(
			SELECT TOP 1 
				Language2.*,
				ConfigurationPath2.Level,
				ConfigurationPath2.ApplicationId AS ApplicationId2,
				ConfigurationPath2.LanguageId AS LanguageId2,
				ConfigurationPath2.UserId,
				ConfigurationPath2.SessionId
			FROM
				FrameworkLanguage Language2,
				FrameworkConfigurationPath ConfigurationPath2
			WHERE
				Language2.Name = Language.Name AND
				ConfigurationPath2.ConfigurationId = Configuration.Id AND
				Language2.ConfigurationId = ConfigurationPath2.ConfigurationIdContain

			ORDER BY
				ConfigurationPath2.Level desc
		) AS Language2

LEFT JOIN
	FrameworkConfigurationView ConfigurationView ON (ConfigurationView.ConfigurationId = Configuration.Id)

LEFT JOIN
	FrameworkConfigurationView ConfigurationViewSource ON (ConfigurationViewSource.ConfigurationId = Language2.ConfigurationId)

LEFT JOIN
	FrameworkConfigurationView ConfigurationLanguage ON (ConfigurationLanguage.LanguageId = Language2.Id)

WHERE
	Language2.Id IS NOT NULL

GROUP BY
	Configuration.Id,
	Language2.Id,
	Language.Name,
	Language2.ConfigurationId,
	Language2.Level,
	Language2.ApplicationId2,
	Language2.LanguageId2,
	Language2.UserId,
	Language2.SessionId,
	ConfigurationLanguage.ConfigurationId,
	ConfigurationView.ApplicationId,
	ConfigurationView.ApplicationName,
	ConfigurationView.Debug,
	ConfigurationViewSource.Debug

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