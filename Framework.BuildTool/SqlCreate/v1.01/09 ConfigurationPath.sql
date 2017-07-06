/* Every Application, Language and User gets it's Configuration layer */
INSERT INTO FrameworkConfiguration (ApplicationId, LanguageId, UserId)
SELECT Id, NULL, NULL FROM FrameworkApplication
EXCEPT SELECT ApplicationId, LanguageId, UserId FROM FrameworkConfiguration

INSERT INTO FrameworkConfiguration (ApplicationId, LanguageId, UserId)
SELECT NULL, Id, NULL FROM FrameworkLanguage
EXCEPT SELECT ApplicationId, LanguageId, UserId FROM FrameworkConfiguration

INSERT INTO FrameworkConfiguration (ApplicationId, LanguageId, UserId)
SELECT NULL, NULL, UserId FROM FrameworkSession
EXCEPT SELECT ApplicationId, LanguageId, UserId FROM FrameworkConfiguration

/* ConfigurationTree add Application */
INSERT INTO FrameworkConfigurationTree (ApplicationId, ApplicationParentId)
SELECT Id, ParentId FROM FrameworkApplication
EXCEPT
SELECT ApplicationId, ApplicationParentId FROM FrameworkConfigurationTree

UPDATE ConfigurationTree SET ConfigurationTree.ParentId = (SELECT Id FROM FrameworkConfigurationTree ConfigurationTree2 WHERE ConfigurationTree2.ApplicationId = ConfigurationTree.ApplicationParentId)
FROM FrameworkConfigurationTree ConfigurationTree

UPDATE ConfigurationTree SET ConfigurationTree.ConfigurationId = (SELECT Id FROM FrameworkConfiguration Configuration2 WHERE Configuration2.ApplicationId = ConfigurationTree.ApplicationId)
FROM FrameworkConfigurationTree ConfigurationTree

/* Update FrameworkConfigurationPath */
DELETE FrameworkConfigurationPath
INSERT INTO FrameworkConfigurationPath (ConfigurationId, ConfigurationIdContain, Level)
SELECT ConfigurationId, ConfigurationIdContain, Level FROM FrameworkConfigurationTreeHierarchy