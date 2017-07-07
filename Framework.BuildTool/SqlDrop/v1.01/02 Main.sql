IF (NOT ISNULL((SELECT Version FROM FrameworkVersion), '') = 'v1.01') RETURN  -- Version Check

/* Text */
DROP VIEW FrameworkTextView
DROP TABLE FrameworkText
/* FileStorageText */
DROP VIEW FrameworkFileStorageView
DROP TABLE FrameworkFileStorage
/* Role */
DROP TABLE FrameworkUserRole
DROP TABLE FrameworkRole
/* Column */
DROP TABLE FrameworkColumn
/* Session */
DROP TABLE FrameworkSession
/* Configuration */
DROP VIEW FrameworkLanguageView
DROP VIEW FrameworkConfigurationView
DROP TABLE FrameworkConfigurationPath
ALTER TABLE FrameworkConfiguration DROP CONSTRAINT FK_FrameworkConfiguration_UserId
DROP TABLE FrameworkUser
ALTER TABLE FrameworkConfiguration DROP CONSTRAINT FK_FrameworkConfiguration_LanguageId
DROP TABLE FrameworkLanguage
DROP TABLE FrameworkConfiguration
DROP TABLE FrameworkApplication
DROP TABLE FrameworkApplicationType