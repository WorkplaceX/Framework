﻿CREATE TABLE FrameworkApplicationType
(
	Id INT PRIMARY KEY IDENTITY,
  	TypeName NVARCHAR(256) NOT NULL UNIQUE,
	IsExist BIT NOT NULL
)

CREATE TABLE FrameworkApplication
(
	Id INT PRIMARY KEY IDENTITY,
  	Text NVARCHAR(256),
	Path NVARCHAR(256) UNIQUE, /* Url */
	ApplicationTypeId INT FOREIGN KEY REFERENCES FrameworkApplicationType(Id) NOT NULL,
	IsActive BIT
)

GO
CREATE VIEW FrameworkApplicationDisplay AS
SELECT
	Application.Id,
	Application.Text,
	Application.Path,
	Application.ApplicationTypeId,
	(SELECT ApplicationType.TypeName FROM FrameworkApplicationType ApplicationType WHERE ApplicationType.Id = Application.ApplicationTypeId) AS TypeName,
	(SELECT ApplicationType.IsExist FROM FrameworkApplicationType ApplicationType WHERE ApplicationType.Id = Application.ApplicationTypeId) AS IsExist,
	Application.IsActive

FROM
	FrameworkApplication Application

GO
CREATE TABLE FrameworkTable /* Used for configuration. Contains all in source code defined tables. */
(
	Id INT PRIMARY KEY IDENTITY,
	TableNameCSharp NVARCHAR(256) NOT NULL UNIQUE, -- See also method UtilDataAccessLayer.TypeRowToNameCSharp();
	TableNameSql NVARCHAR(256), -- Can be null for memory rows.
	IsExist BIT NOT NULL
)

CREATE TABLE FrameworkColumn /* Used for configuration. Contains all in source code defined columns. Also calculated fields. */
(
	Id INT PRIMARY KEY IDENTITY,
	TableId INT FOREIGN KEY REFERENCES FrameworkTable(Id) NOT NULL ,
	ColumnNameCSharp NVARCHAR(256) NOT NULL,
	ColumnNameSql NVARCHAR(256), -- Can be null for calculated columns.
	IsExist BIT NOT NULL
	INDEX IX_FrameworkColumn UNIQUE (TableId, ColumnNameCSharp)
)

CREATE TABLE FrameworkGrid /* Used for configuration. Contains all in source code defined grids. (Static GridName) */
(
	Id INT PRIMARY KEY IDENTITY,
	TableId INT FOREIGN KEY REFERENCES FrameworkTable(Id) NOT NULL,
	GridName NVARCHAR(256),
	IsExist BIT NOT NULL
	INDEX IX_FrameworkGrid UNIQUE (TableId, GridName) -- For example new GridName<Table>("Master");
)

CREATE TABLE FrameworkConfigGrid
(
	Id INT PRIMARY KEY IDENTITY,
	GridId INT FOREIGN KEY REFERENCES FrameworkGrid(Id) NOT NULL UNIQUE,
	PageRowCountDefault INT, /* Defined in CSharp code (Attribute). */
	PageRowCount INT, /* Number of records to load on one page */
	PageColumnCountDefault INT, /* Defined in CSharp code (Attribute). */
	PageColumnCount INT, /* Number of columns to load (transfer to client) for one page */
	IsInsertDefault BIT, /* Defined in CSharp code (Attribute). */
	IsInsert BIT, /* Allow insert record */
)

CREATE TABLE FrameworkConfigColumn
(
	Id INT PRIMARY KEY IDENTITY,
	GridId INT FOREIGN KEY REFERENCES FrameworkGrid(Id) NOT NULL,
	ColumnId INT FOREIGN KEY REFERENCES FrameworkColumn(Id) NOT NULL,
	TextDefault NVARCHAR(256), -- Column header text.
	Text NVARCHAR(256), -- Column header text.
	DescriptionDefault NVARCHAR(256), -- Column header text.
	Description NVARCHAR(256), -- Column header text.
	IsVisibleDefault BIT,
	IsVisible BIT,
	IsReadOnlyDefault BIT,
	IsReadOnly BIT,
	SortDefault FLOAT,
	Sort FLOAT,
	WidthPercentDefault FLOAT,
	WidthPercent FLOAT,
	INDEX IX_FrameworkConfigColumn UNIQUE (GridId, ColumnId)
)

