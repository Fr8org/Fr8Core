param(
    [Parameter(Mandatory = $true)]
	[string]$confirmation,
	[string]$numOfChecks = 11
)

if (($confirmation -match $numOfChecks) -and ($confirmation -match "check") -and ($confirmation -match "passed") -and ($confirmation -match "confirm"))
{
	Write-Host "Build Initiator ($env:BUILD_QUEUEDBY) confirmed that all $numOfChecks pre-deployment checks have passed."
	exit 0
}
else
{
	Write-Error "Build Initiator ($env:BUILD_QUEUEDBY) did not confirm that all pre-deployment checks have passed. The Build&Deploy process is terminated."
	exit 1
}