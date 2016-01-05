$RootDir = Split-Path -parent $PSCommandPath
$HealthMonitorCmd = "$RootDir\..\Tests\HealthMonitor\bin\Debug\HealthMonitor.exe --email-report --app-name VSO-DEV"
$SrcConfigFile = "$RootDir\DEV-HealthMonitor.exe.config"
$DstConfigFile = "$RootDir\..\Tests\HealthMonitor\bin\Debug\HealthMonitor.exe.config"

Write-Host "Copying HealthMonitor config file"
Copy-Item $SrcConfigFile -Destination $DstConfigFile -Force

Write-Host $HealthMonitorCmd

Start-Sleep -s 90
Invoke-Expression $HealthMonitorCmd
