/* Create tables and views */

IF EXISTS(SELECT * FROM sys.views WHERE name = 'TableName') DROP VIEW TableName

IF EXISTS(SELECT * FROM sys.views WHERE name = 'AirportDisplay') DROP VIEW AirportDisplay

IF EXISTS(SELECT * FROM sys.tables WHERE name = 'Airport') DROP TABLE Airport

IF EXISTS(SELECT * FROM sys.tables WHERE name = 'Country') DROP TABLE Country

CREATE TABLE Country
(
	Id INT PRIMARY KEY IDENTITY (1, 1),
	Text NVARCHAR(50),
	TextShort NVARCHAR(2),
	Continent NVARCHAR(2)
)

CREATE TABLE Airport
(
	Id INT PRIMARY KEY IDENTITY (1, 1),
	Text NVARCHAR(256),
	Code NVARCHAR(3),
	CountryId INT FOREIGN KEY REFERENCES Country(Id)
)

GO

CREATE VIEW AirportDisplay AS
SELECT
	Airport.Id AS AirportId,
	Airport.Text AS AirportText,
	Airport.Code AS AirportCode,
	Country.Id AS CountryId,
	Country.Text AS CountryText,
	Country.Continent AS CountryContinent

FROM
	Airport Airport

LEFT JOIN
	Country Country ON Country.Id = Airport.CountryId

GO

CREATE VIEW TableName AS
SELECT 
	OBJECT_SCHEMA_NAME(object_id) + '.' + name as TableName2, 
	CAST(0 AS BIT) AS IsView 

FROM 
	sys.tables

UNION ALL

SELECT 
	OBJECT_SCHEMA_NAME(object_id) + '.' + name as TableName2, 
	CAST(1 AS BIT) AS IsView 
	
FROM 
	sys.views
