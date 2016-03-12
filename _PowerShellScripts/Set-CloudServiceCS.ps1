param(
    [Parameter(Mandatory = $true)]
	[string]$connectionString,

    [Parameter(Mandatory = $false)]
	[string]$overrideDbName,

	[Parameter(Mandatory = $false)]
	[string]$serviceName = "fr8company",

	[Parameter(Mandatory = $false)]
	[boolean]$updateService = $false
)

if ([System.String]::IsNullOrEmpty($overrideDbName) -ne $true) {
	$builder = new-object system.data.SqlClient.SqlConnectionStringBuilder($connectionString)
	$builder["Initial Catalog"] = $overrideDbName
	$connectionString = $builder.ToString()
}

$RootDir = Split-Path -parent (Split-Path -parent $MyInvocation.MyCommand.Path)
$ConfigPath = $RootDir+"\terminalCloudService"
$ConfigFile = $ConfigPath+"\ServiceConfiguration.Release.cscfg"

if(Test-Path $ConfigFile)
{  
   Write-Host "Updating connection string in service configuration file: $ConfigFile" 
   $xml = [xml](Get-Content $ConfigFile)
   $node = $xml.ServiceConfiguration.Role | where {$_.name -eq 'terminalWebRole'}
   $node = $node.ConfigurationSettings.Setting | where {$_.name -eq 'Fr8.ConnectionString'}
   $node.value=$connectionString
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
	   try
	   {
		  Set-AzureDeployment -Config -ServiceName $serviceName -Slot "Staging" -Configuration $ConfigFile
		  Write-Host "Azure Cloud Service $serviceName updated"
	   }
	   catch
	   {
		  Write-Host "Exception while updating Cloud Service - $($_.Exception.Message)" 
		  exit 1 
	   } 
   }
}
else
{
    Write-Warning "Azure Service Configuration file wasn't found in the folder $($ConfigFile)"
}

