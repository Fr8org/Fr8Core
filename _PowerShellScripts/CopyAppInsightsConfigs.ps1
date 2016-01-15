$RootDir = Split-Path -parent (Split-Path -parent $MyInvocation.MyCommand.Path)
$FindPath = $RootDir+"\*\bin\*"

$configs = Get-ChildItem -Path $FindPath -Filter ApplicationInsights.config
foreach ($config in $configs)
{
    [string]$TargetDir = $config.Directory
    $TargetDir = $TargetDir.Remove(($TargetDir.Length-3),3)
    Copy-Item -Path $config.FullName -Destination $TargetDir -Force -Verbose
}