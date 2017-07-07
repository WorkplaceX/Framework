/* Every Application gets it's Configuration layer */
INSERT INTO FrameworkConfiguration (ApplicationId, LanguageId, UserId)
SELECT Id, NULL, NULL FROM FrameworkApplication
EXCEPT SELECT ApplicationId, LanguageId, UserId FROM FrameworkConfiguration

INSERT INTO FrameworkConfiguration (ApplicationId, LanguageId, UserId)
SELECT NULL, Id, NULL FROM FrameworkLanguage
EXCEPT SELECT ApplicationId, LanguageId, UserId FROM FrameworkConfiguration

/* Path for Application, Language */
INSERT INTO FrameworkConfigurationPath (ApplicationId, ConfigurationId, ConfigurationIdContain, Level)
SELECT ApplicationId, ConfigurationId, ConfigurationIdContain, Level FROM FrameworkApplicationHierarchy

INSERT INTO FrameworkConfigurationPath (LanguageId, ConfigurationId, ConfigurationIdContain, Level)
SELECT LanguageId, ConfigurationId, ConfigurationIdContain, Level FROM FrameworkLanguageHierarchy

/* Add Text */
INSERT INTO FrameworkText (ConfigurationId, Name)
SELECT (SELECT ConfigurationId FROM FrameworkLanguageView WHERE ApplicationName = 'LPN' AND Name = 'French'), 'Connecter' 
UNION ALL
SELECT (SELECT ConfigurationId FROM FrameworkLanguageView WHERE ApplicationName = 'PTC' AND Name = 'Default'), 'Login' 
UNION ALL
SELECT (SELECT ConfigurationId FROM FrameworkLanguageView WHERE ApplicationName = 'PTC' AND Name = 'German'), 'Anmelden' 

