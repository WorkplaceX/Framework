IF ((SELECT Version FROM FrameworkVersion) = 'v0.0')
BEGIN
	UPDATE FrameworkVersion SET Version = 'v1.0' WHERE Version = 'v0.0'
END
