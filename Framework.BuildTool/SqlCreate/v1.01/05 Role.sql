IF (NOT ISNULL((SELECT Version FROM FrameworkVersion), '') = 'v1.01') RETURN  -- Version Check

CREATE TABLE FrameworkRole /* For example Admin, DataRead */
(
	Id INT PRIMARY KEY IDENTITY,
	ConfigurationId INT FOREIGN KEY REFERENCES FrameworkConfiguration(Id) NOT NULL,
  	Name NVARCHAR(256) NOT NULL,
	INDEX IX_FrameworkRole UNIQUE (Name, ConfigurationId)
)

CREATE TABLE FrameworkUserRole
(
	Id INT PRIMARY KEY IDENTITY,
	UserId INT FOREIGN KEY REFERENCES FrameworkUser(Id) NOT NULL,
	RoleId INT FOREIGN KEY REFERENCES FrameworkRole(Id) NOT NULL,
	INDEX IX_FrameworkUserRole UNIQUE (UserId, RoleId)
)


