param(
    [string]$connectionString,
	[string]$dbName
)

Write-Host "Deletes the specified database."
$errorMessage = "An error while executing the query. Please check connection string for the DeleteDatabase action."

$commandText = "
IF (EXISTS (SELECT name FROM master.dbo.sysdatabases WHERE ('[' + name + ']' = '[$($dbName)]' OR name = '[$($dbName)]')))
BEGIN
	ALTER DATABASE [$($dbName)] SET SINGLE_USER WITH ROLLBACK IMMEDIATE
	DROP DATABASE [$($dbName)]
END"

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
Write-Host "Successfully deleted the database."

$connection.Close()