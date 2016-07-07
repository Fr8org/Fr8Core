<#
    .SYNOPSIS
    The script retrieves the hostname of the staging Cloud Service deployment, adds port number 
	of Plan Directory and calls the Set-Config script to save it to HealthMonitor app.config.

	Add this script to VSO as an Azure Power Shell task since it requires the Azure subscription context.
#>

param(
    [Parameter(Mandatory = $true)]
	[string]$serviceName,

    [Parameter(Mandatory = $true)]
	[string]$filePath	
)

$ErrorActionPreference = 'Stop'

$planDirectoryPort = 64879 # Defined in ServiceDefinition.csdef
$rootDir = Split-Path -parent $PSCommandPath

$deployment = Get-AzureDeployment -ServiceName $serviceName -Slot Staging
$stagingHostname = $deployment.Url.Host
$planDirectoryURL = "http://{0}:{1}" -f $stagingHostname, $planDirectoryPort 
Echo "Plan Directory URL: $planDirectoryURL"
& "$rootDir\Set-Config.ps1" -as:PlanDirectoryBaseUrl "$planDirectoryURL" -as:PlanDirectoryBaseApiUrl "$planDirectoryURL/api/" -filePath "$filePath"