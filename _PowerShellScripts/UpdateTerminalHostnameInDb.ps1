<#
    .SYNOPSIS
  	THIS SCRIPT FILE IS OBSOLETE. THE BUILD SYSTEM IS BEING MIGRATED TO THE SCRIPT Update-TerminalHostnameInDb.ps1.
	Please do any changes in that script. 
	The script updates terminal hostname in the database to prepare it for integration testing.
	Called during CI process for non-Dev/Master builds. 
#>
param(
    [Parameter(Mandatory = $true)]
	[string]$connectionString,

	[Parameter(Mandatory = $true)]
	[string]$newHostname,

	[Parameter(Mandatory = $false)]
	[string]$overrideDbName
)

Write-Host "Update terminal URLs to $newHostname"

$commandText = "UPDATE Terminals SET [Endpoint] = 
('$newHostname' +
		(CASE when CHARINDEX (':', REVERSE ([Endpoint])) = 0
		    then ''
		else 
			RIGHT ([Endpoint], CHARINDEX (':', REVERSE ([Endpoint])))
		END))
		
		WHERE 
		
		"
Write-Host $commandText 

if ([System.String]::IsNullOrEmpty($overrideDbName) -ne $true) {
	$builder = new-object system.data.SqlClient.SqlConnectionStringBuilder($connectionString)
	$builder["Initial Catalog"] = $overrideDbName
	$connectionString = $builder.ToString()
}

$connection = new-object system.data.SqlClient.SQLConnection($connectionString)

$command = new-object system.data.sqlclient.sqlcommand($commandText, $connection)
$connection.Open()
$command.CommandTimeout = 20 #20 seconds
$command.ExecuteNonQuery()


$commandText = "
IF (EXISTS (SELECT * 
                 FROM INFORMATION_SCHEMA.TABLES 
                 WHERE TABLE_SCHEMA = 'dbo' 
                 AND  TABLE_NAME = 'TerminalRegistration'))
BEGIN
	DELETE from TerminalRegistration where UserId is not null;
	
	WITH cte
    AS (SELECT ROW_NUMBER() OVER (PARTITION BY 
     ('$newHostname' +
        (CASE when CHARINDEX (':', REVERSE ([Endpoint])) = 0
            then ''
        else 
            RIGHT ([Endpoint], CHARINDEX (':', REVERSE ([Endpoint])))end ))
     ORDER BY ( SELECT 0)) RN
        FROM   TerminalRegistration)
	delete FROM cte
	WHERE  RN > 1

    UPDATE TerminalRegistration SET [Endpoint] = 
			('$newHostname' +
		(CASE when CHARINDEX (':', REVERSE ([Endpoint])) = 0
		    then ''
		else 
			RIGHT ([Endpoint], CHARINDEX (':', REVERSE ([Endpoint])))
		END))
END";

$command = new-object system.data.sqlclient.sqlcommand($commandText, $connection)
$command.CommandTimeout = 20 #20 seconds
$command.ExecuteNonQuery()

Write-Host "Successfully updated terminal hostname."

$connection.Close()