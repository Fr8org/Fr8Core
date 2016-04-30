param(
	[Parameter(Mandatory = $true)]
	[string]$endPoint
)

Write-Host 'Using endpoint -> ' $endPoint
$RootDir = Split-Path -parent $PSCommandPath
$projectDirectory = (get-item $RootDir ).parent.FullName
$GulpProtractorCmd = "$projectDirectory\node_modules\.bin\gulp.cmd"
$arguments = "e2etests --baseUrl $endPoint"
Write-Host $GulpProtractorCmd $arguments

#Invoke-Expression $GulpProtractorCmd | Out-Null

Start-Process $GulpProtractorCmd $arguments -NoNewWindow -Wait