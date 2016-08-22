namespace Fr8.Testing.Unit.Fixtures
{
    partial class FixtureData
    {
        public static string TestSqlScript_healthdemo()
        {
            return @"
                    USE master
                    GO
                    IF NOT EXISTS(SELECT 1 FROM SYSDATABASES WHERE name='demodb_health')
                    BEGIN
	                    PRINT 'Creating database ''demodb_health''...'

	                    CREATE DATABASE demodb_health

	                    PRINT 'Database ''demodb_health'' created successfully'
                    END
                    GO
                    USE demodb_health
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


            ";
        }
        
    }
}
