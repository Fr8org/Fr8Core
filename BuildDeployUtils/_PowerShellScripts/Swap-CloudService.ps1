<#
.SYNOPSIS
    Swap the deployments between production and staging of an Azure cloud service in a given subscription.
.DESCRIPTION
    First check to see if the staging slot of given cloud service is deployed already. If it is deployed, then swap the 
    deployments between production and staging. This is useful to roll back to the previous deployment kept in staging slot.
	Adapted from: https://gallery.technet.microsoft.com/scriptcenter/Swap-the-Deployments-f588a811
#>

param(            
	# cloud service name for the swap
	[Parameter(Mandatory = $true)] 
	[String]$ServiceName               
)

$Start = [System.DateTime]::Now
"Starting: " + $Start.ToString("HH:mm:ss.ffffzzz")

$Deployment = Get-AzureDeployment -Slot "Staging" -ServiceName $ServiceName
if ($Deployment -ne $null -AND $Deployment.DeploymentId  -ne $null)
{
	Write-Output ("Staging slot of {0} is deployed at {1}" -f $ServiceName, $Deployment.CreatedTime.ToString("HH:mm:ss.ffffzzz"))
	Write-Output ("Deployment label: {0}" -f $Deployment.Label)
	$MoveStatus = Move-AzureDeployment -ServiceName $ServiceName
	Write-Output ("Vip swap of {0} status: {1}" -f $ServiceName, $MoveStatus.OperationStatus)    
}else
{
	Write-Output ("There is no deployment in staging slot of {0} to swap." -f $ServiceName)
}
    
$Finish = [System.DateTime]::Now
$TotalUsed = $Finish.Subtract($Start).TotalSeconds
   
Write-Output ("VIP swapped cloud service {0} in subscription {1} in {2} seconds." -f $ServiceName, $SubscriptionName, $TotalUsed)
		"Finished " + $Finish.ToString("HH:mm:ss.ffffzzz")