GO
CREATE VIEW FrameworkConfigGridDisplay AS
SELECT
	Grid.Id AS GridId,
	Grid.GridName,
	Grid.IsExist AS GridIsExist,
	TableX.Id AS TableId,
	TableX.TableNameCSharp,
	TableX.TableNameSql,
	TableX.IsExist AS TableIsExist,
	Config.Id AS ConfigId,
	Config.PageRowCountDefault,
	Config.PageRowCount,
	Config.PageColumnCountDefault,
	Config.PageColumnCount,
	Config.IsInsertDefault,
	Config.IsInsert

FROM
	FrameworkGrid Grid

LEFT JOIN
	FrameworkTable TableX ON TableX.Id = Grid.TableId
	
LEFT JOIN
	FrameworkConfigGrid Config ON Config.GridId = Grid.Id

GO
CREATE VIEW FrameworkConfigColumnDisplay AS
SELECT
	Grid.Id AS GridId,
	Grid.GridName,
	Grid.IsExist AS GridIsExist,
	TableX.Id AS TableId,
	TableX.TableNameCSharp,
	TableX.TableNameSql,
	TableX.IsExist AS TableIsExist,
	ColumnX.Id AS ColumnId,
	ColumnX.ColumnNameCSharp,
	ColumnX.ColumnNameSql,
	ColumnX.IsExist AS ColumnIsExist,
	Config.Id AS ConfigId,
	Config.TextDefault,
	Config.Text,
	Config.DescriptionDefault,
	Config.Description,
	Config.IsVisibleDefault,
	Config.IsVisible,
	Config.IsReadOnlyDefault,
	Config.IsReadOnly,
	Config.SortDefault,
	Config.Sort,
	Config.WidthPercentDefault,
	Config.WidthPercent

FROM
	FrameworkGrid Grid

LEFT JOIN
	FrameworkTable TableX ON TableX.Id = Grid.TableId

LEFT JOIN
	FrameworkColumn ColumnX ON ColumnX.TableId = Grid.TableId

LEFT JOIN
	FrameworkConfigColumn Config ON Config.GridId = Grid.Id AND Config.ColumnId = ColumnX.Id

GO
CREATE TABLE FrameworkFileStorage
(
	Id INT PRIMARY KEY IDENTITY,
	ApplicationId INT FOREIGN KEY REFERENCES FrameworkApplication(Id),
  	Name NVARCHAR(256) NOT NULL, /* File name with path */
  	FileNameUpload NVARCHAR(256),
	Data VARBINARY(MAX),
	IsDelete BIT,
	INDEX IX_FrameworkFileStorage UNIQUE (ApplicationId, Name)
)

GO
CREATE TABLE FrameworkComponent
(
	Id INT PRIMARY KEY IDENTITY,
	ComponentNameCSharp NVARCHAR(256)  NOT NULL,
	IsPage BIT NOT NULL, /* Derives from Page */
	IsExist BIT NOT NULL
)

CREATE TABLE FrameworkNavigation
(
	Id INT PRIMARY KEY IDENTITY,
  	Text NVARCHAR(256),
	ComponentId INT FOREIGN KEY REFERENCES FrameworkComponent(Id),
)

GO
CREATE VIEW FrameworkNavigationDisplay AS
SELECT
	Navigation.Id AS Id,
	Navigation.Text AS Text,
	Navigation.ComponentId,
	Component.ComponentNameCSharp
FROM
	FrameworkNavigation Navigation
LEFT JOIN
	FrameworkComponent Component ON (Component.Id = Navigation.ComponentId)

