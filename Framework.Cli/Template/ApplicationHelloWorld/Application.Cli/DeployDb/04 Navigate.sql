CREATE TABLE Doc.Navigate
(
    Id INT PRIMARY KEY IDENTITY,
    ParentId INT FOREIGN KEY REFERENCES Doc.Navigate(Id), -- ParentId Integrate naming convention for hierarchical structure.
    Name NVARCHAR(256) NOT NULL UNIQUE,
    TextHtml NVARCHAR(256),
    IsDivider BIT NOT NULL,
    IsNavbarEnd BIT NOT NULL,
    NavigatePath NVARCHAR(256),
    PageTypeName NVARCHAR(256),
    Sort FLOAT,
)
GO
CREATE VIEW Doc.NavigateIntegrate AS
SELECT
    *,
    Name AS IdName,
    (SELECT Name FROM Doc.Navigate Navigate2 WHERE Navigate2.Id = Navigate.ParentId) AS ParentIdName
FROM
    Doc.Navigate Navigate

-- Navigate to Role mapping
GO
CREATE TABLE Doc.NavigateRole
(
    Id INT PRIMARY KEY IDENTITY,
	NavigateId INT NOT NULL FOREIGN KEY REFERENCES Doc.Navigate(Id),
	LoginRoleId INT NOT NULL FOREIGN KEY REFERENCES Doc.LoginRole(Id),
	IsActive BIT
	INDEX IX_NavigateRole UNIQUE (NavigateId, LoginRoleId)
)
GO
CREATE VIEW Doc.NavigateRoleIntegrate AS
SELECT
	*,
	(SELECT Name FROM Doc.Navigate WHERE Id = Data.NavigateId) AS NavigateIdName,
	(SELECT Name FROM Doc.LoginRole WHERE Id = Data.LoginRoleId) AS LoginRoleIdName
FROM
	Doc.NavigateRole Data

GO
CREATE VIEW Doc.NavigateRoleDisplay AS
SELECT
    Navigate.Id AS NavigateId,
	LoginRole.Id AS LoginRoleId,
	LoginRole.Name AS LoginRoleName,
	(SELECT Id FROM Doc.NavigateRole WHERE NavigateId = Navigate.Id AND LoginRoleId = LoginRole.Id) AS NavigateRoleId,
	(SELECT IsActive FROM Doc.NavigateRole WHERE NavigateId = Navigate.Id AND LoginRoleId = LoginRole.Id) AS IsActive
FROM
	Doc.Navigate Navigate,
	Doc.LoginRole LoginRole

GO
CREATE VIEW Doc.NavigateDisplay AS
SELECT
    LoginUser.Id AS LoginUserId,
    LoginUser.Name AS LoginUserName,
    Navigate.*
FROM
    Doc.Navigate Navigate,
    Doc.LoginUser LoginUser
WHERE
EXISTS
(
	SELECT LoginRoleId FROM Doc.LoginUserRole WHERE LoginUserId = LoginUser.Id AND IsActive = 1
	INTERSECT
	SELECT LoginRoleId FROM Doc.NavigateRole WHERE NavigateId = Navigate.Id AND IsActive = 1
)

/*
CREATE VIEW Doc.NavigateDisplay AS
WITH Cte (Id, ParentId, Name, TextHtml, IsDivider, IsNavbarEnd, NavigatePath, PageTypeName, Sort, Level, IsParent, Path) AS
(
    SELECT 
        Navigate.*, 
        0 AS Level, 
        CASE WHEN EXISTS(SELECT * FROM  Doc.Navigate Navigate2 WHERE Navigate2.ParentId = Navigate.Id) THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END AS IsParent, 
        CAST(Name as NVARCHAR(1024)) AS Path 
    FROM 
        Doc.Navigate Navigate WHERE ParentId IS NULL
    UNION ALL
    SELECT 
        Navigate.*, 
        Cte.Level + 1 AS Level, 
        CASE WHEN EXISTS(SELECT * FROM  Doc.Navigate Navigate2 WHERE Navigate2.ParentId = Navigate.Id) THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END AS IsParent, 
        CAST(CONCAT(Cte.Path, ' > ', Navigate.Name) AS NVARCHAR(1024)) AS Path
    FROM 
        Doc.Navigate Navigate
    INNER JOIN Cte ON Cte.Id = Navigate.ParentId
)
SELECT * FROM Cte
*/