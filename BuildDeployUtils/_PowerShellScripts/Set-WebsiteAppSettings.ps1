<#
    .SYNOPSIS
    The script changes Azure Websites (App Service) application settings. 
	To update an application setting, prefix its name with as:
	Supplying $null as a value will remove that setting from Azure. 
	
	.EXAMPLE
	.\Set-WebsiteAppSettings.ps1 -name fr8 -as:HubApiBaseUrl "http://localhost" -as:AzureSearchApiKey "IDKLSWOVC"
#>

$ErrorActionPreference = 'Stop'

$appSettings = @{}
$name = $null
$slot = "Staging"
$website = $null

For  ($i=0; $i -lt $args.Count; $i++) {
	if (($i -gt 0) -and ($args[$i - 1] -ieq "-as:")) {
		$appSettings.Add($args[$i], $args[$i + 1])
	}

	if ($i -gt 0 -and $args[$i - 1] -ieq ("-name")) {
		$name = $args[$i]
	}
	if ($i -gt 0 -and $args[$i - 1] -ieq ("-slot")) {
		$slot = $args[$i]
	}
}

try {
	$website = Get-AzureWebSite -name $name -slot $slot
}
catch [System.NullReferenceException]
{
	Write-Error "Cannot find website $name, slot '$slot'. Exiting." 
	exit 1
}

Write-Host "Updating website configuration: $name, slot '$slot'" 
$websiteAppSettings = $website.AppSettings
$websiteAppSettingsEnu = $appSettings.GetEnumerator()
ForEach($appSetting in $websiteAppSettingsEnu)
{
	try {
		$node = $websiteAppSettings | where {$_.Name -eq $appSetting.Name}

		if ($appSetting.Value -eq $null)
		{
			# Delete AppSetting if the value provided is null
			Write-Host ("Deleting the {0} AppSetting" -f $appSetting.Name)
			$websiteAppSettings.Remove($appSetting.Name);
		}
		else {
			Write-Host ("Updating the {0} AppSetting" -f $appSetting.Name)
			$websiteAppSettings[$appSetting.Name] = $appSetting.Value
		}
	}
	catch {
		Write-Error "Unable to set value for the AppSetting $appSetting.Name:  $_.Exception.Message"
	}
}

try
{
	Set-AzureWebsite -Slot $slot -Name $name -AppSettings $websiteAppSettings
	Write-Host "AppSettings updated."
}
catch
{
	Write-Host "Exception while saving AppSettings: $($_.Exception.Message)" 
	exit 1 
}   