IF (NOT ISNULL((SELECT Version FROM FrameworkVersion), '') = 'v1.01') RETURN  -- Version Check

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
	Configuration.Id AS ConfigurationId,
	Text2.Id AS TextId,
	Text.Name,
	Text2.ConfigurationId AS ConfigurationIdSource,
	Text2.Level AS Level,
	ConfigurationView.ApplicationId,
	ConfigurationView.ApplicationName,
	ConfigurationView.LanguageId,
	ConfigurationView.LanguageName,
	ConfigurationView.Debug AS ConfigurationDebug,
	ConfigurationViewSource.Debug AS ConfigurationSourceDebug

FROM
	FrameworkConfiguration Configuration
	CROSS JOIN FrameworkText Text
	OUTER APPLY
		(
			SELECT TOP 1 
				Text2.*,
				ConfigurationPath2.Level
			FROM
				FrameworkText Text2,
				FrameworkConfigurationPath ConfigurationPath2
			WHERE
				Text2.Name = Text.Name AND
				ConfigurationPath2.ConfigurationId = Configuration.Id AND
				Text2.ConfigurationId = ConfigurationPath2.ConfigurationIdContain

			ORDER BY
				ConfigurationPath2.Level
		) AS Text2

LEFT JOIN
	FrameworkConfigurationView ConfigurationView ON (ConfigurationView.ConfigurationId = Configuration.Id)

LEFT JOIN
	FrameworkConfigurationView ConfigurationViewSource ON (ConfigurationViewSource.ConfigurationId = Text2.ConfigurationId)

WHERE
	Text2.Id IS NOT NULL

GROUP BY
	Configuration.Id,
	Text2.Id,
	Text.Name,
	Text2.ConfigurationId,
	Text2.Level,
	ConfigurationView.ApplicationId,
	ConfigurationView.ApplicationName,
	ConfigurationView.LanguageId,
	ConfigurationView.LanguageName,
	ConfigurationView.Debug,
	ConfigurationViewSource.Debug
