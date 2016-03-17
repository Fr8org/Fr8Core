param(
	[Parameter(Mandatory = $true)]
	[string]$webHost
)

$rootDir = Split-Path -parent (Split-Path -parent $MyInvocation.MyCommand.Path)
$terminalList = $rootDir+"\fr8terminals.txt"

$terminals = Get-Content $terminalList  |  % { $_ -replace "localhost", $webHost }
Set-Content $terminalList $terminals