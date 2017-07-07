IF (NOT ISNULL((SELECT Version FROM FrameworkVersion), '') = 'v1.01') RETURN  -- Version Check

INSERT INTO FrameworkApplicationType (Name)
SELECT 'Framework' AS Name
UNION ALL
SELECT 'PTC' AS Name
UNION ALL
SELECT 'LPN' AS Name

INSERT INTO FrameworkApplication (Name, ApplicationTypeId, Domain)
SELECT 'Framework' AS Name, (SELECT Id FROM FrameworkApplicationType WHERE Name = 'Framework') AS ApplicationTypeId, 'framework' AS Domain
INSERT INTO FrameworkConfiguration (ApplicationId)
SELECT (SELECT Id From FrameworkApplication WHERE Name = 'Framework')
--
INSERT INTO FrameworkApplication (Name, ApplicationTypeId, Domain)
SELECT 'PTC' AS Name, (SELECT Id FROM FrameworkApplicationType WHERE Name = 'PTC'), 'ptc' AS Domain
INSERT INTO FrameworkConfiguration (ApplicationId)
SELECT (SELECT Id From FrameworkApplication WHERE Name = 'PTC')
--
INSERT INTO FrameworkApplication (ParentId, Name, ApplicationTypeId, Domain)
SELECT (SELECT Id From FrameworkApplication WHERE Name = 'PTC'), 'PTC CH' AS Name, (SELECT Id FROM FrameworkApplicationType WHERE Name = 'PTC'), 'ptcch'
INSERT INTO FrameworkConfiguration (ApplicationId)
SELECT (SELECT Id From FrameworkApplication WHERE Name = 'PTC CH')
--
INSERT INTO FrameworkApplication (ParentId, Name, ApplicationTypeId, Domain)
SELECT (SELECT Id From FrameworkApplication WHERE Name = 'PTC CH'), 'PTC D' AS Name, (SELECT Id FROM FrameworkApplicationType WHERE Name = 'PTC'), 'ptcd'
INSERT INTO FrameworkConfiguration (ApplicationId)
SELECT (SELECT Id From FrameworkApplication WHERE Name = 'PTC D')
--
INSERT INTO FrameworkApplication (Name, ApplicationTypeId, Domain)
SELECT 'LPN' AS Name, (SELECT Id FROM FrameworkApplicationType WHERE Name = 'LPN'), 'lpn'
INSERT INTO FrameworkConfiguration (ApplicationId)
SELECT (SELECT Id From FrameworkApplication WHERE Name = 'LPN')

INSERT INTO FrameworkFileStorage (ConfigurationId, Name, FileNameUpload)
SELECT (SELECT ConfigurationId FROM FrameworkConfigurationView WHERE ApplicationName = 'PTC'), 'Outline A', 'OutlineA.docx'
UNION ALL
SELECT (SELECT ConfigurationId FROM FrameworkConfigurationView WHERE ApplicationName = 'PTC'), 'Outline B', 'OutlineB.docx'
UNION ALL
SELECT (SELECT ConfigurationId FROM FrameworkConfigurationView WHERE ApplicationName = 'PTC CH'), 'Outline D', 'OutlineD.docx'
UNION ALL
SELECT (SELECT ConfigurationId FROM FrameworkConfigurationView WHERE ApplicationName = 'PTC CH'), 'Outline B', 'OutlineB2.docx'

INSERT FrameworkLanguage (ConfigurationId, Name)
SELECT (SELECT ConfigurationId FROM FrameworkConfigurationView WHERE ApplicationName = 'PTC'), 'Default'
--
INSERT FrameworkLanguage (ConfigurationId, ParentId, Name)
SELECT (SELECT ConfigurationId FROM FrameworkConfigurationView WHERE ApplicationName = 'PTC'), (SELECT Id FROM FrameworkLanguage WHERE ConfigurationId = (SELECT ConfigurationId FROM FrameworkConfigurationView WHERE ApplicationName = 'PTC') AND Name = 'Default'), 'English'
--
INSERT FrameworkLanguage (ConfigurationId, ParentId, Name)
SELECT (SELECT ConfigurationId FROM FrameworkConfigurationView WHERE ApplicationName = 'PTC'), (SELECT Id FROM FrameworkLanguage WHERE ConfigurationId = (SELECT ConfigurationId FROM FrameworkConfigurationView WHERE ApplicationName = 'PTC') AND Name = 'English'), 'German'
--
INSERT FrameworkLanguage (ConfigurationId, Name)
SELECT (SELECT ConfigurationId FROM FrameworkConfigurationView WHERE ApplicationName = 'LPN'), 'French'

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
