IF (NOT ISNULL((SELECT Version FROM FrameworkVersion), '') = 'v1.01') RETURN  -- Version Check

CREATE TABLE FrameworkSession
(
	Id INT PRIMARY KEY IDENTITY,
  	Name UNIQUEIDENTIFIER NOT NULL UNIQUE,
	ApplicationTypeId INT FOREIGN KEY REFERENCES FrameworkApplicationType(Id),
	ApplicationId INT FOREIGN KEY REFERENCES FrameworkApplication(Id),
	LanguageId INT FOREIGN KEY REFERENCES FrameworkApplication(Id), /* User interface language for this session */
	ConfigurationId INT FOREIGN KEY REFERENCES FrameworkConfiguration(Id),
	UserId INT FOREIGN KEY REFERENCES FrameworkUser(Id) /* Link to user when logged in. */
)
