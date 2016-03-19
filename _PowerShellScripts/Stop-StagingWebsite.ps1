param(
    [Parameter(Mandatory = $true)]
	[string]$serviceName
)
	
$ErrorActionPreference = 'Stop'
Stop-AzureWebsite -Name $serviceName -Slot Staging 