GO
CREATE TABLE FrameworkLoginUser
(
	Id INT PRIMARY KEY IDENTITY,
    ApplicationId INT FOREIGN KEY REFERENCES FrameworkApplication(Id), /* User belongs to this application instance */
    ApplicationTypeId INT FOREIGN KEY REFERENCES FrameworkApplicationType(Id), /* BuiltIn User like Guest or Admin */
  	UserName NVARCHAR(256),
	Password NVARCHAR(256),
	IsBuiltIn BIT NOT NULL, /* BuiltIn User like Guest and Admin */
	IsBuiltInExist BIT NOT NULL,
	INDEX IX_FrameworkLoginUser UNIQUE (ApplicationId, ApplicationTypeId, UserName, IsBuiltIn)
)

GO
CREATE VIEW FrameworkLoginUserDisplay
AS
SELECT
	Data.Id,
	Data.ApplicationId,
	(SELECT Data2.Text FROM FrameworkApplication Data2 WHERE Data2.Id = Data.ApplicationId) AS ApplicationText,
	Data.ApplicationTypeId,
	(SELECT Data2.TypeName FROM FrameworkApplicationType Data2 WHERE Data2.Id = Data.ApplicationTypeId) AS ApplicationTypeName,
	Data.UserName,
	Data.Password,
	Data.IsBuiltIn,
	Data.IsBuiltInExist
FROM
	FrameworkLoginUser Data

GO
CREATE TABLE FrameworkLoginRole
(
	Id INT PRIMARY KEY IDENTITY,
    ApplicationId INT FOREIGN KEY REFERENCES FrameworkApplication(Id), /* User defined Role belongs to this application instance */
    ApplicationTypeId INT FOREIGN KEY REFERENCES FrameworkApplicationType(Id), /* BuiltIn Role like Guest or Admin */
  	RoleName NVARCHAR(256),
  	Description NVARCHAR(256),
	IsBuiltIn BIT NOT NULL, /* BuiltIn Role like Guest, Admin, Developer */
	IsBuiltInExist BIT NOT NULL,
	INDEX IX_FrameworkLoginrRole UNIQUE (ApplicationId, ApplicationTypeId, RoleName, IsBuiltIn)
)

GO
CREATE VIEW FrameworkLoginRoleDisplay
AS
SELECT
	Data.Id,
	Data.ApplicationId,
	Data.ApplicationTypeId,
	(SELECT ApplicationType.TypeName FROM FrameworkApplicationType ApplicationType WHERE ApplicationType.Id = Data.ApplicationTypeId) AS ApplicationTypeName,
	Data.RoleName,
	Data.Description,
	Data.IsBuiltIn,
	Data.IsBuiltInExist
FROM
	FrameworkLoginRole Data

GO
CREATE TABLE FrameworkLoginUserRole
(
	Id INT PRIMARY KEY IDENTITY,
    UserId INT FOREIGN KEY REFERENCES FrameworkLoginUser(Id) NOT NULL,
    RoleId INT FOREIGN KEY REFERENCES FrameworkLoginRole(Id) NOT NULL,
	IsBuiltIn BIT NOT NULL,
  	IsActive BIT NOT NULL
	INDEX IX_FrameworkLoginrRole UNIQUE (UserId, RoleId, IsBuiltIn)
)

GO
CREATE VIEW FrameworkLoginUserRoleDisplay
AS
SELECT
	Data.Id,
	Data.UserId,
	UserX.UserName,
	Data.RoleId,
	Role.RoleName,
	Role.Description AS RoleDescription,
	Data.IsBuiltIn,
	Data.IsActive,
	Application.Text,
	ApplicationType.TypeName AS ApplicationTypeName
FROM
	FrameworkLoginUserRole Data
LEFT JOIN
	FrameworkLoginUser UserX ON UserX.Id = Data.UserId
LEFT JOIN
	FrameworkLoginRole Role ON Role.Id = Data.RoleId
LEFT JOIN
	FrameworkApplication Application ON Application.Id = UserX.ApplicationId
