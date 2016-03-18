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
	[int]$terminalVerson = 1
)

$ErrorActionPreference = 'Stop'

if ([System.String]::IsNullOrEmpty($overrideDbName) -ne $true) {
	$builder = new-object system.data.SqlClient.SqlConnectionStringBuilder($connectionString)
	$builder["Initial Catalog"] = $overrideDbName
	$connectionString = $builder.ToString()
}


# Get terminal list 
$terminalList = @{}
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
        $_.value = $terminalList[$terminalName]
        Write-Host "$terminalName  - "$_.value
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



