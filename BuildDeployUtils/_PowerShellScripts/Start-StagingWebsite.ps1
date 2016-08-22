param(
    [Parameter(Mandatory = $true)]
	[string]$serviceName
)
	
$ErrorActionPreference = 'Stop'
Start-AzureWebsite -Name $serviceName -Slot Staging 