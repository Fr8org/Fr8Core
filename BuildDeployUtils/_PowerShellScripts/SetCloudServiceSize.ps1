<#
    .SYNOPSIS
    Script takes the WebRoleSize and replaces with it the vmsize attribute value in ServiceDefinition.csdef file located in terminalCloudService folder

    .NOTES
    22 - Jan - 2015 - Andrii Ivanskiy - Script was created.

    .INPUTS

    .OUPUTS

    .EXAMPLE
    .\SetCloudServiceSize.ps1 - will set Cloud Serivce size with default - "Small"
    .\SetCloudServiceSize.ps1 -WebRoleSize Large - will set Cloud Serivce size to Large


#>
param(
    [ValidateSet("ExtraSmall","Small","Medium","Large","ExtraLarge")]
    $WebRoleSize = "Small" 
)

$RootDir = Split-Path -parent (Split-Path -parent $MyInvocation.MyCommand.Path)
$ConfigPath = $RootDir+"\terminalCloudService"
$ServiceDefFile = $ConfigPath+"\ServiceDefinition.csdef"

if(Test-Path $ServiceDefFile)
{  
   Write-Host "Setting cloud service instance size to $($WebRoleSize)" 
   $xml = [xml](Get-Content $ServiceDefFile)
   $node = $xml.ServiceDefinition.WebRole | where {$_.name -eq 'terminalWebRole'}
   $node.vmsize=$WebRoleSize
   try
   {
     $xml.Save($ServiceDefFile)
     Write-Host "Done! File $($ServiceDefFile) amended!"
   }
   catch
   {
      Write-Host "Exception while saving $($ServiceDefFile) - $($_.Exception.Message)"  
   }
   
}
else
{
    Write-Warning "Azure Service Definition file wasn't found in path: $($ServiceDefFile)"
}