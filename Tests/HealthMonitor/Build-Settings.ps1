<#
.SYNOPSIS
This scripts collects Hub and terminal settings from respective exterminal configuration files 
(Config/Settings.config) and adds settings from them to the HM external configuration file.
This script is added as a post-build task to HealthMonitor project in the following way: 

powershell.exe –NonInteractive –ExecutionPolicy Unrestricted –command "& { $(ProjectDir)BuildSettings.ps1 }"
#>

$ErrorActionPreference = 'Stop'
$includeNodesToDelete = New-Object System.Collections.ArrayList
$healthMonitorPath = Split-Path -parent $PSCommandPath
$configPath = "$healthMonitorPath\Config\Settings.config"
$solutionRootPath = Split-Path -parent (Split-Path -parent $configPath)
$ignoredSettings = @('HubApiVersion', 'TerminalSecret', 'TerminalId', 'owin:AutomaticAppStartup', 'CoreWebServerUrl')


if(-not (Test-Path $configPath)) {
	Write-Warning "Cannot find HealthMonitor external configuration file Config/Settings.config. Exiting."
	Exit 1
}
	   
$hmConfigXml = [xml](Get-Content $configPath)
$includeNodes = $hmConfigXml.appSettings.include

ForEach ($curInclude in $includeNodes) {
	# Resovle relative path to absolute path
	$curExtSettingPath =  [System.IO.Path]::GetFullPath((Join-Path (Split-Path -parent $configPath) $curInclude.src))
	if(Test-Path $curExtSettingPath) {
		Echo ("Including appSettings from {0}" -f $curExtSettingPath)
		$includeNodesToDelete.Add($curInclude);
		# Get appSettings from the file and copy them to the HM external configuration file.
		$curConfigXml = [xml](Get-Content $curExtSettingPath)
		ForEach ($curSetting in $curConfigXml.appSettings.add) {
			# Check if duplicating setting
			$hmConfigXml.appSettings | Where-Object { $_.key -ieq $curSetting.key} | ForEach-Object {
				Echo "Warning: attempt to add a duplicating setting $curSetting.key with value $curSetting.value. Skipping."
				Continue
			}

			If ($ignoredSettings.IndexOf($curSetting.key) -ieq -1) { # copy if not ignored 
				$clonedSetting = $hmConfigXml.ImportNode($curSetting, $false)
				$hmConfigXml.appSettings.AppendChild($clonedSetting)
			}
		}
	}
	else
	{
		Write-Warning ("Cannot find external configuration file {0}. Skipping." -f $curExtSettingPath)
	}
}

# Delete include nodes
ForEach ($curInclude in $includeNodesToDelete) {
	$hmConfigXml.appSettings.RemoveChild($curInclude)
}

$hmConfigXml.Save($configPath)