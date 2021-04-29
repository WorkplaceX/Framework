GO
CREATE TABLE HelloWorld
(
    Id INT PRIMARY KEY IDENTITY,
    Name NVARCHAR(256) UNIQUE,
    Text NVARCHAR(256),
    Number FLOAT
)

GO
CREATE VIEW HelloWorldIntegrate AS
SELECT 
    *, 
    Name AS IdName -- Key used to generate data in C# code and update data on database.
FROM 
    HelloWorld