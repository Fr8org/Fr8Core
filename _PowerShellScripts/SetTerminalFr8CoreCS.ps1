param(
    $ConnectionString
)

$RootDir = Split-Path -parent (Split-Path -parent $MyInvocation.MyCommand.Path)
$ConfigPath = $RootDir+"\terminalFr8Core"
$WebConfig = $ConfigPath+"\web.config"

if(Test-Path $WebConfig)
{  
   Write-Host "Adding database connection string to $($WebRoleSize)" 
   $xml = [xml](Get-Content $WebConfig)
   $node = $xml.configuration.connectionStrings.add | where {$_.name -eq 'DockyardDB'}
   $node.connectionString=$ConnectionString
   try
   {
     $xml.Save($WebConfig)
     Write-Host "The file $($WebConfig) updated successfully."
   }
   catch
   {
      Write-Host "Exception while saving $($WebConfig) - $($_.Exception.Message)"  
   }
   
}
else
{
    Write-Warning "web.config was not found in this location: $($WebConfig)"
}