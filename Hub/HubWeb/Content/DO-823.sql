USE master
GO
IF NOT EXISTS(SELECT 1 FROM SYSDATABASES WHERE name='healthdemoDb')
BEGIN
	PRINT 'Creating database ''healthdemoDb''...'

	CREATE DATABASE healthdemoDb

	PRINT 'Database ''healthdemoDb'' created successfully'
END
GO
USE healthdemoDb
GO
IF EXISTS(SELECT 1 FROM SYSOBJECTS O WHERE O.name=N'Patients' AND O.type=N'U')
BEGIN
	PRINT 'Droping table ''Patients''...'

	DROP TABLE Patients

	PRINT 'Table ''Patients'' dropped successfully'
END
GO

PRINT 'Creating table ''Patients''...'

CREATE TABLE Patients
(
	Name VARCHAR(250),
	DateOfBirth VARCHAR(50),
	Doctor VARCHAR(250),
	Condition VARCHAR(500)
)

PRINT 'Table ''Patients'' created successfully'