param(
    [Parameter(Mandatory = $true)]
	[string]$confirmation,
	[string]$numOfChecks = 11
)

if (($confirmation -contains $numOfChecks) -and ($confirmation -contains "check") -and ($confirmation -contains "passed") -and ($confirmation -contains "confirm"))
{
	Write-Host "Build Initiator ($env:BUILD_QUEUEDBY) confirmed that all $numOfChecks pre-deployment checks have passed."
	exit 0
}
else
{
	Write-Error "Build Initiator ($env:BUILD_QUEUEDBY) did not confirm that all pre-deployment checks have passed. The Build&Deploy process is terminated."
	exit 1
}