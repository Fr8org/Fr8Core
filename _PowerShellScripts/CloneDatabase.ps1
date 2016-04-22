param(
    [string]$connectionString,
	[string]$sourceDbName,
	[string]$targetDbName,
	[string]$serverName
)

Write-Host "Deletes old target database if exists and creates a new one from the specified database."
$errorMessage = "An error while executing the query. Possibly cannot connect to the database to clone it. Please check connection string for CloneDatabase action."

$commandText = "IF (EXISTS (SELECT name FROM master.dbo.sysdatabases WHERE ('[' + name + ']' = '[$($targetDbName)]' OR name = '[$($targetDbName)]')))
					DROP DATABASE [$($targetDbName)]"

Write-Host $commandText

$connection = new-object system.data.SqlClient.SQLConnection($connectionString)
$command = new-object system.data.sqlclient.sqlcommand($commandText, $connection)
$connection.Open()
$command.CommandTimeout = 300 #5 minutes

if ($command.ExecuteNonQuery() -ne -1)
{
	Write-Host $errorMessage
	exit 1
}
Write-Host "Successfully deleted old target database."

$commandText = "RESTORE DATABASE [$($targetDbName)]
				FROM DISK = 'C:\DbBackups\Dev\4_19_2016.bak'
				WITH MOVE 'DockyardDB2_data' TO 'D:\MSSQLSERVER\$($targetDbName).mdf',
				MOVE 'DockyardDB2_log' TO 'D:\MSSQLSERVER\$($targetDbName).ldf'"

Write-Host $commandText
$command.CommandText = $commandText
if ($command.ExecuteNonQuery() -ne -1)
{
	Write-Host $errorMessage
	exit 1
}
Write-Host "Successfully cloned the database."

$connection.Close()