IF NOT EXISTS(SELECT * FROM FrameworkVersion WHERE Name = 'Framework' AND Version = 'v1.01') BEGIN SELECT 'RETURN' RETURN END -- Version Check

DROP TABLE FrameworkFileStorage
DROP VIEW FrameworkConfigColumnView
DROP TABLE FrameworkConfigColumn
DROP TABLE FrameworkColumn
DROP TABLE FrameworkTable
DROP TABLE FrameworkSession
DROP VIEW FrameworkApplicationView
DROP TABLE FrameworkApplication
DROP TABLE FrameworkApplicationType
