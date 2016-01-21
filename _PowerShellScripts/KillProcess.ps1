$RootDir = Split-Path -parent (Split-Path -parent $MyInvocation.MyCommand.Path)
$Path= $RootDir+"\*\obj\Release"

$files = Get-ChildItem $Path -Recurse | where {$_.extension -eq ".resources" -OR $_.Extension -eq ".dll"}
foreach($file in $files)
{
Get-Process | foreach{$processVar = $_;$_.Modules | foreach{if($_.FileName -eq $file)
{
    $processVar.Name + " PID:" + $processVar.id
    Stop-Process -id $processVar.Id -passThru -Force -Verbose
}
}}
}
