param(
	[Parameter(Mandatory = $true)]
	[string]$migrationToolPath,
	
	[Parameter(Mandatory = $true)]
	[string]$newConnectionString,
	
	[Parameter(Mandatory = $false)]
	[string]$buildResultDirectory
)
	

$RootDir = Split-Path -parent $PSCommandPath


$cfg = [xml](gc $RootDir\..\web.config)

$con= $cfg.configuration.connectionStrings.add|?{$_.name -eq "DockyardDB"};
# Replace the content
$con.connectionString = $newConnectionString

$cfg.Save("$RootDir\..\_Web.config");

$runMigrationCmd = "$RootDir\..\$migrationToolPath Data.dll /startupDirectory=`"$RootDir\..\$buildResultDirectory\bin`"  /startupConfigurationFile=`"$RootDir\..\$buildResultDirectory\_web.config`"";

Write-Host $runMigrationCmd

Invoke-Expression $runMigrationCmd