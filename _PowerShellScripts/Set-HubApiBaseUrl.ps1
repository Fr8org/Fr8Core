param(
	[Parameter(Mandatory = $true)]
	[string]$hubApiBaseUrl,

	[Parameter(Mandatory = $true)]
	[string]$filePath
)

$rootDir = Split-Path -parent (Split-Path -parent $MyInvocation.MyCommand.Path)
$configPath = [System.IO.Path]::Combine($rootDir, $filePath)

if(Test-Path $configPath)
{  
   Write-Host "Updating HubApiBaseUrl value in the file: $configPath" 
   $xml = [xml](Get-Content $configPath)
   $urlNode = $xml.configuration.appSettings.add | where {$_.key -eq 'HubApiBaseUrl'}
   $urlNode.value = $hubApiBaseUrl
   
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

