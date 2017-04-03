IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID('TempExcelDisplay') AND type = 'V') DROP VIEW TempExcelDisplay

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID('TempExcel') AND type = 'U') DROP TABLE TempExcel
