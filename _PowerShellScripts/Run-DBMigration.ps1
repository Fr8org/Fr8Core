param(
	[Parameter(Mandatory = $true)]
	[string]$migrationToolPath,
	
	[Parameter(Mandatory = $true)]
	[string]$buildResultDirectory,
)
	

$RootDir = Split-Path -parent $PSCommandPath

$runMigrationCmd = "$migrationToolPath Data.dll /startupDirectory=`"$buildResultDirectory\bin`"  /startupConfigurationFile=`"$buildResultDirectory\web.config`"";

Write-Host $runMigrationCmd

Invoke-Expression $runMigrationCmd