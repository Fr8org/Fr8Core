$RootDir = Split-Path -parent $PSCommandPath
$HealthMonitorCmd = "$RootDir\..\Tests\HealthMonitor\bin\Release\HealthMonitor.exe --email-report --app-name VSO-MASTER --ensure-startup --aiik 9db0241b-6f3d-404d-adeb-3839bcb6e3"
$SrcConfigFile = "$RootDir\MASTER-HealthMonitor.exe.config"
$DstConfigFile = "$RootDir\..\Tests\HealthMonitor\bin\Release\HealthMonitor.exe.config"

Write-Host "Copying HealthMonitor config file"
Copy-Item $SrcConfigFile -Destination $DstConfigFile -Force

Write-Host $HealthMonitorCmd
Invoke-Expression $HealthMonitorCmd
