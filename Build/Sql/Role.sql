IF EXISTS(SELECT * FROM sys.views WHERE name = 'LoRoleMatrix') DROP VIEW LoRoleMatrix
IF EXISTS(SELECT * FROM sys.views WHERE name = 'SyRoleAccess') DROP VIEW SyRoleAccess
IF EXISTS(SELECT * FROM sys.views WHERE name = 'LoRoleAccess') DROP VIEW LoRoleAccess
IF EXISTS(SELECT * FROM sys.tables WHERE name = 'LoRoleUser') DROP TABLE LoRoleUser
IF EXISTS(SELECT * FROM sys.tables WHERE name = 'LoRoleLoation') DROP TABLE LoRoleLoation
IF EXISTS(SELECT * FROM sys.tables WHERE name = 'LoRole') DROP TABLE LoRole
IF EXISTS(SELECT * FROM sys.tables WHERE name = 'LoLoation') DROP TABLE LoLoation
IF EXISTS(SELECT * FROM sys.tables WHERE name = 'SyRoleUser') DROP TABLE SyRoleUser
IF EXISTS(SELECT * FROM sys.tables WHERE name = 'SyUser') DROP TABLE SyUser
IF EXISTS(SELECT * FROM sys.tables WHERE name = 'SyRole') DROP TABLE SyRole

GO

/* SyUser */
CREATE TABLE SyUser
(
	Id INT PRIMARY KEY IDENTITY,
	Name NVARCHAR(256) NOT NULL UNIQUE,
)
INSERT INTO SyUser(Name)
SELECT 'Max'
UNION ALL
SELECT 'Fritz'
UNION ALL
SELECT 'Heinz'

/* SyRole */
CREATE TABLE SyRole
(
	Id INT PRIMARY KEY IDENTITY,
	Name NVARCHAR(256) NOT NULL UNIQUE,
	IsAdmin BIT
)
INSERT INTO SyRole(Name, IsAdmin)
SELECT 'Admin', 1
UNION ALL
SELECT 'Guest', 0

/* SyRoleUser */
CREATE TABLE SyRoleUser
(
	Id INT PRIMARY KEY IDENTITY,
	UserId INT NOT NULL FOREIGN KEY REFERENCES SyUser(Id), /* Source */
	RoleId INT NOT NULL FOREIGN KEY REFERENCES SyRoleUser(Id), /* Dest */
	IsActive BIT NOT NULL,
	UNIQUE (UserId, RoleId)
)
INSERT INTO SyRoleUser(UserId, RoleId, IsActive)
SELECT Id, (SELECT Id FROM SyRole WHERE Name = 'Admin'), 1 FROM SyUser WHERE Name = 'Max'
UNION ALL
SELECT Id, (SELECT Id FROM SyRole WHERE Name = 'Guest'), 1 FROM SyUser WHERE Name = 'Fritz'

/* LoLoation */
CREATE TABLE LoLoation
(
	Id INT PRIMARY KEY IDENTITY,
	Name NVARCHAR(256) NOT NULL UNIQUE,
)
INSERT INTO LoLoation(Name)
SELECT 'Zurich'
UNION ALL
SELECT 'Berne'
UNION ALL
SELECT 'Geneva'
UNION ALL
SELECT 'Lucerne'

/* LoRole */
CREATE TABLE LoRole
(
	Id INT PRIMARY KEY IDENTITY,
	Name NVARCHAR(256) NOT NULL UNIQUE,
	IsAdmin BIT NOT NULL
)
INSERT INTO LoRole(Name, IsAdmin)
SELECT 'German', 0
UNION ALL
SELECT 'French', 0
UNION ALL
SELECT 'Admin Module', 1


/* LoRoleLoation */
CREATE TABLE LoRoleLoation
(
	Id INT PRIMARY KEY IDENTITY,
	RoleId INT FOREIGN KEY REFERENCES LoRole(Id), /* Source */
	UserId INT FOREIGN KEY REFERENCES SyUser(Id), /* Source (link user directly to location) */
	LoationId INT NOT NULL FOREIGN KEY REFERENCES LoLoation(Id), /* Dest */
	IsActive BIT NOT NULL,
	UNIQUE (RoleId, UserId, LoationId)
)
INSERT INTO LoRoleLoation (RoleId, UserId, LoationId, IsActive)
SELECT Id, NULL, (SELECT Id FROM LoLoation WHERE Name = 'Zurich'), 1 FROM LoRole WHERE Name = 'German'
UNION ALL
SELECT Id, NULL, (SELECT Id FROM LoLoation WHERE Name = 'Berne'), 1 FROM LoRole WHERE Name = 'German'
UNION ALL
SELECT Id, NULL, (SELECT Id FROM LoLoation WHERE Name = 'Geneva'), 1 FROM LoRole WHERE Name = 'French'
UNION ALL
SELECT NULL, (SELECT Id FROM SyUser WHERE Name = 'Fritz'), (SELECT Id FROM LoLoation WHERE Name = 'Lucerne'), 1

