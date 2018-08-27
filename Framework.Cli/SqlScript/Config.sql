CREATE TABLE FrameworkConfigGrid
(
	Id INT PRIMARY KEY IDENTITY,
	TypeName NVARCHAR(256),
	RowCountMax INT,
	IsInsert BIT,
	INDEX IX_FrameworkConfigGrid UNIQUE (TypeName)
)

CREATE TABLE FrameworkConfigColumn
(
	Id INT PRIMARY KEY IDENTITY,
	TypeName NVARCHAR(256),
	FieldName NVARCHAR(256),
	Text NVARCHAR(256), -- Column header text.
	Description NVARCHAR(256), -- Column header text.
	IsVisible BIT,
	IsReadOnly BIT,
	Sort FLOAT,
	INDEX IX_FrameworkConfigColumn UNIQUE (TypeName, FieldName)
)
