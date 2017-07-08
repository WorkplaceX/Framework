IF (NOT ISNULL((SELECT Version FROM FrameworkVersion), '') = 'v1.01') RETURN  -- Version Check

/* ApplicationType */
INSERT INTO FrameworkApplicationType (Name)
SELECT 'Framework' AS Name
UNION ALL
SELECT 'PTC' AS Name
UNION ALL
SELECT 'LPN' AS Name

/* Application */
INSERT INTO FrameworkApplication (Name, ApplicationTypeId, Domain)
SELECT 'Framework' AS Name, (SELECT Id FROM FrameworkApplicationType WHERE Name = 'Framework') AS ApplicationTypeId, 'framework' AS Domain
INSERT INTO FrameworkApplication (Name, ApplicationTypeId, Domain)
SELECT 'PTC' AS Name, (SELECT Id FROM FrameworkApplicationType WHERE Name = 'PTC'), 'ptc' AS Domain
INSERT INTO FrameworkApplication (ParentId, Name, ApplicationTypeId, Domain)
SELECT (SELECT Id From FrameworkApplication WHERE Name = 'PTC'), 'PTC CH' AS Name, (SELECT Id FROM FrameworkApplicationType WHERE Name = 'PTC'), 'ptcch'
INSERT INTO FrameworkApplication (ParentId, Name, ApplicationTypeId, Domain)
SELECT (SELECT Id From FrameworkApplication WHERE Name = 'PTC CH'), 'PTC D' AS Name, (SELECT Id FROM FrameworkApplicationType WHERE Name = 'PTC'), 'ptcd'
INSERT INTO FrameworkApplication (Name, ApplicationTypeId, Domain)
SELECT 'LPN' AS Name, (SELECT Id FROM FrameworkApplicationType WHERE Name = 'LPN'), 'lpn'

GO
EXEC FrameworkConfigurationPathUpdate
GO

/* FileStorage */
INSERT INTO FrameworkFileStorage (ConfigurationId, Name, FileNameUpload)
SELECT (SELECT ConfigurationId FROM FrameworkConfigurationView WHERE ApplicationName = 'PTC'), 'Outline A', 'OutlineA.docx'
UNION ALL
SELECT (SELECT ConfigurationId FROM FrameworkConfigurationView WHERE ApplicationName = 'PTC'), 'Outline B', 'OutlineB.docx'
UNION ALL
SELECT (SELECT ConfigurationId FROM FrameworkConfigurationView WHERE ApplicationName = 'PTC CH'), 'Outline D', 'OutlineD.docx'
UNION ALL
SELECT (SELECT ConfigurationId FROM FrameworkConfigurationView WHERE ApplicationName = 'PTC CH'), 'Outline B', 'OutlineB2.docx'

/* Language */
INSERT FrameworkLanguage (ConfigurationId, Name)
SELECT (SELECT ConfigurationId FROM FrameworkConfigurationView WHERE ApplicationName = 'PTC'), 'Default'
INSERT FrameworkLanguage (ConfigurationId, ParentId, Name)
SELECT (SELECT ConfigurationId FROM FrameworkConfigurationView WHERE ApplicationName = 'PTC'), (SELECT Id FROM FrameworkLanguage WHERE ConfigurationId = (SELECT ConfigurationId FROM FrameworkConfigurationView WHERE ApplicationName = 'PTC') AND Name = 'Default'), 'English'
INSERT FrameworkLanguage (ConfigurationId, ParentId, Name)
SELECT (SELECT ConfigurationId FROM FrameworkConfigurationView WHERE ApplicationName = 'PTC'), (SELECT Id FROM FrameworkLanguage WHERE ConfigurationId = (SELECT ConfigurationId FROM FrameworkConfigurationView WHERE ApplicationName = 'PTC') AND Name = 'English'), 'German'
INSERT FrameworkLanguage (ConfigurationId, ParentId, Name)
SELECT (SELECT ConfigurationId FROM FrameworkConfigurationView WHERE ApplicationName = 'PTC CH'), (SELECT Id FROM FrameworkLanguage WHERE ConfigurationId = (SELECT ConfigurationId FROM FrameworkConfigurationView WHERE ApplicationName = 'PTC') AND Name = 'English'), 'Italian'
INSERT FrameworkLanguage (ConfigurationId, Name)
SELECT (SELECT ConfigurationId FROM FrameworkConfigurationView WHERE ApplicationName = 'LPN'), 'French'

