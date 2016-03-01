$rootDir = Split-Path -parent $PSCommandPath

$archiveFolderName = "$rootDir\Master-WebJob-Archive"
$outputArchiveFile = "$rootDir\master-azurewebsitejob.zip"

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
Write-Host "Creating current $archiveFolderName folder"
New-Item -ItemType Directory -Force -Path $archiveFolderName | Out-Null

# Copy HealthMonitor DLLs to archive folder.
Write-Host "Copying HealthMonitor executable & DLLs"
$srcFiles = "$rootDir\..\Tests\HealthMonitor\bin\Debug\*"
$exclude = @("HealthMonitor.vshost.exe", "HealthMonitor.vshost.exe.config", "HealthMonitor.vshost.exe.manifest")
Copy-Item $srcFiles -Destination $archiveFolderName -Exclude $exclude -Force -Recurse

# Copy HealthMonitor config-file to archive folder.
Write-Host "Copying HealthMonitor config file"
$srcConfigFile = "$rootDir\MASTER-HealthMonitor.exe.config"
$dstConfigFile = "$archiveFolderName\HealthMonitor.exe.config"
Copy-Item $srcConfigFile -Destination $dstConfigFile -Force

# Copy settings.job to archive folder.
Write-Host "Copying HealthMonitor settings.job file"
$srcSettingsFile = "$rootDir\MASTER-settings.job"
$dstSettingsFile = "$archiveFolderName\settings.job"
Copy-Item $srcSettingsFile -Destination $dstSettingsFile -Force

# Copy run.cmd to archive folder
Write-Host "Copying run.cmd file"
$srcRunFile = "$rootDir\MASTER-job-run.cmd"
$dstRunFile = "$archiveFolderName\run.cmd"
Copy-Item $srcRunFile -Destination $dstRunFile -Force

# Create zip archive.
Write-Host "Creating Job ZIP-archive"
$archiveFiles = "$archiveFolderName\*"
Compress-Archive -Path $archiveFiles -DestinationPath $outputArchiveFile -Force

# Deploy AzureWebsiteJob.
Write-Host "Deploying AzureWebsiteJob"
$site = Get-AzureWebsite -Name "fr8" -Slot "production"
# $site = Get-AzureWebsite -Name "fr8dev"

New-AzureWebsiteJob -Name $site[0].Name `
  -JobName "HealthMonitor-Master" `
  -JobType Triggered `
  -JobFile $outputArchiveFile `
  -Slot Staging;

# New-AzureWebsiteJob -Name $site.Name `
#   -JobName "HealthMonitor-Continuous" `
#   -JobType triggered `
#   -JobFile $outputArchiveFile;


# Remove zip archive.
# Write-Host "Removing current deployment zip archive"
# Remove-Item $outputArchiveFile -Force -Recurse

# Remove current archive folder.
Write-Host "Removing current Master-WebJob-Archive folder"
Remove-Item $archiveFolderName -Force -Recurse
