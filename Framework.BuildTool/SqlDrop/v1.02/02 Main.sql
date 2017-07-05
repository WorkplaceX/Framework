IF (NOT ISNULL((SELECT Version FROM FrameworkVersion), '') = 'v1.02') RETURN  -- Version Check
DROP VIEW FrameworkTextView
DROP VIEW FrameworkLanguageView
DROP VIEW FrameworkSwitchView
DROP VIEW FrameworkFileStorageView
DROP TABLE FrameworkSession
DROP TABLE FrameworkUserRole
DROP TABLE FrameworkRole
DROP TABLE FrameworkColumn
DROP TABLE FrameworkUser
DROP TABLE FrameworkText
ALTER TABLE FrameworkSwitch DROP CONSTRAINT FK_FrameworkSwitch_LanguageId
DROP TABLE FrameworkLanguage
DROP TABLE FrameworkFileStorage
DROP TABLE FrameworkSwitchPath
DROP TABLE FrameworkSwitchTree
DROP TABLE FrameworkSwitch
DROP TABLE FrameworkApplication
