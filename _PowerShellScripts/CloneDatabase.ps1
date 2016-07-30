param(
    [string]$connectionString,
	[string]$sourceDbName,
	[string]$targetDbName,
	[string]$serverName,
	[Parameter(Mandatory = $false)]
	[string]$serviceObjective
)

Write-Host "Deletes old target database if exists and creates a new one from the specified database."
$errorMessage = "An error while executing the query. Possibly cannot connect to the database to clone it. Please check connection string for CloneDatabase action."

$commandText = "DECLARE @kill varchar(8000) = ''; SELECT @kill = @kill + 'kill ' + CONVERT(varchar(5), spid) + ';' FROM master..sysprocesses WHERE dbid = db_id('$($targetDbName)') EXEC(@kill); "
$commandText += "DROP DATABASE IF EXISTS [$($targetDbName)]"
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

Write-Host "Successfully deleted old target database. 120 sec delay to let the database to delete..."

Start-Sleep -Seconds 120

if ([System.String]::IsNullOrEmpty($serviceObjective) -eq $true) 
{
	$serviceObjective = "S1";
}

$commandText = "CREATE DATABASE [$($targetDbName)] AS COPY OF [$($serverName)].[$($sourceDbName)] (EDITION='standard', SERVICE_OBJECTIVE = '$serviceObjective');"
Write-Host $commandText
$command.CommandText = $commandText
if ($command.ExecuteNonQuery() -ne -1)
{
	Write-Host $errorMessage
	exit 1
}
Write-Host "Successfully cloned the database."

$connection.Close()