<#
.SYNOPSIS
    Swap the deployments between production and staging of an Azure cloud service in a given subscription.
.DESCRIPTION
    Swaps staging and production slots of the specified Azure AppService.
#>

param(            
	# AppService name for the swap
	[Parameter(Mandatory = $true)] 
	[String]$serviceName               
)

Switch-AzureWebsiteSlot -Name $serviceName -Slot1 staging -Slot2 production -Force