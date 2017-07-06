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
	ConfigurationView.ApplicationId AS ConfigurationApplicationId,
	ConfigurationView.ApplicationName AS ConfigurationApplicationName,
	ConfigurationView.LanguageId AS ConfigurationLanguageId,
	ConfigurationView.LanguageName AS ConfigurationLanguageName,
	Text2.Id AS TextId,
	Text.Name

FROM
	FrameworkConfiguration Configuration
	CROSS JOIN FrameworkText Text
	OUTER APPLY
		(
			SELECT TOP 1 
				Text2.*
			FROM
				FrameworkText Text2,
				FrameworkConfigurationPath ConfigurationPath2
			WHERE
				Text2.Name = Text.Name AND
				ConfigurationPath2.ConfigurationId = Configuration.Id AND
				Text2.ConfigurationId = ConfigurationPath2.ContainConfigurationId

			ORDER BY
				ConfigurationPath2.Level
			-- FOR XML AUTO
		) AS Text2

LEFT JOIN
	FrameworkConfigurationView ConfigurationView ON (ConfigurationView.ConfigurationId = Configuration.Id)

WHERE
	Text2.Id IS NOT NULL

GROUP BY
	Configuration.Id,
	ConfigurationView.ApplicationId,
	ConfigurationView.ApplicationName,
	ConfigurationView.LanguageId,
	ConfigurationView.LanguageName,
	Text2.Id,
	Text.Name

GO

