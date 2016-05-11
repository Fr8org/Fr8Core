<#
    .SYNOPSIS
	The script configures terminal settings in the Azure Cloud Service configuration file,
	ServiceConfiguration.Release.cscfg. The following settings are configured: 
	* ConnectionString: is taken from the -connectionString argument with an optional application
	of -overrideDbName if actual database differs from what is specified in the connection string.
	* CoreWebServerUrl: is taken from the -coreWebServerUrl argument. 
	* {terminalName}.TerminalEndpoint: for each entry with such pattern in ServiceConfiguration.Release.cscfg
	a record will be taken in the Terminals table of the database. If the '-inheritEndpoints 1'
	attribute is specified, no values will be read from the database. Instead, each entry will be assigned an 
	empty string to force terminals to retrieve the value from web.config. 
#>

param(
    [Parameter(Mandatory = $true)]
	[string]$connectionString,

    [Parameter(Mandatory = $false)]
	[string]$overrideDbName,

	[Parameter(Mandatory = $false)]
	[string]$coreWebServerUrl,

	[Parameter(Mandatory = $false)]
	[string]$serviceName = "fr8company",

	[Parameter(Mandatory = $false)]
	[boolean]$updateService = $false,

	[Parameter(Mandatory = $false)]
	[int]$terminalVerson = 1,

	[Parameter(Mandatory = $false)]
	[boolean]$inheritEndpoints = $false
)

$ErrorActionPreference = 'Stop'

if ([System.String]::IsNullOrEmpty($overrideDbName) -ne $true) {
	$builder = new-object system.data.SqlClient.SqlConnectionStringBuilder($connectionString)
	$builder["Initial Catalog"] = $overrideDbName
	$connectionString = $builder.ToString()
}

# Get terminal list. Don't do it if just restoring default endpoints (enherited from web.config) 
# since in this case we only need to reset settings in the XML file to empty strings.
$terminalList = @{}
if ($inheritEndpoints -ne $true) {
	$commandText = 'SELECT Name, Endpoint FROM Terminals WHERE Version = ' + $terminalVerson

	$connection = new-object system.data.SqlClient.SQLConnection($connectionString)
	$command = new-object system.data.sqlclient.sqlcommand($commandText, $connection)
	$connection.Open()
	$command.CommandTimeout = 300 #5 minutes

	$reader = $command.ExecuteReader()
	while ($reader.read()) {
		try {
			$terminalList.Add($reader.GetString(0), $reader.GetString(1))
		}
		catch {
			#Ignore duplicates 
		}
	}
}

$RootDir = Split-Path -parent (Split-Path -parent $MyInvocation.MyCommand.Path)
$ConfigPath = $RootDir+"\terminalCloudService"
$ConfigFile = $ConfigPath+"\ServiceConfiguration.Release.cscfg"

if(Test-Path $ConfigFile)
{  
	Write-Host "Updating connection string in service configuration file: $ConfigFile" 
	$xml = [xml](Get-Content $ConfigFile)
	$roleNode = $xml.ServiceConfiguration.Role | where {$_.name -eq 'terminalWebRole'}

	# Update connection string
	$csNode = $roleNode.ConfigurationSettings.Setting | where {$_.name -eq 'Fr8.ConnectionString'}
	$csNode.value=$connectionString

	# Update CoreWebServerUrl
	$urlNode = $roleNode.ConfigurationSettings.Setting | where {$_.name -eq 'CoreWebServerUrl'}
	$urlNode.value=$coreWebServerUrl

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
			$_.value = $terminalList[$terminalName]
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
    Write-Warning "Azure Service Configuration file wasn't found in the folder $($ConfigFile)"
}



