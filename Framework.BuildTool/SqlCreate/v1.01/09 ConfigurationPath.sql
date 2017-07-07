/* Every Application, Language and User with Session gets it's Configuration layer */
INSERT INTO FrameworkConfiguration (ApplicationId)
SELECT Id FROM FrameworkApplication
EXCEPT SELECT ApplicationId FROM FrameworkConfiguration

INSERT INTO FrameworkConfiguration (LanguageId)
SELECT Id FROM FrameworkLanguage
EXCEPT SELECT LanguageId FROM FrameworkConfiguration

INSERT INTO FrameworkConfiguration (UserId)
SELECT UserId FROM FrameworkSession
EXCEPT SELECT UserId FROM FrameworkConfiguration

/* Path for Application, Language */
INSERT INTO FrameworkConfigurationPath (ApplicationId, ConfigurationId, ConfigurationIdContain, Level)
SELECT ApplicationId, ConfigurationId, ConfigurationIdContain, Level FROM FrameworkApplicationHierarchy

INSERT INTO FrameworkConfigurationPath (LanguageId, ConfigurationId, ConfigurationIdContain, Level)
SELECT LanguageId, ConfigurationId, ConfigurationIdContain, Level FROM FrameworkLanguageHierarchy

/* Add User */
INSERT INTO FrameworkUser (ApplicationId, Name, LanguageId)
SELECT (SELECT Id AS ApplicationId FROM FrameworkApplication WHERE Name = 'PTC CH') AS ApplicationId, 'Hnc2' AS Name, (SELECT LanguageId FROM FrameworkLanguageView WHERE ApplicationName = 'PTC CH' AND Name = 'Italian')
INSERT INTO FrameworkSession (Name, ApplicationId, ApplicationTypeId, UserId, LanguageId)
SELECT NEWID(), (SELECT Id FROM FrameworkApplication WHERE Name = 'PTC CH'), (SELECT ApplicationTypeId FROM FrameworkApplication WHERE Name = 'PTC CH'), (SELECT Id FROM FrameworkUser WHERE ApplicationId = (SELECT Id FROM FrameworkApplication WHERE Name = 'PTC CH') AND Name = 'hnc2'), (SELECT LanguageId FROM FrameworkUser WHERE ApplicationId = (SELECT Id FROM FrameworkApplication WHERE Name = 'PTC CH') AND Name = 'hnc2')
INSERT INTO FrameworkConfiguration (UserId)
SELECT UserId FROM FrameworkSession
EXCEPT SELECT UserId FROM FrameworkConfiguration
UPDATE Session
SET Session.ConfigurationId = (SELECT Id FROM FrameworkConfiguration WHERE UserId = Session.UserId)
FROM FrameworkSession Session
WHERE Session.UserId IS NOT NULL AND Session.ConfigurationId IS NULL

/* Add Text */
INSERT INTO FrameworkText (ConfigurationId, Name)
SELECT (SELECT ConfigurationIdLanguage FROM FrameworkLanguageView WHERE ApplicationName = 'LPN' AND Name = 'French'), 'Connecter' 
UNION ALL
SELECT (SELECT ConfigurationIdLanguage FROM FrameworkLanguageView WHERE ApplicationName = 'PTC' AND Name = 'Default'), 'Login' 
UNION ALL
SELECT (SELECT ConfigurationIdLanguage FROM FrameworkLanguageView WHERE ApplicationName = 'PTC' AND Name = 'German'), 'Anmelden' 

/* Configuration Path */
INSERT INTO FrameworkConfigurationPath (ApplicationId, LanguageId, UserId, SessionId, ConfigurationId, ConfigurationIdContain, Level)
SELECT
	Session.ApplicationId, Session.LanguageId, 
	Session.UserId,
	Session.Id,
	Application.ConfigurationId,
	Application.ConfigurationIdContain,
	Application.Level
FROM
	FrameworkSession Session
LEFT JOIN
	FrameworkApplicationHierarchy Application ON Application.ApplicationId = Session.ApplicationId
UNION ALL
SELECT
	Session.ApplicationId, Session.LanguageId, 
	Session.UserId,
	Session.Id,
	Language.ConfigurationId,
	Language.ConfigurationIdContain,
	Language.Level + 1000
FROM
	FrameworkSession Session
JOIN
	FrameworkLanguageHierarchy Language ON Language.LanguageId = Session.LanguageId
UNION ALL
SELECT
	Session.ApplicationId, Session.LanguageId, 
	Session.UserId,
	Session.Id,
	Session.ConfigurationId,
	Session.ConfigurationId AS ConfigurationIdContain,
	2000 AS Level
FROM
	FrameworkSession Session
WHERE
	Session.ConfigurationId IS NOT NULL

GO

CREATE PROCEDURE FrameworkConfigurationPathUpdate
AS
DELETE FrameworkConfigurationPath

INSERT INTO FrameworkConfiguration (ApplicationId)
SELECT Id FROM FrameworkApplication
EXCEPT SELECT ApplicationId FROM FrameworkConfiguration

INSERT INTO FrameworkConfiguration (LanguageId)
SELECT Id FROM FrameworkLanguage
EXCEPT SELECT LanguageId FROM FrameworkConfiguration

INSERT INTO FrameworkConfiguration (UserId)
SELECT UserId FROM FrameworkSession
EXCEPT SELECT UserId FROM FrameworkConfiguration

INSERT INTO FrameworkConfigurationPath (ApplicationId, LanguageId, UserId, SessionId, ConfigurationId, ConfigurationIdContain, Level)
SELECT
	Session.ApplicationId, Session.LanguageId, 
	Session.UserId,
	Session.Id AS SessionId,
	Application.ConfigurationId,
	Application.ConfigurationIdContain,
	Application.Level
FROM
	FrameworkSession Session
LEFT JOIN
	FrameworkApplicationHierarchy Application ON Application.ApplicationId = Session.ApplicationId
UNION ALL
SELECT
	Session.ApplicationId, Session.LanguageId, 
	Session.UserId,
	Session.Id,
	Language.ConfigurationId,
	Language.ConfigurationIdContain,
	Language.Level + 1000
FROM
	FrameworkSession Session
JOIN
	FrameworkLanguageHierarchy Language ON Language.LanguageId = Session.LanguageId
UNION ALL
SELECT
	Session.ApplicationId, Session.LanguageId, 
	Session.UserId,
	Session.Id,
	Session.ConfigurationId,
	Session.ConfigurationId AS ConfigurationIdContain,
	2000 AS Level
FROM
	FrameworkSession Session
WHERE
	Session.ConfigurationId IS NOT NULL
