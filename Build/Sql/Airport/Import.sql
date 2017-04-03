IF EXISTS(SELECT * FROM sys.views WHERE name = 'ImportCountry') DROP VIEW ImportCountry

GO

CREATE VIEW ImportCountry AS
SELECT 
	C.ValueText AS Text,
	B.ValueText AS TextShort,
	D.ValueText AS Continent

FROM
	ImportExcelDisplay A

LEFT JOIN
	ImportExcelDisplay B ON B.FileNameId = A.FileNameId AND B.SheetNameId = A.SheetNameId AND B.Row = A.Row AND B.ColumnName = 'B'

LEFT JOIN
	ImportExcelDisplay C ON C.FileNameId = A.FileNameId AND C.SheetNameId = A.SheetNameId AND C.Row = A.Row AND C.ColumnName = 'C'

LEFT JOIN
	ImportExcelDisplay D ON D.FileNameId = A.FileNameId AND D.SheetNameId = A.SheetNameId AND D.Row = A.Row AND D.ColumnName = 'D'

WHERE 
	A.SheetName = 'Country' AND A.ColumnName = 'A' AND A.ValueText IS NOT NULL AND A.Row >= 2

GO

IF EXISTS(SELECT * FROM sys.views WHERE name = 'ImportAirport') DROP VIEW ImportAirport

GO

CREATE VIEW ImportAirport AS
SELECT 
	D.ValueText AS Text,
	N.ValueText AS Code,
	I.ValueText AS CountryTextShort

FROM
	ImportExcelDisplay A

LEFT JOIN
	ImportExcelDisplay D ON D.FileNameId = A.FileNameId AND D.SheetNameId = A.SheetNameId AND D.Row = A.Row AND D.ColumnName = 'D'

LEFT JOIN
	ImportExcelDisplay N ON N.FileNameId = A.FileNameId AND N.SheetNameId = A.SheetNameId AND N.Row = A.Row AND N.ColumnName = 'N'

LEFT JOIN
	ImportExcelDisplay I ON I.FileNameId = A.FileNameId AND I.SheetNameId = A.SheetNameId AND I.Row = A.Row AND I.ColumnName = 'I'

WHERE 
	A.SheetName = 'Airport' AND A.ColumnName = 'A' AND A.ValueText IS NOT NULL AND A.Row >= 2

GO

INSERT INTO Country (Text, TextShort, Continent)
SELECT Text, TextShort, Continent FROM ImportCountry

GO

INSERT INTO Airport (Text, Code, CountryId)
SELECT 
	Airport.Text,
	Airport.Code,
	(SELECT Country.Id FROM Country Country WHERE Country.TextShort = Airport.CountryTextShort) AS CountryId 

FROM
	ImportAirport Airport
