$RootDir = Split-Path -parent (Split-Path -parent $MyInvocation.MyCommand.Path)
$lockedFile= $RootDir+"\Utilities\obj\Release\Utilities.dll"
Get-Process | foreach{$processVar = $_;$_.Modules | foreach{if($_.FileName -eq $lockedFile)
{
    $processVar.Name + " PID:" + $processVar.id
    Stop-Process -id $processVar.Id -passThru -Force -Verbose
}
}}