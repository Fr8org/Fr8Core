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
$process = Start-Process $GulpProtractorCmd $arguments -NoNewWindow -Wait -PassThru
Exit $process.ExitCode