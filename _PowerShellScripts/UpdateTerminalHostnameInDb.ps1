<#
    .SYNOPSIS
    The script updates terminal hostname in the database to prepare it for integration testing.
	Called during CI process for non-Dev/Master builds. 
#>
param(
    [string]$connectionString,
	[string]$newHostname,
	[string]$overrideDbName
)

Write-Host "Update terminal URLs to $newHostname"

$commandText = "UPDATE Terminals SET [Endpoint] = '$newHostname" + ":' + RIGHT([Endpoint], 5)"
Write-Host $commandText

$builder = new SqlConnectionStringBuilder(connectionString);
builder.InitialCatalog = $overrideDbName;
$connectionString = builder.ToString;

$connection = new-object system.data.SqlClient.SQLConnection($connectionString)
$command = new-object system.data.sqlclient.sqlcommand($commandText, $connection)
$connection.Open()
$command.CommandTimeout = 20 #20 seconds

$command.ExecuteNonQuery()
Write-Host "Successfully updated terminal hostname."

$connection.Close()