param(
	[string]$hubApiBaseUrl,

    [string]$connectionString,	

	[string]$overrideDbName,

	[Parameter(Mandatory = $true)]
	[string]$filePath,

	[string]$keyVaultClientSecret,

	[string]$keyVaultClientId,

	[string]$keyVaultUrl
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
   
	if (-not([String]::IsNullOrEmpty($hubApiBaseUrl)) )
	{
		Write-Host "Updating HubApiBaseUrl value" 
		$urlNode = $xml.configuration.appSettings.add | where {$_.key -eq 'HubApiBaseUrl'}
		$urlNode.value = $hubApiBaseUrl
	}

	if (-not([String]::IsNullOrEmpty($keyVaultClientSecret)) )
	{
		Write-Host "Updating KeyVault Secret" 
		$kvNode = $xml.configuration.appSettings.add | where {$_.key -eq 'KeyVaultClientSecret'}
		$kvNode.value="$keyVaultClientSecret"	
	}

	if (-not([String]::IsNullOrEmpty($keyVaultClientId)) )
	{
		Write-Host "Updating KeyVault Id" 
		$kvNode = $xml.configuration.appSettings.add | where {$_.key -eq 'KeyVaultClientId'}
		$kvNode.value="$keyVaultClientId"	
	}

	if (-not([String]::IsNullOrEmpty($keyVaultUrl)) )
	{
		Write-Host "Updating KeyVault URL" 
		$kvNode = $xml.configuration.appSettings.add | where {$_.key -eq 'KeyVaultUrl'}
		$kvNode.value="$keyVaultUrl"	
	}
	
	if (-not([String]::IsNullOrEmpty($connectionString)) )
	{
		Write-Host "Updating connection string" 
		$node = $xml.configuration.connectionStrings.add | where {$_.name -eq 'DockyardDB'}
		$node.connectionString="$connectionString"
	}

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