CROSS APPLY
(
	SELECT * FROM FrameworkApplicationType ApplicationType WHERE ApplicationType.Id = UserX.ApplicationTypeId
	UNION ALL
	SELECT * FROM FrameworkApplicationType ApplicationType WHERE ApplicationType.Id = Application.ApplicationTypeId
) ApplicationType

GO
CREATE TABLE FrameworkLoginPermission
(
	Id INT PRIMARY KEY IDENTITY,
    ApplicationTypeId INT FOREIGN KEY REFERENCES FrameworkApplicationType(Id) NOT NULL, /* Permission belongs to this Application */
  	PermissionName NVARCHAR(256),
	Description NVARCHAR(256),
	IsExist BIT NOT NULL,
	INDEX IX_FrameworkLoginUser UNIQUE (ApplicationTypeId, PermissionName)
)

GO
CREATE VIEW FrameworkLoginPermissionDisplay
AS
SELECT
	Data.Id,
	Data.ApplicationTypeId,
	(SELECT ApplicationType.TypeName FROM FrameworkApplicationType ApplicationType WHERE ApplicationType.Id = Data.ApplicationTypeId) AS ApplicationTypeName,
	Data.PermissionName,
	Data.Description,
	Data.IsExist
FROM
	FrameworkLoginPermission Data

GO
CREATE TABLE FrameworkLoginRolePermission
(
	Id INT PRIMARY KEY IDENTITY,
    RoleId INT FOREIGN KEY REFERENCES FrameworkLoginRole(Id) NOT NULL,
    PermissionId INT FOREIGN KEY REFERENCES FrameworkLoginPermission(Id) NOT NULL,
	IsBuiltIn BIT NOT NULL,
  	IsActive BIT NOT NULL
	INDEX IX_FrameworkLoginrRole UNIQUE (RoleId, PermissionId, IsBuiltIn)
)

GO
CREATE VIEW FrameworkLoginRolePermissionDisplay
AS
SELECT 
	Data.Id,
	Data.RoleId,
	Data.PermissionId,
	Data.IsBuiltIn,
	Data.IsActive,
	Role.Id AS RoleRoleId,
	Role.RoleName AS RoleRoleName,
	Role.IsBuiltIn AS RoleIsBuiltIn,
	Role.IsBuiltInExist AS RoleIsBuiltInExist,
	Permission.Id AS PermissionPermissionId,
	Permission.PermissionName AS PermissionPermissionName,
	Permission.IsExist AS PermissionIsExist,
	ApplicationType.Id AS ApplicationTypeId,
	ApplicationType.TypeName AS ApplicationTypeTypeName
FROM
	FrameworkLoginRolePermission Data
LEFT JOIN
	FrameworkLoginRole Role ON Role.Id = Data.RoleId
LEFT JOIN
	FrameworkLoginPermission Permission ON Permission.Id = Data.PermissionId
LEFT JOIN
	FrameworkApplication Application ON Application.Id = Role.ApplicationId
CROSS APPLY
(
	SELECT * FROM FrameworkApplicationType ApplicationType WHERE ApplicationType.Id = Role.ApplicationTypeId
	UNION ALL
	SELECT * FROM FrameworkApplicationType ApplicationType WHERE ApplicationType.Id = Application.ApplicationTypeId
) ApplicationType

GO
CREATE VIEW FrameworkLoginUserRolePermissionDisplay
AS
SELECT
	UserX.Id AS UserId,
	UserX.IsBuiltIn AS UserIsBuiltIn,
	UserX.IsBuiltInExist AS UserIsBuiltInExist,
	Userx.ApplicationId,
	UserX.ApplicationTypeId,
	ApplicationType.TypeName AS ApplicationTypeName,
	Role.Id AS RoleId,
	Role.RoleName,
	Permission.Id AS PermissionId,
	Permission.PermissionName,
	Permission.IsExist
FROM
	FrameworkLoginUser UserX
LEFT JOIN
	FrameworkApplication Application ON Application.Id = UserX.ApplicationId
