param(
	[Parameter(Mandatory = $true)]
	[string]$buildConfiguration,

	[Parameter(Mandatory = $false)]
	[int]$delay = 90
)

$RootDir = Split-Path -parent $PSCommandPath
$HealthMonitorCmd = "$RootDir\..\Tests\HealthMonitor\bin\$buildConfiguration\HealthMonitor.exe --app-name VSO-$($buildConfiguration.ToUpper()) --ensure-startup"
Write-Host $HealthMonitorCmd

Start-Sleep -s $delay
Invoke-Expression $HealthMonitorCmd