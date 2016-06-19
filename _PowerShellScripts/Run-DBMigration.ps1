param(
	[Parameter(Mandatory = $true)]
	[string]$migrationToolPath,
	
	[Parameter(Mandatory = $false)]
	[string]$buildResultDirectory
)
	

$RootDir = Split-Path -parent $PSCommandPath

$runMigrationCmd = "$RootDir\..\$migrationToolPath Data.dll /startupDirectory=`"$RootDir\..\$buildResultDirectory\bin`"  /startupConfigurationFile=`"$RootDir\..\$buildResultDirectory\web.config`"";

Write-Host $runMigrationCmd

Invoke-Expression $runMigrationCmd