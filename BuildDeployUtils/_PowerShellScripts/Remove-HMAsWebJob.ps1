<#
    .SYNOPSIS
    The script deletes the Azure Web Job which runs HealthMonitor. 

	Add this script to VSO as an Azure Power Shell task since it requires Azure subscription info.
#>


param(
    [Parameter(Mandatory = $false)]
	[string]$siteName = "fr8",

    [Parameter(Mandatory = $false)]
	[string]$jobName ="HealthMonitor",

    [Parameter(Mandatory = $false)]
	[string]$slot = "Staging"
)

Remove-AzureWebsiteJob -JobName $jobName -Name $siteName -Slot $slot -JobType triggered -Force
