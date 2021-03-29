GO
CREATE TABLE Doc.LoginUser
(
    Id INT PRIMARY KEY IDENTITY,
    Name NVARCHAR(256) NOT NULL UNIQUE, -- Email
    Password NVARCHAR(256),
    IsIntegrate BIT NOT NULL, -- Built into CSharp code with IdNameEnum and deployed with cli deployDb command
    IsDelete BIT NOT NULL,
)
GO
CREATE VIEW Doc.LoginUserIntegrate AS
SELECT
    LoginUser.*,
    LoginUser.Name AS IdName
FROM
    Doc.LoginUser LoginUser

-- LoginRole (Authenticated, Reseller, Admin, Developer)
GO
CREATE TABLE Doc.LoginRole
(
    Id INT PRIMARY KEY IDENTITY,
    Name NVARCHAR(256) NOT NULL UNIQUE,
    Sort FLOAT,
)
GO
CREATE VIEW Doc.LoginRoleIntegrate AS
SELECT
    *,
    Name AS IdName
FROM
    Doc.LoginRole

-- User to LoginRole mapping
GO
CREATE TABLE Doc.LoginUserRole
(
    Id INT PRIMARY KEY IDENTITY,
    LoginUserId INT NOT NULL FOREIGN KEY REFERENCES Doc.LoginUser(Id),
    LoginRoleId INT NOT NULL FOREIGN KEY REFERENCES Doc.LoginRole(Id),
    IsActive BIT
    INDEX IX_LoginUserRole UNIQUE (LoginUserId, LoginRoleId)
)
GO
CREATE VIEW Doc.LoginUserRoleIntegrate AS
SELECT
    *,
    (SELECT IsIntegrate FROM Doc.LoginUser WHERE Id = Data.LoginUserId) AS LoginUserIsIntegrate,
    (SELECT Name FROM Doc.LoginUser WHERE Id = Data.LoginUserId) AS LoginUserIdName,
    (SELECT Name FROM Doc.LoginRole WHERE Id = Data.LoginRoleId) AS LoginRoleIdName
FROM
    Doc.LoginUserRole Data

GO
CREATE VIEW Doc.LoginUserRoleDisplay AS
SELECT
    LoginUser.Id AS LoginUserId,
    LoginUser.Name AS LoginUserName,
    LoginRole.Id AS LoginRoleId,
    LoginRole.Name AS LoginRoleName,
    (SELECT Id FROM Doc.LoginUserRole WHERE LoginUserId = LoginUser.Id AND LoginRoleId = LoginRole.Id) AS LoginUserRoleId,
    (SELECT IsActive FROM Doc.LoginUserRole WHERE LoginUserId = LoginUser.Id AND LoginRoleId = LoginRole.Id) AS IsActive
FROM
    Doc.LoginUser LoginUser,
    Doc.LoginRole LoginRole

GO
CREATE VIEW Doc.LoginUserRoleApp AS -- Used by application only
SELECT
    LoginUser.Id AS LoginUserId,
    LoginUser.Name AS LoginUserName,
    LoginUser.Password AS LoginUserPassword,
    LoginRole.Name AS LoginRoleName,
    LoginUserRole.IsActive AS LoginUserRoleIsActive
FROM
    Doc.LoginUser LoginUser
LEFT JOIN
    Doc.LoginUserRole LoginUserRole ON (LoginUserRole.LoginRoleId = LoginUser.Id)
LEFT JOIN
    Doc.LoginRole LoginRole ON (LoginRole.Id = LoginUserRole.LoginRoleId)
