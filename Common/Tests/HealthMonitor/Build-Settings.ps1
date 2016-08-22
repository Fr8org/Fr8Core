<#
.SYNOPSIS
This scripts collects Hub and terminal settings from respective exterminal configuration files 
(Config/Settings.config) and adds settings from them to the HM external configuration file.
This script is added as a post-build task to HealthMonitor project in the following way: 

powershell.exe –NonInteractive –ExecutionPolicy Unrestricted –command "& { $(ProjectDir)Build-Settings.ps1 }"
#>

$ErrorActionPreference = 'Stop'
$includeNodesToDelete = New-Object System.Collections.ArrayList
$healthMonitorPath = Split-Path -parent $PSCommandPath
$configPath = "$healthMonitorPath\Config\Settings.config.src"
$solutionRootPath = Split-Path -parent (Split-Path -parent $configPath)
$ignoredSettings = @('HubApiVersion', 'TerminalSecret', 'TerminalId', 'owin:AutomaticAppStartup', 'DefaultHubUrl', 'HubApiBaseUrl')
[string[]] $only

if(-not (Test-Path $configPath)) {
	Write-Warning "Cannot find HealthMonitor external configuration file Config/Settings.config. Exiting."
	Exit 1
}
	   
$hmConfigXml = [xml](Get-Content $configPath)
$includeNodes = $hmConfigXml.appSettings.include

ForEach ($curInclude in $includeNodes) {
	# Resovle relative path to absolute path
	$curExtSettingPath =  [System.IO.Path]::GetFullPath((Join-Path $healthMonitorPath $curInclude.src))
	if(Test-Path $curExtSettingPath) {
		if ($curInclude.only) {
			$only = $curInclude.only -split ","
			$only = $only | % { $_.Trim() } # remove spaces
			Echo ("Filter: " + $only | Format-List)
		}
		else {
			$only = @()
		}

		Echo ("Including appSettings from {0}" -f $curExtSettingPath)
		$includeNodesToDelete.Add($curInclude) | Out-Null
		# Get appSettings from the file and copy them to the HM external configuration file.
		$curConfigXml = [xml](Get-Content $curExtSettingPath)
		
		# Get appSettigns section
		$settings = $curConfigXml.appSettings.add # if an incuded settings file
		if (($settings -eq $null) -or ($settings.Count -eq 0))
		{
			$settings = $curConfigXml.configuration.appSettings.add # if a web.config
		}

		ForEach ($curSetting in $settings) {
			# Apply settings filter if enabled
			if ($only.Count -gt 0) {
				if (-not $only.Contains($curSetting.key)) {
					Continue
				}
			}

			# Check if a duplicating setting
			$hmConfigXml.appSettings.add | Where-Object { $_.key -ieq $curSetting.key} | ForEach-Object {
				Write-Warning ("Attempt to add a duplicating setting '{0}' with value '{1}'. Skipping." -f $_.key, $curSetting.value)
				Continue
			}

			# Copy only if setting not ignored 
			If ($ignoredSettings.IndexOf($curSetting.key) -ieq -1) {
				$clonedSetting = $hmConfigXml.ImportNode($curSetting, $false)
				$curInclude.ParentNode.InsertAfter($clonedSetting, $curInclude) | Out-Null
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
	$hmConfigXml.appSettings.RemoveChild($curInclude) | Out-Null
}

# Save without the src extension
$length = $configPath.Length;
$hmConfigXml.Save($configPath.Substring(0, $length-4))