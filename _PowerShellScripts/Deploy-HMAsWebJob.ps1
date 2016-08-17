param(
    [Parameter(Mandatory = $true)]
	[string]$slot,

    [Parameter(Mandatory = $true)]
	[string]$buildConfiguration
)

$ErrorActionPreference = 'Stop'

$rootDir = Split-Path -parent $PSCommandPath

$archiveFolderName = "$rootDir\HM-WebJob-Archive"
$outputArchiveFile = "$rootDir\HM-azurewebsitejob.zip"

# Check if archive folder exists, remove if it does.
If (Test-Path $archiveFolderName){
	Write-Host "Removing old $archiveFolderName folder"
	Remove-Item $archiveFolderName -Force -Recurse
}

# Check if previous archive file exists, remove if it does.
If (Test-Path $outputArchiveFile){
	Write-Host "Removing old $outputArchiveFile archive file"
	Remove-Item $outputArchiveFile -Force -Recurse
} 

# Create archive folder.
New-Item -ItemType Directory -Force -Path $archiveFolderName | Out-Null
# Create SQL folder
New-Item -ItemType Directory -Force -Path $archiveFolderName\SQL | Out-Null
# Create Config\HealthMonitor folder
New-Item -ItemType Directory -Force -Path $archiveFolderName\Config | Out-Null
New-Item -ItemType Directory -Force -Path $archiveFolderName\Config\HealthMonitor | Out-Null

# Copy HealthMonitor DLLs to archive folder.
$srcFiles = "$rootDir\..\Tests\HealthMonitor\bin\$($buildConfiguration)\*"
$exclude = @("HealthMonitor.vshost.exe", "HealthMonitor.vshost.exe.config", "HealthMonitor.vshost.exe.manifest")
Copy-Item $srcFiles -Destination $archiveFolderName -Exclude $exclude -Force -Recurse

# Copy settings.job to archive folder.
$srcSettingsFile = "$rootDir\HM-job-settings.job"
$dstSettingsFile = "$archiveFolderName\settings.job"
Copy-Item $srcSettingsFile -Destination $dstSettingsFile -Force

# Copy run.cmd to archive folder
$srcRunFile = "$rootDir\HM-job-run.cmd"
$dstRunFile = "$archiveFolderName\run.cmd"
Copy-Item $srcRunFile -Destination $dstRunFile -Force

# Copy SQL folder
$srcRunFile = "$rootDir\..\Tests\HealthMonitor\SQL\*"
$dstRunFile = "$archiveFolderName\SQL\"
Copy-Item $srcRunFile -Destination $dstRunFile -Force

# Copy Config
$srcRunFile = "$rootDir\..\Tests\HealthMonitor\Config\*"
$dstRunFile = "$archiveFolderName\Config\"
Copy-Item $srcRunFile -Destination $dstRunFile -Force

# Fix the file argument 
$configFile = $archiveFolderName + "\HealthMonitor.exe.config"
$xml = [xml](Get-Content ($configFile))
$node = $xml.configuration.appSettings
if ($node -ne $NULL)
{
	$node.file = "Config\Settings.config"
	$xml.Save($configFile)
}		

# Copy PowerShell script
$srcRunFile = "$rootDir\CleanUpAfterTests.ps1"
$dstRunFile = "$archiveFolderName\"
Copy-Item $srcRunFile -Destination $dstRunFile -Force

# Create zip archive.
$archiveFiles = "$archiveFolderName\*"
Compress-Archive -Path $archiveFiles -DestinationPath $outputArchiveFile -Force

# Create zip archive.
$archiveFiles = "$archiveFolderName\*"
Compress-Archive -Path $archiveFiles -DestinationPath $outputArchiveFile -Force

# Deploy AzureWebsiteJob.
Write-Host "Deploying Azure WebJob"
$site = Get-AzureWebsite -Name "fr8" -Slot $slot
# $site = Get-AzureWebsite -Name "fr8dev"

New-AzureWebsiteJob -Name $site[0].Name `
  -JobName "HealthMonitor" `
  -JobType Triggered `
  -JobFile $outputArchiveFile `
  -Slot $slot;

# Remove zip archive.
# Write-Host "Removing current deployment zip archive"
# Remove-Item $outputArchiveFile -Force -Recurse

# Remove current archive folder.
Remove-Item $archiveFolderName -Force -Recurse