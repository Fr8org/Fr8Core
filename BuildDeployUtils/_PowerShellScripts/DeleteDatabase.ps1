param(
    [string]$connectionString,
	[string]$dbName
)

Write-Host "Deletes the specified database."
$errorMessage = "An error while executing the query. Please check connection string for the DeleteDatabase action."

$commandText = "DECLARE @kill varchar(8000) = ''; SELECT @kill = @kill + 'kill ' + CONVERT(varchar(5), spid) + ';' FROM master..sysprocesses WHERE dbid = db_id('$dbName') EXEC(@kill); "
$commandText += "DROP DATABASE IF EXISTS [$dbName]"
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