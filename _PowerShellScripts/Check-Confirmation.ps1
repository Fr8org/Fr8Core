param(
    [Parameter(Mandatory = $true)]
	[string]$confirmation
)

if (($confirmation -contains "11") -and ($confirmation -contains "check") -and ($confirmation -contains "passed") -and ($confirmation -contains "confirm"))
{
	Write-Host "Build Initiator (($env:BUILD_QUEUEDBY)) confirmed that all 11 pre-deployment checks have passed."
	exit 0
}
else
{
	Write-Error "Build Initiator (($env:BUILD_QUEUEDBY)) did not confirm that pre-deployment checks have passed. The Build&Deploy process is terminated."
	exit 1
}