param(
	[Parameter(Mandatory = $true)]
	[string]$endPoint
)

$RootDir = Split-Path -parent $PSCommandPath
$GulpProtractorCmd = "$RootDir\..\node_modules\.bin\gulp e2etests --baseUrl $endPoint"
Write-Host $GulpProtractorCmd

Invoke-Expression $GulpProtractorCmd | Out-Null