<#
    .SYNOPSIS
	The script configures terminal settings in the Azure Cloud Service configuration file,
	ServiceConfiguration.Release.cscfg. The following settings are configured: 
	* {terminalName}.TerminalEndpoint: for each entry with such pattern in ServiceConfiguration.Release.cscfg
	a record will be taken in the Terminals table of the database. If the '-inheritEndpoints 1'
	attribute is specified, no values will be read from the database. Instead, each entry will be assigned an 
	empty string to force terminals to retrieve the value from web.config. 
	* Any settings which are provided in the -as: argument. Example for CoreWebsiteUrl: -as:CoreWebsiteUrl http://localhost
	* Connection string. In order to insert connection string, use the following syntax: -as:ConnectionStringSettingName {cs}. 
	The {cs} pattern will be replaced with the connection string, optionally with another database name 
	specified using the -overrideDbName argument. 
	* Staging hostname. In order to insert current Staging hostname, use the following syntax: -as:StagingHostnameSettingName {staurl}. 
	The {staurl} pattern will be replaced with the hostname.

	This script requires that the Azure context is present so use the Azure PowerShell action in VSO to add the script. 
#>

$ErrorActionPreference = 'Stop'

$appSettings = @{}
$requiresCloudService = $false

For  ($i=0; $i -lt $args.Count; $i++) {

	if (($i -gt 0) -and ($args[$i - 1] -ieq "-as:")) {
		if ($args[$i + 1] -like "*{staurl}*")
		{
			$requiresCloudService = $true;
		}
		$appSettings.Add($args[$i], $args[$i + 1])
	}

	if ($i -gt 0 -and $args[$i - 1] -ieq ("-connectionString")) {
		$connectionString = $args[$i]
	}

	if ($i -gt 0 -and $args[$i - 1] -ieq ("-overrideDbName")) {
		$overrideDbName = $args[$i]
	}

	if ($i -gt 0 -and $args[$i - 1] -ieq ("-serviceName")) {
		$serviceName = $args[$i]
	}	
	
	if ($i -gt 0 -and $args[$i - 1] -ieq ("-updateService")) {
		$updateService = [bool]$args[$i]
	}

	if ($i -gt 0 -and $args[$i - 1] -ieq ("-inheritEndpoints")) {
		$inheritEndpoints = [bool]$args[$i]
	}
}

if (([System.String]::IsNullOrEmpty($connectionString) -ne $true) -and ([System.String]::IsNullOrEmpty($overrideDbName) -ne $true)) {
	$builder = new-object system.data.SqlClient.SqlConnectionStringBuilder($connectionString)
	$builder["Initial Catalog"] = $overrideDbName
	$connectionString = $builder.ToString()
}

$RootDir = Split-Path -parent (Split-Path -parent $MyInvocation.MyCommand.Path)
$ConfigPath = $RootDir+"\terminalCloudService"
$ConfigFile = $ConfigPath+"\ServiceConfiguration.Release.cscfg"
$epConfigFile = $ConfigPath+"\ServiceDefinition.csdef"

if ($updateService -or (-not $inheritEndpoints) -or $requiresCloudService)
{
	if ($serviceName -eq $null)
	{
		Write-Error "This operation requires access to the Cloud Service. Please specify -serviceName argument."
	}
	else 
	{
		$deployment = Get-AzureDeployment -ServiceName $serviceName -Slot Staging
		$stagingHostname = $deployment.Url.Host
	}
}

# Get terminal list. Don't do it if just restoring default endpoints (enherited from web.config) 
# since in this case we only need to reset settings in the XML file to empty strings.
$terminalList = @{}

if ($inheritEndpoints -ne $true) {
		
	$xml = [xml](Get-Content $epConfigFile)
	$roleNode = $xml.ServiceDefinition.WebRole | where {$_.name -eq 'terminalWebRole'}
	$terminalEndpointSettings = $roleNode.Endpoints.InputEndpoint | where {($_.name -like 'terminal*') -and ($_.protocol -like 'http')}
    $terminalEndpointSettings | ForEach-Object {
		$terminalPort = $_.port
		$terminalName = $_.name
				
		try {
			$terminalList.Add($terminalName, $stagingHostname + ":" + $terminalPort)
		}
		catch {
			#Ignore duplicates 
		}
   }
}

if(Test-Path $ConfigFile)
{  
	Write-Host "Updating connection string in service configuration file: $ConfigFile" 
	$xml = [xml](Get-Content $ConfigFile)
	$roleNode = $xml.ServiceConfiguration.Role | where {$_.name -eq 'terminalWebRole'}

	$settings = $roleNode.ConfigurationSettings.Setting

	# Update settings
	ForEach($item in $appSettings.GetEnumerator())
	{
		Write-Host ("Updating {0} value" -f $item.Name) 
		$node = $settings | where {$_.name -eq $item.Name}
		if ($node -ne $NULL)
		{
			# Handle reference to Staging Cloud Service URL 
			if ($item.Value.Contains("{staurl}"))
			{
				# Assign Staging endpoint hostname to the setting
				if ($serviceName -ne $null)
				{
					$node.Value = $item.Value -replace "{staurl}", $stagingHostname 
				}
				else 
				{
					Write-Error "-serviceName argument must be specified in order to set staging endpoint URL ('staurl:')"
					exit 1
				}
			}
			# Handle reference to Connection String
			elseif ($item.Value.Contains("{cs}"))
			{
				# Assign connection string to the setting
				if ($connectionString -ne $null)
				{
					$node.Value = $item.Value -replace "{cs}", $connectionString 
				}
				else 
				{
					Write-Error "-connectionString argument must be specified in order to insert connection string"
					exit 1
				}
			}
			else 
			{
				$node.Value = $item.Value
			}
		}
	}


	# Update TerminalEndpoint
    Write-Host "New terminal endpoint URLs:"
	$terminalEndpointSettings = $roleNode.ConfigurationSettings.Setting | where {$_.name -like '*.TerminalEndpoint'}
    $terminalEndpointSettings | ForEach-Object {
        $terminalName = ($_.name -split "\." )[0]
		if ($inheritEndpoints -eq $true) {
			$_.value = ""
			Write-Host "$terminalName - inherit from web.config" 
		}
		else {
			$_.value = "http://" + $terminalList[$terminalName]
			Write-Host "$terminalName  - "$_.value
		}
    }

	try
	{
		$xml.Save($ConfigFile)
		Write-Host "File updated."
	}
	catch
	{
		Write-Host "Exception while saving $($ConfigFile) - $($_.Exception.Message)" 
		exit 1 
	}   

	if ($updateService) {
		Write-Host "Uploading and applying the file to Azure Cloud Service..."
		Set-AzureDeployment -Config -ServiceName $serviceName -Slot "Staging" -Configuration $ConfigFile -ErrorAction Ignore 
		Write-Host "Azure Cloud Service $serviceName updated"
	}
}
else
{
    throw "Azure Service Configuration file wasn't found in the folder $($ConfigFile)"
}



