<#
    .SYNOPSIS
    The script updates terminal hostname in the database to prepare it for integration testing.
	Called during CI process for non-Dev/Master builds. 
#>
param(
    [Parameter(Mandatory = $true)]
	[string]$connectionString,

	[Parameter(Mandatory = $false)]
	[string]$stagingHostname,

	[Parameter(Mandatory = $false)]
	[ValidateSet("staging", "sta", "production", "prod")]
	[string]$slot,

	[Parameter(Mandatory = $false)]
	[string]$overrideDbName
)

$ErrorActionPreference = 'Stop'

$rootDir = Split-Path -parent (Split-Path -parent $MyInvocation.MyCommand.Path)
$configPath = $rootDir+"\terminalCloudService"
$defFile = $configPath+"\ServiceDefinition.csdef"

if ([System.String]::IsNullOrEmpty($overrideDbName) -ne $true) {
	$builder = new-object system.data.SqlClient.SqlConnectionStringBuilder($connectionString)
	$builder["Initial Catalog"] = $overrideDbName
	$connectionString = $builder.ToString()
}

if(-not (Test-Path $defFile))
{  
	throw "Cloud Service configuration file is not found."
}

$connection = new-object system.data.SqlClient.SQLConnection($connectionString)
$command = new-object system.data.sqlclient.sqlcommand
$command.Connection = $connection
$connection.Open()
$command.CommandTimeout = 20 #20 seconds

if ($slot -contains "sta")
{
	if([string]::IsNullOrEmpty($stagingHostname))
	{
		throw "Specify -stagingHostname for Staging configuration"
	}

	$xml = [xml](Get-Content $defFile)
	$roleNode = $xml.ServiceDefinition.WebRole | where {$_.name -eq 'terminalWebRole'}
	$terminalEndpointSettings = $roleNode.Endpoints.InputEndpoint | where {($_.name -like 'terminal*') -and ($_.protocol -like 'http')}
    $terminalEndpointSettings | ForEach-Object {
		$terminalPort = $_.port
		$terminalName = $_.name
		$command.CommandText = "UPDATE Terminals SET Endpoint = '$stagingHostname" + ":$terminalPort' WHERE Name = '$terminalName'"
		Write-Host "Executing: " + $command.CommandText
		$command.ExecuteNonQuery()
    }
}

if ($slot -contains "prod")
{
	$command.CommandText = "UPDATE Terminals SET Endpoint = 'https://' + LOWER(Name) + '.fr8.co'"
	Write-Host "Executing: " + $command.CommandText
	$command.ExecuteNonQuery()
}

Write-Host "Successfully updated terminal hostname."

$connection.Close()