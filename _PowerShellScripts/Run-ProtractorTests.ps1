param(
	[Parameter(Mandatory = $true)]
	[string]$endPoint,

	[Parameter(Mandatory = $true)]
	[string]$username,

	[Parameter(Mandatory = $true)]
	[string]$password
)

Write-Host 'Using endpoint -> ' $endPoint
$RootDir = Split-Path -parent $PSCommandPath
$projectDirectory = (get-item $RootDir ).parent.FullName
$GulpProtractorCmd = "$projectDirectory\node_modules\.bin\gulp.cmd"
$arguments = "e2etests --baseUrl $endPoint --username $username --password $password"
Write-Host $GulpProtractorCmd $arguments
$process = Start-Process $GulpProtractorCmd $arguments -NoNewWindow -Wait -PassThru
Exit $process.ExitCode