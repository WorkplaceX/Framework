﻿IF NOT EXISTS(SELECT * FROM FrameworkVersion WHERE Name = 'Framework' AND Version = 'v1.02') BEGIN SELECT 'RETURN' RETURN END -- Version Check
