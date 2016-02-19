param(
    [string]$connectionString,
	[string]$databaseName,
	[string]$serverName
)

Write-Host "Delete old staging database if exists and create a new one from the current PROD database."
$errorMessage = "An error while executing the query. Possibly cannot connect to the database to clone it for the Staging slot. Please check connection string for SetupStagingDatabase action in the Master (Release) build pipeline."

$commandText = "DECLARE @kill varchar(8000) = ''; SELECT @kill = @kill + 'kill ' + CONVERT(varchar(5), spid) + ';' FROM master..sysprocesses WHERE dbid = db_id('$($databaseName)Staging') EXEC(@kill); "
$commandText += "DROP DATABASE IF EXISTS $($databaseName)Staging"
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
Write-Host "Successfully deleted old staging database."

$commandText = "CREATE DATABASE $($databaseName)Staging AS COPY OF $($serverName).$($databaseName) (SERVICE_OBJECTIVE = 'basic');"
Write-Host $commandText
$command.CommandText = $commandText
if ($command.ExecuteNonQuery() -ne -1)
{
	Write-Host $errorMessage
	exit 1
}
Write-Host "Successfully created a new staging database."

$connection.Close()

