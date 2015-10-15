param(
    [string]$webHost = "local"
)

$terminalList = "..\fr8terminals.txt"

$terminals = Get-Content $terminalList  |  % { $_ -replace "localhost", $webHost }
Set-Content $terminalList $terminals