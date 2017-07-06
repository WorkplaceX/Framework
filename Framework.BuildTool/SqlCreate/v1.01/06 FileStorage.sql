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
	Configuration.Id AS ConfigurationId,
	FileStorage2.Id AS FileStorageId,
	FileStorage.Name,
	FileStorage2.FileNameUpload,
	FileStorage2.Data,
	FileStorage2.IsDelete,
	FileStorage2.ConfigurationId AS ConfigurationIdSource,
	FileStorage2.Level AS Level,
	ConfigurationView.Debug AS ConfigurationDebug,
	ConfigurationViewSource.Debug AS ConfigurationSourceDebug

FROM
	FrameworkConfiguration Configuration
	CROSS JOIN FrameworkFileStorage FileStorage
	OUTER APPLY
		(
			SELECT TOP 1 
				FileStorage2.*,
				ConfigurationPath2.Level
			FROM
				FrameworkFileStorage FileStorage2,
				FrameworkConfigurationPath ConfigurationPath2
			WHERE
				FileStorage2.Name = FileStorage.Name AND
				ConfigurationPath2.ConfigurationId = Configuration.Id AND
				FileStorage2.ConfigurationId = ConfigurationPath2.ContainConfigurationId

			ORDER BY
				ConfigurationPath2.Level
		) AS FileStorage2

LEFT JOIN
	FrameworkConfigurationView ConfigurationView ON (ConfigurationView.ConfigurationId = Configuration.Id)

LEFT JOIN
	FrameworkConfigurationView ConfigurationViewSource ON (ConfigurationViewSource.ConfigurationId = FileStorage2.ConfigurationId)

WHERE
	FileStorage2.Id IS NOT NULL

GROUP BY
	Configuration.Id,
	FileStorage2.Id,
	FileStorage.Name,
	FileStorage2.FileNameUpload,
	FileStorage2.Data,
	FileStorage2.IsDelete,
	FileStorage2.ConfigurationId,
	FileStorage2.Level,
	ConfigurationView.Debug,
	ConfigurationViewSource.Debug
