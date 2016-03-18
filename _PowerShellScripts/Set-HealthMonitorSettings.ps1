param(
	[Parameter(Mandatory = $true)]
	[string]$hubApiBaseUrl,

	[Parameter(Mandatory = $true)]
    [string]$connectionString,	

	[Parameter(Mandatory = $true)]
	[string]$overrideDbName,

	[Parameter(Mandatory = $true)]
	[string]$filePath
)

if ([System.String]::IsNullOrEmpty($overrideDbName) -ne $true) {
	$builder = new-object system.data.SqlClient.SqlConnectionStringBuilder($connectionString)
	$builder["Initial Catalog"] = $overrideDbName
	$connectionString = $builder.ToString()
}

$rootDir = Split-Path -parent (Split-Path -parent $MyInvocation.MyCommand.Path)
$configPath = [System.IO.Path]::Combine($rootDir, $filePath)

if(Test-Path $configPath)
{  
	Write-Host "HealthMonitor configuration file: $configPath" 
	$xml = [xml](Get-Content $configPath)
   
	Write-Host "Updating HubApiBaseUrl value" 
	$urlNode = $xml.configuration.appSettings.add | where {$_.key -eq 'HubApiBaseUrl'}
	$urlNode.value = $hubApiBaseUrl

	Write-Host "Updating connection string" 
	$node = $xml.configuration.connectionStrings.add | where {$_.name -eq 'DockyardDB'}
	$node.connectionString="$connectionString"
   
	try
	{
		$xml.Save($configPath)
		Write-Host "File updated."
	}
	catch
	{
		Write-Host "Exception while saving $($configPath) - $($_.Exception.Message)" 
		exit 1 
	}   
}
else
{
    Write-Error "Configuration file $($configPath) wasn't found."
}

