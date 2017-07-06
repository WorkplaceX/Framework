IF (NOT ISNULL((SELECT Version FROM FrameworkVersion), '') = 'v1.01') RETURN  -- Version Check

CREATE TABLE FrameworkFileStorage
(
	Id INT PRIMARY KEY IDENTITY,
	ConfigurationId INT FOREIGN KEY REFERENCES FrameworkConfiguration(Id) NOT NULL,
  	FileName NVARCHAR(256) NOT NULL,
  	FileNameUpload NVARCHAR(256),
	Data VARBINARY(MAX),
	IsDelete BIT,
	INDEX IX_FrameworkConfigurationTree UNIQUE (ConfigurationId, FileName)
)

GO

CREATE VIEW FrameworkFileStorageView
AS
SELECT
	Configuration.Id AS ConfigurationId,
	ConfigurationView.ApplicationId AS ConfigurationApplicationId,
	ConfigurationView.ApplicationName AS ConfigurationApplicationName,
	ConfigurationView.LanguageId AS ConfigurationLanguageId,
	ConfigurationView.LanguageName AS ConfigurationLanguageName,
	FileStorage2.Id AS FileStorageId,
	FileStorage.FileName,
	FileStorage2.FileNameUpload,
	FileStorage2.Data,
	FileStorage2.IsDelete

FROM
	FrameworkConfiguration Configuration
	CROSS JOIN FrameworkFileStorage FileStorage
	OUTER APPLY
		(
			SELECT TOP 1 
				FileStorage2.*
			FROM
				FrameworkFileStorage FileStorage2,
				FrameworkConfigurationPath ConfigurationPath2
			WHERE
				FileStorage2.FileName = FileStorage.FileName AND
				ConfigurationPath2.ConfigurationId = Configuration.Id AND
				FileStorage2.ConfigurationId = ConfigurationPath2.ContainConfigurationId

			ORDER BY
				ConfigurationPath2.Level
			-- FOR XML AUTO
		) AS FileStorage2

LEFT JOIN
	FrameworkConfigurationView ConfigurationView ON (ConfigurationView.ConfigurationId = Configuration.Id)

WHERE
	FileStorage2.Id IS NOT NULL

GROUP BY
	Configuration.Id,
	ConfigurationView.ApplicationId,
	ConfigurationView.ApplicationName,
	ConfigurationView.LanguageId,
	ConfigurationView.LanguageName,
	FileStorage2.Id,
	FileStorage.FileName,
	FileStorage2.FileNameUpload,
	FileStorage2.Data,
	FileStorage2.IsDelete
