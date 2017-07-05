IF (NOT ISNULL((SELECT Version FROM FrameworkVersion), '') = 'v1.0') RETURN  -- Version Check

DROP TABLE FrameworkVersion

