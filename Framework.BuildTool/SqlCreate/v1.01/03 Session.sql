IF (NOT ISNULL((SELECT Version FROM FrameworkVersion), '') = 'v1.01') RETURN  -- Version Check

CREATE TABLE FrameworkSession
(
	Id INT PRIMARY KEY IDENTITY,
  	Name UNIQUEIDENTIFIER NOT NULL UNIQUE,
	LanguageId INT FOREIGN KEY REFERENCES FrameworkLanguage(Id) NOT NULL, /* User interface language */
	UserId INT FOREIGN KEY REFERENCES FrameworkUser(Id) /* Link to user when logged in. */
)
