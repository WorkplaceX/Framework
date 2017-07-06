IF (NOT ISNULL((SELECT Version FROM FrameworkVersion), '') = 'v1.01') RETURN  -- Version Check

INSERT INTO FrameworkApplicationType (Name)
SELECT 'Framework' AS Name
UNION ALL
SELECT 'PTC' AS Name
UNION ALL
SELECT 'LPN' AS Name

INSERT INTO FrameworkApplication (Name, ParentId, ApplicationTypeId, Domain)
SELECT 'Framework' AS Name, NULL AS ParentId, (SELECT Id FROM FrameworkApplicationType WHERE Name = 'Framework') AS ApplicationTypeId, 'framework' AS Domain
INSERT INTO FrameworkApplication (Name, ParentId, ApplicationTypeId, Domain)
SELECT 'PTC' AS Name, NULL, (SELECT Id FROM FrameworkApplicationType WHERE Name = 'PTC'), 'ptc' AS Domain
INSERT INTO FrameworkApplication (Name, ParentId, ApplicationTypeId, Domain)
SELECT 'PTC CH' AS Name, (SELECT Id FROM FrameworkApplication WHERE Name = 'PTC'), (SELECT Id FROM FrameworkApplicationType WHERE Name = 'PTC'), 'ptcch'
INSERT INTO FrameworkApplication (Name, ParentId, ApplicationTypeId, Domain)
SELECT 'PTC D' AS Name, (SELECT Id FROM FrameworkApplication WHERE Name = 'PTC'), (SELECT Id FROM FrameworkApplicationType WHERE Name = 'PTC'), 'ptcd'
INSERT INTO FrameworkApplication (Name, ParentId, ApplicationTypeId, Domain)
SELECT 'LPN' AS Name, NULL, (SELECT Id FROM FrameworkApplicationType WHERE Name = 'LPN'), 'lpn'

INSERT INTO FrameworkConfiguration (ApplicationId)
SELECT Id FROM FrameworkApplication WHERE Name = 'PTC'
UNION ALL
SELECT Id FROM FrameworkApplication WHERE Name = 'PTC CH'
UNION ALL
SELECT Id FROM FrameworkApplication WHERE Name = 'PTC D'
UNION ALL
SELECT Id FROM FrameworkApplication WHERE Name = 'LPN'

INSERT INTO FrameworkFileStorage (ConfigurationId, Name, FileNameUpload)
SELECT (SELECT ConfigurationId FROM FrameworkConfigurationView WHERE ApplicationName = 'PTC'), 'Outline A', 'OutlineA.docx'
UNION ALL
SELECT (SELECT ConfigurationId FROM FrameworkConfigurationView WHERE ApplicationName = 'PTC'), 'Outline B', 'OutlineB.docx'
UNION ALL
SELECT (SELECT ConfigurationId FROM FrameworkConfigurationView WHERE ApplicationName = 'PTC CH'), 'Outline D', 'OutlineD.docx'
UNION ALL
SELECT (SELECT ConfigurationId FROM FrameworkConfigurationView WHERE ApplicationName = 'PTC CH'), 'Outline B', 'OutlineB2.docx'

INSERT FrameworkLanguage (ConfigurationId, ParentId, Name)
SELECT (SELECT ConfigurationId FROM FrameworkConfigurationView WHERE ApplicationName = 'PTC'), NULL, 'Default'
UNION ALL
SELECT (SELECT ConfigurationId FROM FrameworkConfigurationView WHERE ApplicationName = 'PTC'), (SELECT Id FROM FrameworkLanguage WHERE Name = 'Default'), 'English'
UNION ALL
SELECT (SELECT ConfigurationId FROM FrameworkConfigurationView WHERE ApplicationName = 'PTC'), (SELECT Id FROM FrameworkLanguage WHERE Name = 'English'), 'German'
UNION ALL
SELECT (SELECT ConfigurationId FROM FrameworkConfigurationView WHERE ApplicationName = 'LPN'), NULL, 'French'

INSERT INTO FrameworkConfiguration (LanguageId)
SELECT Id FROM FrameworkLanguage

GO

INSERT INTO FrameworkText (ConfigurationId, Name)
SELECT Id AS ConfigurationId, 'Connecter' FROM FrameworkConfiguration WHERE LanguageId =
(SELECT Id AS LanguageId FROM FrameworkLanguage WHERE ConfigurationId = (SELECT Id AS ApplicationId FROM FrameworkApplication WHERE Name = 'LPN') AND Name = 'French')
UNION ALL
SELECT Id AS ConfigurationId, 'Login' FROM FrameworkConfiguration WHERE LanguageId =
(SELECT Id AS LanguageId FROM FrameworkLanguage WHERE ConfigurationId = (SELECT Id AS ApplicationId FROM FrameworkApplication WHERE Name = 'PTC') AND Name = 'Default')
UNION ALL
SELECT Id AS ConfigurationId, 'Anmelden' FROM FrameworkConfiguration WHERE LanguageId =
(SELECT Id AS LanguageId FROM FrameworkLanguage WHERE ConfigurationId = (SELECT Id AS ApplicationId FROM FrameworkApplication WHERE Name = 'PTC') AND Name = 'German')

INSERT INTO FrameworkUser (ApplicationId, Name)
SELECT (SELECT Id AS ApplicationId FROM FrameworkApplication WHERE Name = 'Framework') AS ApplicationId,  'Admin' AS Name
UNION ALL
SELECT (SELECT Id AS ApplicationId FROM FrameworkApplication WHERE Name = 'LPN') AS ApplicationId,  'John' AS Name
UNION ALL
SELECT (SELECT Id AS ApplicationId FROM FrameworkApplication WHERE Name = 'PTC CH') AS ApplicationId,  'Hnc' AS Name


INSERT INTO FrameworkRole (ApplicationTypeId, Name)
SELECT (SELECT Id FROM FrameworkApplication WHERE Name = 'Framework'), 'Admin'
UNION ALL
SELECT (SELECT Id FROM FrameworkApplication WHERE Name = 'PTC'), 'Coordy'
UNION ALL
SELECT (SELECT Id FROM FrameworkApplication WHERE Name = 'PTC'), 'Spky'

INSERT INTO FrameworkSession (Name, ApplicationId, ApplicationTypeId)
SELECT NEWID(), (SELECT Id FROM FrameworkApplication WHERE Name = 'PTC CH'), (SELECT ApplicationTypeId FROM FrameworkApplication WHERE Name = 'PTC CH') /* User not logged in, no language selected */
INSERT INTO FrameworkSession (Name, ApplicationId, ApplicationTypeId, UserId)
SELECT NEWID(), (SELECT Id FROM FrameworkApplication WHERE Name = 'PTC CH'), (SELECT ApplicationTypeId FROM FrameworkApplication WHERE Name = 'PTC CH'), (SELECT Id FROM FrameworkUser WHERE ApplicationId = (SELECT Id FROM FrameworkApplication WHERE Name = 'PTC CH') AND Name = 'hnc')
