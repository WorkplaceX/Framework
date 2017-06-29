IF ((SELECT Version FROM FrameworkVersion) = 'v1.0')
BEGIN
	UPDATE FrameworkVersion SET Version = 'v0.0' WHERE Version = 'v1.0'
END
