param(
	[Parameter(Mandatory = $true)]
	[string]$webHost
)

$rootDir = Split-Path -parent (Split-Path -parent $MyInvocation.MyCommand.Path)
$terminalList = $rootDir+"\fr8terminals.txt"
Write-Host "Updating $terminalList"
$terminals = Get-Content $terminalList  |  % { $_ -replace ($_.Substring(0, $_.IndexOf(":"))), $webHost }
Set-Content $terminalList $terminals