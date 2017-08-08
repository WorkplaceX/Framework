IF NOT EXISTS(SELECT * FROM FrameworkVersion WHERE Name = 'Framework' AND Version = 'v1.0') BEGIN SELECT 'RETURN' RETURN END -- Version Check

DELETE FrameworkVersion WHERE Name = 'Framework'

