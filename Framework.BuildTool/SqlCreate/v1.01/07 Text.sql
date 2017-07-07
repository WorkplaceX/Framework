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