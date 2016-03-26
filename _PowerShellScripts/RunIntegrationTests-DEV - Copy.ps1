param(
	[Parameter(Mandatory = $false)]
	[int]$delay = 90
)

$RootDir = Split-Path -parent $PSCommandPath
$HealthMonitorCmd = "$RootDir\..\Tests\HealthMonitor\bin\Dev\HealthMonitor.exe --app-name VSO-DEV --ensure-startup"
#$SrcConfigFile = "$RootDir\DEV-HealthMonitor.exe.config"
#$DstConfigFile = "$RootDir\..\Tests\HealthMonitor\bin\Dev\HealthMonitor.exe.config"

#Write-Host "Copying HealthMonitor config file"
#Copy-Item $SrcConfigFile -Destination $DstConfigFile -Force

Write-Host $HealthMonitorCmd

Start-Sleep -s $delay
Invoke-Expression $HealthMonitorCmd