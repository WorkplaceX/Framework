GO
CREATE TABLE Doc.StorageFile
(
	Id INT PRIMARY KEY IDENTITY,
	FileName NVARCHAR(256) UNIQUE,
	Data VARBINARY(MAX),
    DataImageThumbnail VARBINARY(MAX),
	Description NVARCHAR(MAX),
	SourceUrl NVARCHAR(512),
	IsIntegrate BIT NOT NULL,
	IsDelete BIT NOT NULL
)
GO
CREATE VIEW Doc.StorageFileIntegrate AS
SELECT
    *,
    FileName AS IdName
FROM
    Doc.StorageFile Data