CROSS APPLY
(
	SELECT * FROM FrameworkApplicationType ApplicationType WHERE ApplicationType.Id = UserX.ApplicationTypeId
	UNION ALL
	SELECT * FROM FrameworkApplicationType ApplicationType WHERE ApplicationType.Id = Application.ApplicationTypeId
) ApplicationType
LEFT JOIN
	FrameworkLoginUserRole UserRole ON UserRole.UserId = UserX.Id
LEFT JOIN
	FrameworkLoginRole Role ON Role.Id = UserRole.RoleId
LEFT JOIN
	FrameworkLoginRolePermission RolePermission ON RolePermission.RoleId = Role.Id
LEFT JOIN
	FrameworkLoginPermission Permission ON Permission.Id = RolePermission.PermissionId

GO
CREATE TABLE FrameworkSession
(
	Id INT PRIMARY KEY IDENTITY,
  	Session UNIQUEIDENTIFIER NOT NULL UNIQUE,
	ApplicationId INT FOREIGN KEY REFERENCES FrameworkApplication(Id) NOT NULL,
	UserId INT FOREIGN KEY REFERENCES FrameworkLoginUser(Id) NOT NULL
)

GO
CREATE VIEW FrameworkSessionPermissionDisplay
AS
SELECT
	Session.Id AS SessionId,
	Session.Session AS Session,
	Session.UserId AS UserId,
	Application.Id AS ApplicationId,
	ApplicationType.Id AS ApplicationTypeId,
	ApplicationType.TypeName AS ApplicationTypeName,
	Permission.Id AS PermissionId,
	Permission.PermissionName AS PermissionName,
	Permission.IsExist AS PermissionIsExist
FROM
	FrameworkSession Session
LEFT JOIN
	FrameworkApplication Application ON Application.Id = Session.ApplicationId
LEFT JOIN
	FrameworkApplicationType ApplicationType ON ApplicationType.Id = Application.ApplicationTypeId
LEFT JOIN
	FrameworkLoginUserRole UserRole ON UserRole.UserId = Session.UserId
LEFT JOIN
	FrameworkLoginRolePermission RolePermission ON RolePermission.RoleId = UserRole.RoleId
LEFT JOIN
	FrameworkLoginPermission Permission ON Permission.Id = RolePermission.PermissionId

GO
CREATE PROCEDURE FrameworkLogin
(
	@Path NVARCHAR(256),
	@UserName NVARCHAR(256),
	@UserNameIsBuiltIn BIT,
	@Session UNIQUEIDENTIFIER
)
AS
BEGIN
	DECLARE @ApplicationId INT, @ApplicationTypeId INT
	
	SELECT 
		@ApplicationId = Id,
		@ApplicationTypeId = ApplicationTypeId 
	FROM 
		FrameworkApplication WHERE ISNULL(Path, '') = ISNULL(@Path, '')

	IF @ApplicationId IS NULL 
	BEGIN
		DECLARE @Message NVARCHAR(MAX) = 'Login failed! No path defined on sql table FrameworkApplication. (' + ISNULL(@Path, '') + ')';
		THROW 50000, @Message, 1
	END

	DECLARE @UserId INT 

	IF (@UserNameIsBuiltIn = 1)
		SET @UserId = (SELECT Id FROM FrameworkLoginUser WHERE ApplicationTypeId = @ApplicationTypeId AND UserName = @UserName)
	ELSE
		SET @UserId = (SELECT Id FROM FrameworkLoginUser WHERE ApplicationId = @ApplicationId AND UserName = @UserName)

	IF (@Session IS NULL)
	BEGIN
		SET @Session = NEWID()
		INSERT INTO FrameworkSession(Session, ApplicationId, UserId)
		SELECT @Session, @ApplicationId, @UserId
	END
	ELSE
	BEGIN
		UPDATE FrameworkSession SET ApplicationId = @ApplicationId, UserId = @UserId WHERE Session = @Session
	END

	SELECT * FROM FrameworkSessionPermissionDisplay WHERE Session = @Session
END
