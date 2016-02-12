<#
    .SYNOPSIS
    The script updates terminal hostname in the database to prepare it for integration testing.
	Called during CI process for non-Dev/Master builds. 
#>
param(
    [string]$connectionString,
	[string]$newHostname
)

Write-Host "Waiting for 30 seconds after the previous action to allow the test database to activate."
Start-Sleep 30000
Write-Host "Update terminal URLs to $newHostname"
$errorMessage = "An error while executing the query. Possibly cannot connect to the database, please check connection string."

$commandText = "UPDATE Terminals SET [Endpoint] = '$newHostname" + ":' + RIGHT([Endpoint], 5)"
Write-Host $commandText

$connection = new-object system.data.SqlClient.SQLConnection($connectionString)
$command = new-object system.data.sqlclient.sqlcommand($commandText, $connection)
$connection.Open()
$command.CommandTimeout = 10 #10 seconds

if ($command.ExecuteNonQuery() -ne -1)
{
	Write-Host $errorMessage
	exit 1
}
Write-Host "Successfully updated terminal hostname."

$connection.Close()