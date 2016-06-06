<#
    .SYNOPSIS
    The script retrieves the hostname of the staging Cloud Service deployment 
	and assigns it to the STAGING_HOSTNAME environment variable.

	Add this script to VSO as an Azure Power Shell task since it requires Azure subscription info.
#>

param(
    [Parameter(Mandatory = $true)]
	[string]$serviceName
)

$deployment = Get-AzureDeployment -ServiceName $serviceName -Slot Staging
$env:STAGING_HOSTNAME = $deployment.Url.Host