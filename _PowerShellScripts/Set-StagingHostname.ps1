<#
    .SYNOPSIS
    The script updates terminal hostname in the database to prepare it for integration testing.
	The hostname is taken from the URL of the Staging copy of the Cloud Service. 

	Add this script to VSO as an Azure Power Shell task since it requires Azure subscription info.
#>
param(
    [Parameter(Mandatory = $true)]
	[string]$serviceName,

    [Parameter(Mandatory = $true)]
	[string]$connectionString,

	[Parameter(Mandatory = $true)]
	[ValidateSet("db", "txt", "both")]
	[string]$update,
	
	[Parameter(Mandatory = $false)]
	[string]$overrideDbName
)
	
$ErrorActionPreference = 'Stop'
$rootDir = Split-Path -parent $PSCommandPath
$deployment = Get-AzureDeployment -ServiceName $serviceName -Slot Staging
$hostName = $deployment.Url.Host
Write-Host $hostName

if (($update.ToLowerInvariant() -eq 'db') -or ($update.ToLowerInvariant() -eq 'both')) {
	$commandLine = "$rootDir\Update-TerminalHostnameInDb.ps1 -connectionString '$connectionString' -slot sta -stagingHostname http://$hostName"
	if ([String]::IsNullOrEmpty($overrideDbName) -eq $false) {
		$commandLine +=  " -overrideDbName $overrideDbName"
	}
	Invoke-Expression $commandLine
}

if (($update.ToLowerInvariant() -eq 'txt') -or ($update.ToLowerInvariant() -eq 'both')) {
	$commandLine = "$rootDir\UpdateTerminalUrl.ps1 -connectionString '$connectionString'  -newHostName $hostName";
	
	if ([String]::IsNullOrEmpty($overrideDbName) -eq $false) {
		$commandLine +=  " -overrideDbName $overrideDbName"
	}
	Invoke-Expression $commandLine
}