/* User */
INSERT INTO FrameworkUser (ApplicationId, Name, LanguageId)
SELECT (SELECT Id AS ApplicationId FROM FrameworkApplication WHERE Name = 'Framework') AS ApplicationId,  'Admin' AS Name, NULL
UNION ALL
SELECT (SELECT Id AS ApplicationId FROM FrameworkApplication WHERE Name = 'LPN') AS ApplicationId,  'John' AS Name, NULL
UNION ALL
SELECT (SELECT Id AS ApplicationId FROM FrameworkApplication WHERE Name = 'PTC') AS ApplicationId, 'Hnc' AS Name, NULL

/* Role */
INSERT INTO FrameworkRole (ApplicationTypeId, Name)
SELECT (SELECT Id FROM FrameworkApplication WHERE Name = 'Framework'), 'Admin'
UNION ALL
SELECT (SELECT Id FROM FrameworkApplication WHERE Name = 'PTC'), 'Coordy'
UNION ALL
SELECT (SELECT Id FROM FrameworkApplication WHERE Name = 'PTC'), 'Spky'

GO
EXEC FrameworkConfigurationPathUpdate
GO

/* Add User */
INSERT INTO FrameworkUser (ApplicationId, Name, LanguageId)
SELECT (SELECT Id AS ApplicationId FROM FrameworkApplication WHERE Name = 'PTC CH') AS ApplicationId, 'Hnc2' AS Name, (SELECT TOP 1 LanguageLanguageId FROM FrameworkLanguageView WHERE ApplicationName = 'PTC CH' AND Name = 'Italian')
INSERT INTO FrameworkSession (Name, ApplicationId, ApplicationTypeId, UserId, LanguageId)
SELECT NEWID(), (SELECT Id FROM FrameworkApplication WHERE Name = 'PTC CH'), (SELECT ApplicationTypeId FROM FrameworkApplication WHERE Name = 'PTC CH'), (SELECT Id FROM FrameworkUser WHERE ApplicationId = (SELECT Id FROM FrameworkApplication WHERE Name = 'PTC CH') AND Name = 'hnc2'), (SELECT LanguageId FROM FrameworkUser WHERE ApplicationId = (SELECT Id FROM FrameworkApplication WHERE Name = 'PTC CH') AND Name = 'hnc2')
INSERT INTO FrameworkConfiguration (UserId)
SELECT UserId FROM FrameworkSession
EXCEPT SELECT UserId FROM FrameworkConfiguration
UPDATE Session
SET Session.ConfigurationId = (SELECT Id FROM FrameworkConfiguration WHERE UserId = Session.UserId)
FROM FrameworkSession Session
WHERE Session.UserId IS NOT NULL AND Session.ConfigurationId IS NULL

GO
EXEC FrameworkConfigurationPathUpdate
GO

/* Add Text */
INSERT INTO FrameworkText (ConfigurationId, Name)
SELECT (SELECT TOP 1 ConfigurationIdLanguage FROM FrameworkLanguageView WHERE ApplicationName = 'LPN' AND Name = 'French'), 'Connecter' 
UNION ALL
SELECT (SELECT TOP 1 ConfigurationIdLanguage FROM FrameworkLanguageView WHERE ApplicationName = 'PTC' AND Name = 'Default'), 'Login' 
UNION ALL
SELECT (SELECT TOP 1 ConfigurationIdLanguage FROM FrameworkLanguageView WHERE ApplicationName = 'PTC' AND Name = 'German'), 'Anmelden' 

GO

EXEC FrameworkConfigurationPathUpdate

GO