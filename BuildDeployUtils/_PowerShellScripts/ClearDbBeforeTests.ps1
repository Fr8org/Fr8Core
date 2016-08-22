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

	[Parameter(Mandatory = $false)]
	[string]$overrideDbName
)

Write-Host "Remove dev specific data from the database to speedup tests"

$commandText = "delete MtData from MtData inner join MtTypes on MtTypes.Id = MtData.Type where MtTypes.ManifestId = 42"
$commandText = "delete from TerminalRegistration where UserId is not null"

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
Write-Host "Successfully removed odd data from the database"

$connection.Close()