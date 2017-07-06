IF (NOT ISNULL((SELECT Version FROM FrameworkVersion), '') = 'v1.01') RETURN  -- Version Check

CREATE TABLE FrameworkApplication
(
	Id INT PRIMARY KEY IDENTITY,
  	Name NVARCHAR(256) NOT NULL UNIQUE
)

CREATE TABLE FrameworkConfiguration
(
	Id INT PRIMARY KEY IDENTITY,
	ApplicationId INT FOREIGN KEY REFERENCES FrameworkApplication(Id),
	LanguageId INT,
	UserId INT
)

CREATE TABLE FrameworkConfigurationTree
(
	Id INT PRIMARY KEY IDENTITY,
	ConfigurationId INT FOREIGN KEY REFERENCES FrameworkConfiguration(Id) NOT NULL,
	ParentId INT FOREIGN KEY REFERENCES FrameworkConfigurationTree(Id) NOT NULL,
)

CREATE TABLE FrameworkConfigurationPath
(
	Id INT PRIMARY KEY IDENTITY,
	ConfigurationId INT FOREIGN KEY REFERENCES FrameworkConfiguration(Id) NOT NULL,
	ContainConfigurationId INT FOREIGN KEY REFERENCES FrameworkConfiguration(Id) NOT NULL,
	Level INT NOT NULL,
	INDEX IX_FrameworkConfigurationTree UNIQUE (ConfigurationId, ContainConfigurationId)
)

CREATE TABLE FrameworkLanguage /* For example English, German */
(
	Id INT PRIMARY KEY IDENTITY,
	ConfigurationId INT FOREIGN KEY REFERENCES FrameworkConfiguration(Id) NOT NULL,
	ParentId INT FOREIGN KEY REFERENCES FrameworkLanguage(Id), -- Fallback if language does not exist.
  	Name NVARCHAR(256) NOT NULL,
	INDEX IX_FrameworkLanguageName UNIQUE (ConfigurationId, Name)
)
ALTER TABLE FrameworkConfiguration ADD	CONSTRAINT FK_FrameworkConfiguration_LanguageId FOREIGN KEY (LanguageId) REFERENCES FrameworkLanguage(Id)

CREATE TABLE FrameworkUser
(
	Id INT PRIMARY KEY IDENTITY,
	ConfigurationId INT FOREIGN KEY REFERENCES FrameworkConfiguration(Id) NOT NULL,
  	Name NVARCHAR(256) NOT NULL, /* User name or email */
	Password NVARCHAR(256) NOT NULL,
	LanguageId INT FOREIGN KEY REFERENCES FrameworkLanguage(Id) NOT NULL, /* Default language. See also FrameworkSession.LanguageId */
	Confirmation UNIQUEIDENTIFIER,
	IsConfirmation BIT,
	IsActive BIT,
	INDEX IX_FrameworkUser UNIQUE (ConfigurationId, Name)
)
ALTER TABLE FrameworkConfiguration ADD	CONSTRAINT FK_FrameworkConfiguration_UserId FOREIGN KEY (UserId) REFERENCES FrameworkUser(Id)

GO

CREATE VIEW FrameworkConfigurationView
AS
SELECT
	Configuration.Id AS ConfigurationId,
	Application.Id AS ApplicationId,
	Application.Name AS ApplicationName,
	Language.Id AS LanguageId,
	Language.Name AS LanguageName

FROM
	FrameworkConfiguration Configuration

LEFT JOIN
	FrameworkApplication Application ON Application.Id = Configuration.ApplicationId

LEFT JOIN
	FrameworkLanguage Language ON Language.Id = Configuration.LanguageId

GO

CREATE VIEW FrameworkLanguageView
AS
SELECT
	Configuration.Id AS ConfigurationId,
	ConfigurationView.ApplicationId AS ConfigurationApplicationId,
	ConfigurationView.ApplicationName AS ConfigurationApplicationName,
	ConfigurationView.LanguageId AS ConfigurationLanguageId,
	ConfigurationView.LanguageName AS ConfigurationLanguageName,
	Language2.Id AS LanguageId,
	Language2.ParentId,
	Language.Name

FROM
	FrameworkConfiguration Configuration
	CROSS JOIN FrameworkLanguage Language
	OUTER APPLY
		(
			SELECT TOP 1 
				Language2.*
			FROM
				FrameworkLanguage Language2,
				FrameworkConfigurationPath ConfigurationPath2
			WHERE
				Language2.Name = Language.Name AND
				ConfigurationPath2.ConfigurationId = Configuration.Id AND
				Language2.ConfigurationId = ConfigurationPath2.ContainConfigurationId

			ORDER BY
				ConfigurationPath2.Level
			-- FOR XML AUTO
		) AS Language2

LEFT JOIN
	FrameworkConfigurationView ConfigurationView ON (ConfigurationView.ConfigurationId = Configuration.Id)

WHERE
	Language2.Id IS NOT NULL

GROUP BY
	Configuration.Id,
	ConfigurationView.ApplicationId,
	ConfigurationView.ApplicationName,
	ConfigurationView.LanguageId,
	ConfigurationView.LanguageName,
	Language2.Id,
	Language2.ParentId,
	Language.Name
