<#
    .SYNOPSIS
    The script updates terminal hostname in the database to prepare it for integration testing.
	The hostname is taken from the URL of the Staging copy of the Cloud Service. 

	Add this script to VSO as an Azure Power Shell task since it requires Azure subscription info.
#>
param(
	[string]$serviceName,
    [string]$connectionString,
	[string]$overrideDbName
)
	
$ErrorActionPreference = 'Stop'
  
$deployment = Get-AzureDeployment -ServiceName $serviceName -Slot Staging
$hostName = $deployment.Url.Host
Invoke-Expression ".\UpdateTerminalHostnameInDb.ps1 `
	-connectionString '$connectionString' `
	-newHostname $hostName `
	-overrideDbName $overrideDbName"