/* LoRoleUser */
CREATE TABLE LoRoleUser
(
	Id INT PRIMARY KEY IDENTITY,
	UserId INT NOT NULL FOREIGN KEY REFERENCES SyUser(Id), /* Source */
	RoleId INT NOT NULL FOREIGN KEY REFERENCES LoRole(Id), /* Dest */
	IsActive BIT NOT NULL,
	UNIQUE (UserId, RoleId)
)
INSERT INTO LoRoleUser (UserId, RoleId, IsActive)
SELECT Id, (SELECT Id FROM LoRole WHERE Name = 'German'), 1 FROM SyUser WHERE Name = 'Max'
UNION ALL
SELECT Id, (SELECT Id FROM LoRole WHERE Name = 'French'), 1 FROM SyUser WHERE Name = 'Fritz'
UNION ALL
SELECT Id, (SELECT Id FROM LoRole WHERE Name = 'Admin Module'), 1 FROM SyUser WHERE Name = 'Heinz'

GO

CREATE VIEW SyRoleAccess AS
SELECT 
	SyUser.Id AS UserId,
	SyUser.Name AS UserName,
	SyRole.Id AS RoleId,
	SyRole.Name AS RoleName,
	SyRole.IsAdmin AS IsAdmin

FROM
	SyUser SyUser

LEFT JOIN
	SyRoleUser SyRoleUser ON (SyRoleUser.UserId = SyUser.Id AND SyRoleUser.IsActive = 1)

LEFT JOIN
	SyRole SyRole ON (SyRole.Id = SyRoleUser.RoleId)
	
GO	

CREATE VIEW LoRoleAccess AS
SELECT 
	SyUser.Id AS UserId,
	SyUser.Name AS UserName,
	LoLoation.Id AS LoationId,
	LoLoation.Name AS LoationName

FROM
	SyUser SyUser
	
LEFT JOIN
	SyRoleUser SyRoleUser ON (SyRoleUser.UserId = SyUser.Id AND SyRoleUser.IsActive = 1)

LEFT JOIN
	SyRole SyRole ON (SyRole.Id = SyRoleUser.RoleId)
	
LEFT JOIN
	LoRoleUser LoRoleUser ON (LoRoleUser.UserId = SyUser.Id AND LoRoleUser.IsActive = 1)
	
LEFT JOIN
	LoRole LoRole ON (LoRole.Id = LoRoleUser.RoleId)
	
LEFT JOIN
	LoRoleLoation LoRoleLoation ON ((LoRoleLoation.RoleId = LoRoleUser.RoleId OR LoRoleLoation.UserId = SyUser.Id) AND LoRoleLoation.IsActive = 1)
	
LEFT JOIN
	LoLoation LoLoation ON (LoLoation.Id = LoRoleLoation.LoationId OR SyRole.IsAdmin = 1 OR LoRole.IsAdmin = 1)

GROUP BY
	SyUser.Id,
	SyUser.Name,
	LoLoation.Id,
	LoLoation.Name

GO

CREATE VIEW LoRoleMatrix AS
SELECT 
	SyUser.Id AS UserId,
	SyUser.Name AS UserName,
	LoLoation.Id AS LoationId,
	LoLoation.Name AS LoationName,
	LoRole.Id AS RoleId,
	LoRole.Name AS RoleName,
	(SELECT LoRoleUser.IsActive FROM LoRoleUser WHERE LoRoleUser.UserId = SyUser.Id AND LoRoleUser.RoleId = LoRole.Id) AS IsRole,
	(SELECT LoRoleLoation.IsActive FROM LoRoleLoation LoRoleLoation WHERE LoRoleLoation.UserId = SyUser.Id AND LoRoleLoation.LoationId = LoLoation.Id) AS IsDirect,
	(SELECT SyRoleAccess.IsAdmin FROM SyRoleAccess SyRoleAccess WHERE SyRoleAccess.UserId = SyUser.Id) AS IsAdmin,
	(SELECT LoRoleUser.IsActive FROM LoRoleUser LoRoleUser WHERE LoRoleUser.UserId = SyUser.Id AND LoRoleUser.RoleId = (SELECT LoRole.Id FROM LoRole LoRole WHERE LoRole.IsAdmin = 1)) AS IsAdminModule,
	(SELECT 1 FROM LoRoleAccess LoRoleAccess WHERE LoRoleAccess.UserId = SyUser.Id AND LoRoleAccess.LoationId = LoLoation.Id) AS IsAccess

FROM
	SyUser SyUser

CROSS JOIN
	LoLoation LoLoation

CROSS JOIN
	LoRole LoRole

GO	

SELECT * FROM LoRoleMatrix