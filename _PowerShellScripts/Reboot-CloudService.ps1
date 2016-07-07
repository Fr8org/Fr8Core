<#
    .SYNOPSIS
    The script reboots role instance on Azure Cloud Service.
#>

param(
    [Parameter(Mandatory = $false)]
	[string]$slot = "Staging",
    [Parameter(Mandatory = $true)]
	[string]$serviceName,
    [Parameter(Mandatory = $true)]
	[string]$instanceName
)

$ErrorActionPreference = 'Stop'

Reset-AzureRoleInstance -Slot $slot -ServiceName $serviceName -InstanceName $instanceName -Reboot