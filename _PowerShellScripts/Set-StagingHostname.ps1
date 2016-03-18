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
	
	[Parameter(Mandatory = $false)]
	[string]$overrideDbName
)
	
$ErrorActionPreference = 'Stop'
$rootDir = Split-Path -parent $PSCommandPath
$deployment = Get-AzureDeployment -ServiceName $serviceName -Slot Staging
$hostName = $deployment.Url.Host
Write-Host $hostName

$commandLine = "$rootDir\UpdateTerminalHostnameInDb.ps1 -connectionString '$connectionString' -newHostname $hostName"
if ([String]::IsNullOrEmpty($overrideDbName) -eq $false) {
	$commandLine +=  " -overrideDbName $overrideDbName"
}
Invoke-Expression "$rootDir\UpdateTerminalUrl.ps1 $hostName"
Invoke-Expression $commandLine
