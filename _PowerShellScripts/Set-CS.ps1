param(
	[string]$configs,
    [string]$connectionString,	
	[string]$overrideDbName
)

$ErrorActionPreference = 'Stop'

if ([System.String]::IsNullOrEmpty($overrideDbName) -ne $true) {
	$builder = new-object system.data.SqlClient.SqlConnectionStringBuilder($connectionString)
	$builder["Initial Catalog"] = $overrideDbName
	$connectionString = $builder.ToString()
}

$rootDir = Split-Path -parent (Split-Path -parent $myInvocation.MyCommand.Path)

foreach ($config in $configs.Split(",")) {
	$configPath = "$rootDir\$config"
	if(![System.IO.File]::Exists($configPath))
	{
		# If not a file name was given, assume it is a directory and check configs
		if(Test-Path  "$configPath\web.config")	
		{
			$configPath = "$configPath\web.config"
		}
		elseif (Test-Path  "$configPath\app.config")
		{
			$configPath = "$configPath\app.config"
		}
		else
		{
			Write-Host "Cannot find either app.config or web.config: $configPath"  
			continue
		}
	}

	if(Test-Path $configPath)
	{  
	   Write-Host "Adding database connection string to $configPath" 
	   $xml = [xml](Get-Content $configPath)
	   $node = $xml.configuration.connectionStrings.add | where {$_.name -eq 'Fr8LocalDB'}
	   $node.connectionString="$connectionString"
	   try
	   {
		 $xml.Save($configPath)
		 Write-Host "The file $configPath updated successfully."
	   }
	   catch
	   {
		  Write-Host "Exception while saving $configPath - $_.Exception.Message"  
	   }   
	}
	else
	{
		Write-Warning "Configuration file was not found in this location: $configPath"
	}
}