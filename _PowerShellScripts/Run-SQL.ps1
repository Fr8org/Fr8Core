<#
    .SYNOPSIS
	The script runs arbitrary SQL from a file.
#>
param(
    [Parameter(Mandatory = $true)]
	[string]$connectionString,

	[Parameter(Mandatory = $true)]
	[string]$pathToSql,

	[Parameter(Mandatory = $false)]
	[string]$overrideDbName
)

if ([System.String]::IsNullOrEmpty($overrideDbName) -ne $true) 
{
	$builder = new-object system.data.SqlClient.SqlConnectionStringBuilder($connectionString)
	$builder["Initial Catalog"] = $overrideDbName
	$connectionString = $builder.ToString()
}

$connection = new-object system.data.SqlClient.SQLConnection($connectionString)

$rootPath = Split-Path -parent (Split-Path -parent $MyInvocation.MyCommand.Path)

$commandText = Get-Content ([System.IO.Path]::Combine($rootPath, $pathToSql))

if ($commandText -eq $null)
{
	throw "Specified file does not contain any SQL"
}

$command = new-object system.data.sqlclient.sqlcommand($commandText, $connection)
$connection.Open()
$command.CommandTimeout = 60 #60 seconds
$command.ExecuteNonQuery()

Write-Host "Successfully finished running SQL."

$connection.Close()