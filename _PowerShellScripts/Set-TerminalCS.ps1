param(
	[string]$terminals,
    [string]$connectionString,	
	[string]$overrideDbName
)

if ([System.String]::IsNullOrEmpty($overrideDbName) -ne $true) {
	$builder = new-object system.data.SqlClient.SqlConnectionStringBuilder($connectionString)
	$builder["Initial Catalog"] = $overrideDbName
	$connectionString = $builder.ToString()
}

$rootDir = Split-Path -parent (Split-Path -parent $myInvocation.MyCommand.Path)

foreach ($terminal in $terminals.Split(",")) {
	$configPath = "$rootDir\$terminal"
	$webConfig = $configPath+"\web.config"

	if(Test-Path $webConfig)
	{  
	   Write-Host "Adding database connection string to $($terminal)" 
	   $xml = [xml](Get-Content $webConfig)
	   $node = $xml.configuration.connectionStrings.add | where {$_.name -eq 'DockyardDB'}
	   $node.connectionString="$connectionString"
	   try
	   {
		 $xml.Save($webConfig)
		 Write-Host "The file $($webConfig) updated successfully."
	   }
	   catch
	   {
		  Write-Host "Exception while saving $($webConfig) - $($_.Exception.Message)"  
	   }
   
	}
	else
	{
		Write-Warning "web.config was not found in this location: $($webConfig)"
	}
}