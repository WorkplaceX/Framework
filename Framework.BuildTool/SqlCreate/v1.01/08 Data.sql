IF (NOT ISNULL((SELECT Version FROM FrameworkVersion), '') = 'v1.01') RETURN  -- Version Check

INSERT INTO FrameworkApplication (Name)
SELECT 'PTC' AS Name
UNION ALL
SELECT 'PTC CH' AS Name
UNION ALL
SELECT 'PTC D' AS Name
UNION ALL
SELECT 'LPN' AS Name

INSERT INTO FrameworkConfiguration (ApplicationId)
SELECT Id FROM FrameworkApplication WHERE Name = 'PTC'
UNION ALL
SELECT Id FROM FrameworkApplication WHERE Name = 'PTC CH'
UNION ALL
SELECT Id FROM FrameworkApplication WHERE Name = 'PTC D'
UNION ALL
SELECT Id FROM FrameworkApplication WHERE Name = 'LPN'

INSERT INTO FrameworkFileStorage (ConfigurationId, FileName, FileNameUpload)
SELECT (SELECT ConfigurationId FROM FrameworkConfigurationView WHERE ApplicationName = 'PTC'), 'Outline A', 'OutlineA.docx'
UNION ALL
SELECT (SELECT ConfigurationId FROM FrameworkConfigurationView WHERE ApplicationName = 'PTC'), 'Outline B', 'OutlineB.docx'
UNION ALL
SELECT (SELECT ConfigurationId FROM FrameworkConfigurationView WHERE ApplicationName = 'PTC CH'), 'Outline D', 'OutlineD.docx'
UNION ALL
SELECT (SELECT ConfigurationId FROM FrameworkConfigurationView WHERE ApplicationName = 'PTC CH'), 'Outline B', 'OutlineB2.docx'

INSERT INTO FrameworkConfigurationPath (ConfigurationId, ContainConfigurationId, Level)
SELECT (SELECT ConfigurationId FROM FrameworkConfigurationView WHERE ApplicationName = 'PTC'), (SELECT ConfigurationId FROM FrameworkConfigurationView WHERE ApplicationName = 'PTC'), 1
UNION ALL
SELECT (SELECT ConfigurationId FROM FrameworkConfigurationView WHERE ApplicationName = 'PTC CH'), (SELECT ConfigurationId FROM FrameworkConfigurationView WHERE ApplicationName = 'PTC CH'), 1
UNION ALL
SELECT (SELECT ConfigurationId FROM FrameworkConfigurationView WHERE ApplicationName = 'PTC CH'), (SELECT ConfigurationId FROM FrameworkConfigurationView WHERE ApplicationName = 'PTC'), 2
UNION ALL
SELECT (SELECT ConfigurationId FROM FrameworkConfigurationView WHERE ApplicationName = 'PTC D'), (SELECT ConfigurationId FROM FrameworkConfigurationView WHERE ApplicationName = 'PTC'), 1
UNION ALL
SELECT (SELECT ConfigurationId FROM FrameworkConfigurationView WHERE ApplicationName = 'LPN'), (SELECT ConfigurationId FROM FrameworkConfigurationView WHERE ApplicationName = 'LPN'), 1

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
SELECT (SELECT ConfigurationId FROM FrameworkLanguageView WHERE ConfigurationApplicationName = 'LPN' AND Name='French'), 'Connecter'
UNION ALL
SELECT (SELECT ConfigurationId FROM FrameworkLanguageView WHERE ConfigurationApplicationName = 'PTC' AND Name='Default'), 'Login'
UNION ALL
SELECT (SELECT ConfigurationId FROM FrameworkLanguageView WHERE ConfigurationApplicationName = 'PTC' AND Name='German'), 'Anmelden'

GO

INSERT INTO FrameworkConfigurationPath (ConfigurationId, ContainConfigurationId, Level)
SELECT (SELECT ConfigurationId FROM FrameworkConfigurationView WHERE ApplicationName = 'LPN'), (SELECT ConfigurationId FROM FrameworkConfigurationView WHERE LanguageName = 'French'), 1
