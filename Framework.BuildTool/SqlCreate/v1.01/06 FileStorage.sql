IF (NOT ISNULL((SELECT Version FROM FrameworkVersion), '') = 'v1.01') RETURN  -- Version Check

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