
CREATE PROCEDURE FrameworkConfigurationPathUpdate
AS

/* Every Application and Language gets it's Configuration layer */
INSERT INTO FrameworkConfiguration (ApplicationId)
SELECT Id FROM FrameworkApplication
EXCEPT SELECT ApplicationId FROM FrameworkConfiguration

INSERT INTO FrameworkConfiguration (LanguageId)
SELECT Id FROM FrameworkLanguage
EXCEPT SELECT LanguageId FROM FrameworkConfiguration

/* Session for every language exclusive */
INSERT INTO FrameworkSession (Name, LanguageId)
SELECT NEWID(), Id FROM FrameworkLanguage Language
WHERE EXISTS 
(
	SELECT NULL AS ApplicationId, Language.Id, NULL AS UserId
	EXCEPT
	SELECT Session.ApplicationId, Session.LanguageId, Session.UserId FROM FrameworkSession Session
)

/* Session for every application exclusive */
INSERT INTO FrameworkSession (Name, ApplicationId)
SELECT NEWID(), Id FROM FrameworkApplication Application
WHERE EXISTS 
(
	SELECT Application.Id, NULL AS LanguageId, NULL AS UserId
	EXCEPT
	SELECT Session.ApplicationId, Session.LanguageId, Session.UserId FROM FrameworkSession Session
)

DELETE FrameworkConfigurationPath

INSERT INTO FrameworkConfigurationPath (ApplicationId, LanguageId, UserId, SessionId, ConfigurationId, ConfigurationIdContain, Level)
/* Application layer */
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
WHERE
	Session.ApplicationId IS NOT NULL
UNION ALL
/* Language layer */
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
/* User layer */
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

