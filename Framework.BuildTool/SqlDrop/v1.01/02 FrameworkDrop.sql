IF (NOT ISNULL((SELECT Version FROM FrameworkVersion), '') = 'v1.01') RETURN  -- Version Check

DROP TABLE FrameworkFileStorage
DROP TABLE FrameworkColumn
DROP TABLE FrameworkTable
DROP TABLE FrameworkSession
DROP VIEW FrameworkApplicationView
DROP TABLE FrameworkApplication
DROP TABLE FrameworkApplicationType
